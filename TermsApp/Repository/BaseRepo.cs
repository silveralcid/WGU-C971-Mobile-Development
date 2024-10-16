using SQLite;

namespace TermsApp.Repository
{
    internal abstract class BaseRepo
    {

        public static bool Insert<TEntity>(TEntity entity)
        {
            try
            {
                using (SQLiteConnection connection = new(DBClient.DBPath))
                {
                    connection.Insert(entity);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool Update<TEntity>(TEntity entity)
        {
            try
            {
                using (SQLiteConnection connection = new(DBClient.DBPath))
                {
                    connection.Update(entity);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
