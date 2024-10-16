using TermsApp.Entities;
using TermsApp.Repository;
using Plugin.LocalNotification;

namespace TermsApp
{
    public partial class MainPage : ContentPage
    {
        private static Term? selectedTerm;
        private static List<Term> terms = new List<Term>();
        public static Dictionary<Term, List<Course>> courses = new Dictionary<Term, List<Course>>();
        public static Dictionary<int, Course> courseList = new Dictionary<int, Course>();
        public static Dictionary<int, Instructor> instructors = new Dictionary<int, Instructor>();
        public static IList<NotificationRequest> notificationRequests = new List<NotificationRequest>();


        public MainPage()
        {
            InitializeComponent();
            DBClient.CreateTables();
            DBClient.SeedData();
            LoadDataOnUI(1);
            syncUIFields();
        }

        private async void HandleNotifications()
        {
            await RequestNotificationPermissions();

            var requests = new List<NotificationRequest>();
            var cancelledRequests = new List<int>();
            DateTime currentDateTime = DateTime.Now;

            ProcessCourses(courses.Values, requests, cancelledRequests, currentDateTime);
            ProcessAssessments(AssessmentRepo.GetAll(), requests, cancelledRequests, currentDateTime);

            CancelNotifications(cancelledRequests, notificationRequests.ToList());
            await ShowNotifications(requests);
        }

        private async Task RequestNotificationPermissions()
        {
            notificationRequests = await LocalNotificationCenter.Current.GetPendingNotificationList();
            if (!await LocalNotificationCenter.Current.AreNotificationsEnabled())
            {
                await LocalNotificationCenter.Current.RequestNotificationPermission();
            }
        }

        private void ProcessCourses(IEnumerable<List<Course>> courseLists, List<NotificationRequest> requests, List<int> cancelledRequests, DateTime currentDateTime)
        {
            foreach (var listCourse in courseLists)
            {
                foreach (var course in listCourse)
                {
                    AddCourseNotifications(course, requests, cancelledRequests, currentDateTime);
                }
            }
        }

        private void AddCourseNotifications(Course course, List<NotificationRequest> requests, List<int> cancelledRequests, DateTime currentDateTime)
        {
            if (course.StartNotification == 0)
            {
                cancelledRequests.Add(course.Id + 1000);
            }
            else
            {
                requests.Add(CreateNotificationRequest(course.Id + 1000, "Course Starting Reminder", course.Name, course.StartDate, course.StartNotification, currentDateTime));
            }

            if (course.EndNotification == 0)
            {
                cancelledRequests.Add(course.Id + 2000);
            }
            else
            {
                requests.Add(CreateNotificationRequest(course.Id + 2000, "Course Ending Reminder", course.Name, course.EndDate, course.EndNotification, currentDateTime));
            }
        }

        private void ProcessAssessments(IEnumerable<Assessment> assessments, List<NotificationRequest> requests, List<int> cancelledRequests, DateTime currentDateTime)
        {
            foreach (var assessment in assessments)
            {
                AddAssessmentNotifications(assessment, requests, cancelledRequests, currentDateTime);
            }
        }

        private void AddAssessmentNotifications(Assessment assessment, List<NotificationRequest> requests, List<int> cancelledRequests, DateTime currentDateTime)
        {
            if (assessment.StartNotification == 0)
            {
                cancelledRequests.Add(assessment.Id + 3000);
            }
            else
            {
                requests.Add(CreateNotificationRequest(assessment.Id + 3000, "Assessment Starting Reminder", assessment.Name, assessment.StartDate, assessment.StartNotification, currentDateTime));
            }

            if (assessment.EndNotification == 0)
            {
                cancelledRequests.Add(assessment.Id + 4000);
            }
            else
            {
                requests.Add(CreateNotificationRequest(assessment.Id + 4000, "Assessment Ending Reminder", assessment.Name, assessment.EndDate, assessment.EndNotification, currentDateTime));
            }
        }

        private NotificationRequest CreateNotificationRequest(int notificationId, string title, string description, DateTime date, int notificationDaysBefore, DateTime currentDateTime)
        {
            return new NotificationRequest
            {
                NotificationId = notificationId,
                Title = title,
                Description = description + " Starting soon",
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = date.AddDays(-notificationDaysBefore).AddHours(currentDateTime.Hour).AddMinutes(currentDateTime.Minute + 1),
                    RepeatType = NotificationRepeat.Daily
                }
            };
        }

        private void CancelNotifications(List<int> cancelledRequests, List<NotificationRequest> notificationRequests)
        {
            foreach (var requestId in cancelledRequests)
            {
                var notification = notificationRequests.FirstOrDefault(n => n.NotificationId == requestId);
                notification?.Cancel();
            }
        }

        private async Task ShowNotifications(List<NotificationRequest> requests)
        {
            foreach (var request in requests)
            {
                await LocalNotificationCenter.Current.Show(request);
            }
        }

        protected override void OnAppearing()
        {
            if(selectedTerm != null)
            {
                LoadDataOnUI(selectedTerm.Id);
            }

            HandleNotifications();
        }

