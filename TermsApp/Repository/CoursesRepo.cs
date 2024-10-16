using SQLite;
using TermsApp.Entities;

namespace TermsApp.Repository
{
    internal class CoursesRepo : BaseRepo
    {
        public static List<Course> GetAllByTerm(int  termId)
        {
            try
            {
                using (SQLiteConnection connection = new(DBClient.DBPath))
                {
                    return [.. connection.Query<Course>($"SELECT * FROM Courses WHERE TermId={termId}")];
                }
            }
            catch (Exception)
            {
                return [];
            }
        }

        public static bool AddNew(int termId)
        {
            try
            {
                Course course = new Course(termId, 1, "New Course", DateTime.Now, DateTime.Now.AddMonths(4), "Plan to Take", "Enter Course Details Here:");
                Insert(course);
                MainPage.SyncDatabaseFields();
                return true;
            }
            catch (Exception) 
            {
                return false;
            }
        }
    }
}
