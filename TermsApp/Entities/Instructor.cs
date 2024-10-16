using SQLite;
namespace TermsApp.Entities
{
    [Table("Instructors")]
    public class Instructor
    {
        public Instructor() { }
        public Instructor(string name, string phone, string email)
        {
            Name = name;
            Phone = phone;
            Email = email;
        }
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }
}
