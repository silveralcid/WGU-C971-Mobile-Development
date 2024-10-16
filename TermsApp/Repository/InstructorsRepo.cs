using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TermsApp.Entities;

namespace TermsApp.Repository
{
    internal class InstructorsRepo : BaseRepo
    {
        public static List<Instructor> GetAll()
        {
            try
            {
                using (SQLiteConnection connection = new(DBClient.DBPath))
                {
                    return [.. connection.Query<Instructor>("SELECT * FROM Instructors")];
                }
            }
            catch (Exception)
            {
                return [];
            }
        }
    }
}
