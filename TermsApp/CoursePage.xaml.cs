using System.Text.RegularExpressions;
using TermsApp.Entities;
using TermsApp.Repository;

namespace TermsApp;

public partial class CoursePage : ContentPage
{
    private Course currentCourse;
    private Instructor currentInstructor;
    public Assessment PA, OA;

    public CoursePage(int courseId)
    {
        InitializeComponent();

        LoadCourseDetails(courseId);
        LoadInstructorDetails(currentCourse.InstructorId);
        LoadNotificationSettings();
        LoadAssessments();
        LoadNotesOnUI();
    }

    private void LoadCourseDetails(int courseId)
    {
        Course course = MainPage.courseList[courseId];
        currentCourse = course;

        courseTitle.Text = course.Name;
        courseStart.Date = course.StartDate;
        courseEnd.Date = course.EndDate;

        courseStatus.ItemsSource = new List<string>
    {
        "In Progress",
        "Completed",
        "Dropped",
        "Plan to Take",
    };
        courseStatus.SelectedItem = course.Status;
        courseDetails.Text = course.Details;
    }

    private void LoadInstructorDetails(int instructorId)
    {
        currentInstructor = MainPage.instructors[instructorId];

        instructorName.Text = currentInstructor.Name;
        instructorPhone.Text = currentInstructor.Phone;
        instructorEmail.Text = currentInstructor.Email;
    }

    private void LoadAssessments()
    {
        PA = AssessmentRepo.GetByCourse(currentCourse.Id)
            .FirstOrDefault(assessment => assessment.Type == 1) ?? new Assessment();
        OA = AssessmentRepo.GetByCourse(currentCourse.Id)
            .FirstOrDefault(assessment => assessment.Type == 0) ?? new Assessment();

        LoadAssessmentDetails(PA, paName, paDueDate, paStart, paEnd, paStartNotif, paEndNotif);
        LoadAssessmentDetails(OA, oaName, oaDueDate, oaStart, oaEnd, oaStartNotif, oaEndNotif);
    }

    private void LoadAssessmentDetails(Assessment assessment, Entry nameEntry, DatePicker dueDatePicker, DatePicker startDatePicker, DatePicker endDatePicker, Picker startNotifPicker, Picker endNotifPicker)
    {
        nameEntry.Text = assessment.Name;
        dueDatePicker.Date = assessment.DueDate;
        startDatePicker.Date = assessment.StartDate;
        endDatePicker.Date = assessment.EndDate;
        startNotifPicker.SelectedIndex = assessment.StartNotification;
        endNotifPicker.SelectedIndex = assessment.EndNotification;
    }

    private void LoadNotificationSettings()
    {
        List<int> notificationValues = new List<int> { 0, 1, 2, 3, 5, 7, 14 };

        paStartNotif.ItemsSource = notificationValues;
        paEndNotif.ItemsSource = notificationValues;
        oaStartNotif.ItemsSource = notificationValues;
        oaEndNotif.ItemsSource = notificationValues;
        courseStartNotif.ItemsSource = notificationValues;
        courseEndNotif.ItemsSource = notificationValues;

        courseStartNotif.SelectedIndex = currentCourse.StartNotification;
        courseEndNotif.SelectedIndex = currentCourse.EndNotification;
    }

    private void LoadNotesOnUI()
    {
        noteStack.Children.Clear();

        var notes = NotesRepo.GetByCourse(currentCourse.Id);
        foreach (Note note in notes)
        {
            if (note.CourseId == currentCourse.Id)
            {
                SwipeView swipeView = CreateSwipeViewForNote(note);
                noteStack.Add(swipeView);
            }
        }
    }

    private SwipeView CreateSwipeViewForNote(Note note)
    {
        SwipeItem shareItem = CreateShareItem(note);
        Grid noteGrid = CreateNoteGrid(note);

        return new SwipeView
        {
            RightItems = new SwipeItems { shareItem },
            Content = noteGrid
        };
    }