        private void LoadDBData()
        {
            terms.Clear();
            terms = TermsRepo.GetAll();

            foreach (Term term in terms)
            {
                var coursesByTerms = CoursesRepo.GetAllByTerm(term.Id);
                List<Course> CourseList = new List<Course>();
                foreach (Course course in coursesByTerms)
                {
                    CourseList.Add(course);
                    if (!courseList.ContainsKey(course.Id))
                    {
                        courseList.Add(course.Id, course);
                    }
                }
                courses.Add(term, CourseList);
            }

            var allInstructors = InstructorsRepo.GetAll();
            foreach (Instructor instructor in allInstructors)
            {
                if (!instructors.ContainsKey(instructor.Id))
                {
                    instructors.Add(instructor.Id, instructor);
                }
            }
        }

        private void LoadDataOnUI(int termPosition)
        {
            LoadTermsUIData(termPosition);
            LoadCoursesUIData();
        }

        private void LoadCoursesUIData()
        {
            courseStack.Children.Clear();
            if (selectedTerm == null) return;

            foreach (Course course in courses[selectedTerm])
            {
                Grid grid = new Grid
                {
                    BackgroundColor = Colors.White
                };
                Button button = new Button
                {
                    Text = course.Name,
                    BackgroundColor = Color.FromArgb("#1b7d2d"),
                };
                button.Clicked += async (sender, args) => await Navigation.PushAsync(new CoursePage(course.Id));

                grid.Add(button);

                SwipeView swp = new SwipeView
                {
                    Content = grid
                };
                courseStack.Children.Add(swp);
            }
            if (courses[selectedTerm].Count < 6)
            {
                Button buttonCourseAdd = new Button()
                {
                    Text = "Add Course",
                    BackgroundColor = Color.FromArgb("#1b7d2d"),
                };
                buttonCourseAdd.Clicked += void (sender, args) => onNewCourse();
                courseStack.Children.Add(buttonCourseAdd);
            }
            
        }

        private void onCourseDelete(object sender, EventArgs e)
        {
            var item = sender as SwipeItem;
            var course = item.BindingContext as Course;
            SyncDatabaseFields();
            if (selectedTerm != null)
            {
                LoadDataOnUI(selectedTerm.Id);
            }
        }

        public void onNewCourse()
        {
            if (selectedTerm == null) return;

            CoursesRepo.AddNew(selectedTerm.Id);
            LoadDataOnUI(selectedTerm.Id);
        }

        private void termTitle_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.NewTextValue != null && selectedTerm != null)
            {
                termTitle.Text = e.NewTextValue;
                selectedTerm.Name = e.NewTextValue;
                TermsRepo.Update(selectedTerm);
                LoadDataOnUI(selectedTerm.Id);
            }
        }

        private void termStart_DateSelected(object sender, DateChangedEventArgs e)
        {
            var isValid = validateDates();
            if (isValid && (selectedTerm != null))
            {
                termStart.Date = e.NewDate;
                selectedTerm.StartDate = e.NewDate;
                TermsRepo.Update(selectedTerm);
                LoadDataOnUI(selectedTerm.Id);
            }
        }

        private void termEnd_DateSelected(object sender, DateChangedEventArgs e)
        {
            var isValid = validateDates();
            if (isValid && (selectedTerm != null))
            {
                termEnd.Date = e.NewDate;
                selectedTerm.EndDate = e.NewDate;
                TermsRepo.Update(selectedTerm);
                LoadDataOnUI(selectedTerm.Id);
            }
        }
        private void onNewTerm()
        {
            TermsRepo.AddNew();
            if (selectedTerm != null) { LoadDataOnUI(selectedTerm.Id); }
        }

        private void LoadTermsUIData(int termPosition)
        {
            termStack.Children.Clear();
            LoadDBData();

            if (terms.Count > 0) 
            {
                selectedTerm = terms[termPosition - 1];
                validateDates();
            }
            else
            {
                selectedTerm = null;
            }

            foreach (Term term in terms)
            {
                Button button = new Button
                {
                    Text = term.Name,
                    Padding = 5,
                    BackgroundColor = Colors.LightGreen,
                    TextColor = Colors.Black,
                    CornerRadius = 5,
                };
                button.Clicked += void (sender, args) => {
                    LoadDataOnUI(term.Id);
                    syncUIFields();
                };
                termStack.Children.Add(button);
            }

            Button buttonTermAdd = new Button()
            {
                Text = "Add Term",
                Padding = 5,
                BackgroundColor = Colors.LightGreen,
                TextColor = Colors.Black,
                CornerRadius = 5,
            };
            buttonTermAdd.Clicked += void (sender, args) => onNewTerm();
            termStack.Children.Add(buttonTermAdd);
        }

        private bool validateDates()
        {
            if (termEnd.Date < termStart.Date)
            {
                startDateError.IsVisible = true;
                return false;
            }

            startDateError.IsVisible = false;
            return true;
        }

        private void syncUIFields()
        {
            if (selectedTerm == null) return;

            termTitle.Text = selectedTerm.Name;
            termStart.Date = selectedTerm.StartDate;
            termEnd.Date = selectedTerm.EndDate;
        }

        public static void SyncDatabaseFields()
        {
            terms = new List<Term>();
            courses = new Dictionary<Term, List<Course>>();
            courseList = new Dictionary<int, Course>();
            terms = TermsRepo.GetAll();

            foreach (Term term in terms)
            {
                var coursesByTerm = CoursesRepo.GetAllByTerm(term.Id);
                List<Course> CourseList = new List<Course>();
                foreach (Course course in coursesByTerm)
                {
                    CourseList.Add(course);
                    courseList.Add(course.Id, course);
                }
                courses.Add(term, CourseList);
            }
        }
    }

}