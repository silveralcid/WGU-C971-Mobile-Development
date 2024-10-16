using SQLite;

namespace TermsApp.Entities
{
    [Table("Notes")]
    public class Note
    {
        public Note() { }
        public Note(int courseId, string content)
        {
            this.CourseId = courseId;
            this.Content = content;
        }
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Content { get; set; }
    }
}