    private SwipeItem CreateShareItem(Note note)
    {
        var shareItem = new SwipeItem
        {
            Text = "Share",
            BindingContext = note,
            BackgroundColor = Colors.LightGreen
        };
        shareItem.Invoked += onShareInvoked;
        return shareItem;
    }

    private Grid CreateNoteGrid(Note note)
    {
        var grid = new Grid
        {
            BackgroundColor = Colors.LightGreen,
            ColumnDefinitions =
        {
            new ColumnDefinition { Width = GridLength.Star },
            new ColumnDefinition { Width = GridLength.Auto }
        }
        };

        grid.Add(new Label
        {
            Text = note.Content,
        }, 0, 0);

        grid.Add(new Label
        {
            Text = "(Swipe to share)",
            HorizontalOptions = LayoutOptions.End
        }, 1, 0);

        return grid;
    }

    private async void onShareInvoked(object sender, EventArgs e)
    {
        var item = sender as SwipeItem;
        if (item?.BindingContext is Note note)
        {
            await ShareText(note.Content);
        }
    }

    public async Task ShareText(string text)
    {
        await Share.Default.RequestAsync(new ShareTextRequest { Text = text });
    }

    private void courseTitle_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateCourseField(e.NewTextValue, nameof(currentCourse.Name));
    }

    private void UpdateCourseField(string newValue, string propertyName)
    {
        if (!string.IsNullOrEmpty(newValue))
        {
            switch (propertyName)
            {
                case nameof(currentCourse.Name):
                    currentCourse.Name = newValue;
                    break;
            }

            CoursesRepo.Update(currentCourse);
            MainPage.SyncDatabaseFields();
        }
    }

    private void courseStart_DateSelected(object sender, DateChangedEventArgs e)
    {
        UpdateCourseDate(e.NewDate, true);
    }

    private void courseEnd_DateSelected(object sender, DateChangedEventArgs e)
    {
        UpdateCourseDate(e.NewDate, false);
    }

    private void UpdateCourseDate(DateTime newDate, bool isStartDate)
    {
        if (isStartDate)
        {
            currentCourse.StartDate = newDate;
        }
        else
        {
            currentCourse.EndDate = newDate;
        }

        if (ValidateDates())
        {
            CoursesRepo.Update(currentCourse);
            MainPage.SyncDatabaseFields();
        }
    }

    private void courseStatus_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateCourseStatus(courseStatus.SelectedItem as string);
    }

    private void UpdateCourseStatus(string newStatus)
    {
        currentCourse.Status = newStatus;
        CoursesRepo.Update(currentCourse);
        MainPage.SyncDatabaseFields();
    }

    private void courseDetails_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateCourseDetails(e.NewTextValue);
    }

    private void UpdateCourseDetails(string newDetails)
    {
        if (!string.IsNullOrEmpty(newDetails))
        {
            currentCourse.Details = newDetails;
            CoursesRepo.Update(currentCourse);
            MainPage.SyncDatabaseFields();
        }
    }

    private void instructorName_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateInstructorField(e.NewTextValue, nameof(currentInstructor.Name));
    }

    private void instructorPhone_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateInstructorField(e.NewTextValue, nameof(currentInstructor.Phone));
    }

    private void instructorEmail_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (IsValidEmail(e.NewTextValue))
        {
            UpdateInstructorField(e.NewTextValue, nameof(currentInstructor.Email));
        }
    }

    private void UpdateInstructorField(string newValue, string propertyName)
    {
        if (!string.IsNullOrEmpty(newValue))
        {
            switch (propertyName)
            {
                case nameof(currentInstructor.Name):
                    currentInstructor.Name = newValue;
                    break;
                case nameof(currentInstructor.Phone):
                    currentInstructor.Phone = newValue;
                    break;
                case nameof(currentInstructor.Email):
                    currentInstructor.Email = newValue;
                    break;
            }

            InstructorsRepo.Update(currentInstructor);
            MainPage.SyncDatabaseFields();
        }
    }

    private void paName_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateAssessmentField(e.NewTextValue, nameof(PA.Name));
    }

    private void paDueDate_DateSelected(object sender, DateChangedEventArgs e)
    {
        UpdateAssessmentDate(e.NewDate, PA, true);
    }

    private void paStart_DateSelected(object sender, DateChangedEventArgs e)
    {
        UpdateAssessmentDate(e.NewDate, PA, false, true);
    }

    private void paEnd_DateSelected(object sender, DateChangedEventArgs e)
    {
        UpdateAssessmentDate(e.NewDate, PA, false, false);
    }

    private void UpdateAssessmentField(string newValue, string propertyName)
    {
        if (!string.IsNullOrEmpty(newValue))
        {
            switch (propertyName)
            {
                case nameof(PA.Name):
                    PA.Name = newValue;
                    break;
            }
            AssessmentRepo.Update(PA);
        }
    }

    private void UpdateAssessmentDate(DateTime newDate, Assessment assessment, bool isDueDate, bool isStartDate = false)
    {
        if (isDueDate)
        {
            assessment.DueDate = newDate;
        }
        else if (isStartDate)
        {
            assessment.StartDate = newDate;
        }
        else
        {
            assessment.EndDate = newDate;
        }
        AssessmentRepo.Update(assessment);
    }

    private void paStartNotif_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateAssessmentNotification(PA, paStartNotif.SelectedIndex, true);
    }

    private void paEndNotif_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateAssessmentNotification(PA, paEndNotif.SelectedIndex, false);
    }

    private void UpdateAssessmentNotification(Assessment assessment, int selectedIndex, bool isStartNotification)
    {
        if (isStartNotification)
        {
            assessment.StartNotification = selectedIndex;
        }
        else
        {
            assessment.EndNotification = selectedIndex;
        }
        AssessmentRepo.Update(assessment);
    }

    private void oaName_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateAssessmentField(e.NewTextValue, nameof(OA.Name));
    }

    private void oaDueDate_DateSelected(object sender, DateChangedEventArgs e)
    {
        UpdateAssessmentDate(e.NewDate, OA, true);
    }

    private void oaStart_DateSelected(object sender, DateChangedEventArgs e)
    {
        UpdateAssessmentDate(e.NewDate, OA, false, true);
    }

    private void oaEnd_DateSelected(object sender, DateChangedEventArgs e)
    {
        UpdateAssessmentDate(e.NewDate, OA, false, false);
    }

    private void oaStartNotif_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateAssessmentNotification(OA, oaStartNotif.SelectedIndex, true);
    }

    private void oaEndNotif_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateAssessmentNotification(OA, oaEndNotif.SelectedIndex, false);
    }

    private void addNote_Clicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(noteInput.Text))
        {
            NotesRepo.Insert(new Note(currentCourse.Id, noteInput.Text));
            LoadNotesOnUI();
            noteInput.Text = string.Empty;
        }
    }

    private void courseStartNotif_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateCourseNotification(currentCourse, courseStartNotif.SelectedIndex, true);
    }

    private void courseEndNotif_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateCourseNotification(currentCourse, courseEndNotif.SelectedIndex, false);
    }

    private void UpdateCourseNotification(Course course, int selectedIndex, bool isStartNotification)
    {
        if (isStartNotification)
        {
            course.StartNotification = selectedIndex;
        }
        else
        {
            course.EndNotification = selectedIndex;
        }
        CoursesRepo.Update(course);
        MainPage.SyncDatabaseFields();
    }

    public bool IsValidEmail(string email)
    {
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        Regex regex = new Regex(pattern);

        bool isValid = regex.IsMatch(email);
        SetErrorVisibility(emailError, !isValid);

        return isValid;
    }

    private bool ValidateDates()
    {
        bool isValid = courseEnd.Date >= courseStart.Date;
        SetErrorVisibility(startDateError, !isValid);

        return isValid;
    }

    private void SetErrorVisibility(Label errorLabel, bool isVisible)
    {
        errorLabel.IsVisible = isVisible;
    }
}