using SQLite;
using TermsApp.Entities;

namespace TermsApp.Repository
{
    internal class DBClient
    {
        public static string DBPath = Path.Combine(
            FileSystem.AppDataDirectory,
            "TermsDatabase.db"
        );

        public static void SeedData()
        {
            File.Delete(DBPath);
            CreateTables();
            // Insert Terms
            Term term1 = new Term("Term 1", DateTime.Now, DateTime.Now.AddMonths(6));
            TermsRepo.Insert<Term>(term1);

            Instructor instructor = new Instructor("Anika Patel", "555-123-4567", "anika.patel@strimeuniversity.edu");
            InstructorsRepo.Insert(instructor);

            // Insert Courses
            List<Course> courses = new List<Course>
            {
                new Course(1, 1, "Course 1", DateTime.Now, DateTime.Now.AddMonths(4), "In Progress", "Enter Course Details Here:"),
            };
            foreach (var course in courses)
            {
                CoursesRepo.Insert(course);
            }

            courses = CoursesRepo.GetAllByTerm(1);
            foreach (var course in courses) 
            {
                AssessmentRepo.Insert(new Assessment(1, "Performance Assessment #1", DateTime.Now, DateTime.Now.AddMonths(3), "Enter details about assessment here:", course.Id));
                AssessmentRepo.Insert(new Assessment(0, "Objective Assessment #1", DateTime.Now, DateTime.Now.AddMonths(3), "Enter details about assessment here:", course.Id));
                NotesRepo.Insert(new Note(course.Id, "Test note"));
            }
        }

        public static void CreateTables()
        {
            using (SQLiteConnection connection = new(DBPath))
            {
                connection.CreateTable<Term>();
                connection.CreateTable<Course>();
                connection.CreateTable<Instructor>();
                connection.CreateTable<Assessment>();
                connection.CreateTable<Note>();
            }
        }
    }
}
