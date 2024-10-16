using SQLite;
using TermsApp.Entities;

namespace TermsApp.Repository
{
    internal class TermsRepo : BaseRepo
    {
        public static bool AddNew()
        {
            try
            {
                using (SQLiteConnection connection = new(DBClient.DBPath))
                {
                    var query = connection.Query<Term>($"SELECT * FROM Terms ORDER BY Id DESC LIMIT 1");
                    Term latestTerm = query.First();
                    string termName = "Term " + (latestTerm.Id + 1).ToString();

                    Term newTerm = new(termName, DateTime.Now, DateTime.Now.AddDays(60));
                    Insert(newTerm);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static List<Term> GetAll()
        {
            try
            {
                using (SQLiteConnection connection = new(DBClient.DBPath))
                {
                    return [.. connection.Query<Term>("SELECT * FROM Terms")];
                }
            }
            catch (Exception)
            {
                return [];
            }
        }
    }
}
