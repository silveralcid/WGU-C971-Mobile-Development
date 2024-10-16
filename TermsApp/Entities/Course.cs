using SQLite;

namespace TermsApp.Entities
{
    [Table("Courses")]
    public class Course
    {
        public Course() { }
        public Course(int termId, int instructorId, string courseName, DateTime startDate, DateTime endDate, string status, string courseDetails)
        {
            TermId = termId;
            InstructorId = instructorId;
            Name = courseName;
            StartDate = startDate;
            EndDate = endDate;
            Status = status;
            Details = courseDetails;
        }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int TermId { get; set; }
        public int InstructorId { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public string Details { get; set; }
        public int StartNotification { get; set; }
        public int EndNotification { get; set; }

    }
}
