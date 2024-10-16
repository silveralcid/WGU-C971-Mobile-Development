using SQLite;
using TermsApp.Entities;

namespace TermsApp.Repository
{
    internal class NotesRepo : BaseRepo
    {
        public static List<Note> GetByCourse(int courseId)
        {
            try
            {
                using (SQLiteConnection connection = new(DBClient.DBPath))
                {
                    return [.. connection.Query<Note>($"SELECT * FROM Notes WHERE CourseId={courseId}")];
                }
            }
            catch (Exception)
            {
                return [];
            }
        }
    }
}
