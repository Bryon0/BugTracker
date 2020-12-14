using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Sql;
using System.Data.SqlClient;

namespace BugTracker.Helper
{
    class Database
    {
        private string _server;
        private string _database;

        public Database(string server, string database)
        {
            _server = server;
            _database = database;
        }
        public int CreateNewUser(string firstName, string lastName)
        {
             int rowsAffected = 0;
             //string connectionString = @"server=(local)\SQLExpress;database=Company;integrated Security=SSPI;";
             string connectionString = $@"server={_server};database={_database};integrated Security=SSPI;";
             using(SqlConnection _con = new SqlConnection(connectionString))
             {
                  //string queryStatement = $"INSERT INTO User (FirstName, LastName) VALUES({firstName}, {lastName});";
                  string queryStatement = "INSERT INTO User (FirstName, LastName) VALUES(@FirstName, @LastName);";
                  using(SqlCommand _cmd = new SqlCommand(queryStatement, _con))
                  {
                    
                    _cmd.Parameters.Add("@FirstName", System.Data.SqlDbType.VarChar);
                    _cmd.Parameters["@FirstName"].Value = firstName;

                    _cmd.Parameters.Add("@LastName", System.Data.SqlDbType.VarChar);
                    _cmd.Parameters["@LastName"].Value = lastName;

                    _con.Open();
                    rowsAffected = _cmd.ExecuteNonQuery();

                  }
             }

             return rowsAffected;
        }

        public int CreateNewProject(string projectName)
        {
             int rowsAffected = 0;
             //string connectionString = @"server=(local)\SQLExpress;database=Company;integrated Security=SSPI;";
             string connectionString = $@"server={_server};database={_database};integrated Security=SSPI;";
             using(SqlConnection _con = new SqlConnection(connectionString))
             {
                  string queryStatement = "IF NOT EXISTS(SELECT * FROM Project WHERE ProjectName='@ProjectName') BEGIN INSERT INTO Project (ProjectName, DateAdded) VALUE(@PRojectName, @DateAdded) END";
                  using(SqlCommand _cmd = new SqlCommand(queryStatement, _con))
                  {                    
                        _cmd.Parameters.Add("@ProjectName", System.Data.SqlDbType.VarChar);
                        _cmd.Parameters["@ProjectName"].Value = projectName;

                        _cmd.Parameters.Add("@DateAdded", System.Data.SqlDbType.VarChar);
                        _cmd.Parameters["@DateAdded"].Value = DateTime.Now.ToString();

                        _con.Open();
                        rowsAffected = _cmd.ExecuteNonQuery();
                  }
             }

             return rowsAffected;
        }

        public int AddBugComment(string projectName, string bugComment, string firstName, string lastName)
        {
             int rowsAffected = 0;
             //string connectionString = @"server=(local)\SQLExpress;database=Company;integrated Security=SSPI;";
             string connectionString = $@"server={_server};database={_database};integrated Security=SSPI;";
             using(SqlConnection _con = new SqlConnection(connectionString))
             {
                  string queryStatement = "IF NOT EXISTS(SELECT * FROM Project WHERE ProjectName='@ProjectName') BEGIN INSERT INTO Project (ProjectName, DateAdded) VALUE(@PRojectName, @DateAdded) END";
                  using(SqlCommand _cmd = new SqlCommand(queryStatement, _con))
                  {                    
                        _cmd.Parameters.Add("@ProjectName", System.Data.SqlDbType.VarChar);
                        _cmd.Parameters["@ProjectName"].Value = projectName;

                        _cmd.Parameters.Add("@DateAdded", System.Data.SqlDbType.VarChar);
                        _cmd.Parameters["@DateAdded"].Value = DateTime.Now.ToString();

                        _con.Open();
                        rowsAffected = _cmd.ExecuteNonQuery();
                  }
             }

             return rowsAffected;
        }
    }
}
