using SQLite;
using TermsApp.Entities;

namespace TermsApp.Repository
{
    internal class AssessmentRepo : BaseRepo
    {
        public static List<Assessment> GetAll()
        {
            try
            {
                using (SQLiteConnection connection = new(DBClient.DBPath))
                {
                    return [.. connection.Query<Assessment>("SELECT * FROM Assessments")];
                }
            }
            catch (Exception)
            {
                return [];
            }
        }

        public static List<Assessment> GetByCourse(int courseId)
        {
            try
            {
                using (SQLiteConnection connection = new(DBClient.DBPath))
                {
                    return [.. connection.Query<Assessment>($"SELECT * FROM Assessments WHERE CourseId={courseId}")];
                }
            }
            catch (Exception)
            {
                return [];
            }
        }
    }
}
