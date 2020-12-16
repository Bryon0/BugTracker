using System;
using System.Diagnostics;
using MySqlConnector;  //NuGet Install-Package MySqlConnector -Version 1.0.1
using System.Collections.Generic;

namespace BugTracker
{
    class Database
    {
        private string _server;
        private string _database;
        private string _userid;
        private string _password;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="server"></param>
        /// <param name="database"></param>
        /// <param name="userid"></param>
        /// <param name="password"></param>
        public Database(string server, string database, string userid, string password)
        {
            _server = server;
            _database = database;
            _userid = userid;
            _password = password;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private int Insert(string sql, List<MySqlParameter> parameters)
        {
            int rowsAffected = 0;
            using (var connection = new MySqlConnection($"Server={_server};User ID={_userid};Password={_password};Database={_database}"))
            {   
                connection.Open();                
                using (var command = new MySqlCommand(sql, connection))
                {
                    foreach(MySqlParameter parameter in parameters)
                    {
                        command.Parameters.Add(parameter);
                    }

                    rowsAffected = command.ExecuteNonQuery();
                }
            }
            return rowsAffected;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <returns></returns>
        public int CreateNewUser(string firstName, string lastName)
        {
            int rowsAffected = 0;
            MySqlParameter mySqlParameter;
            string queryStatement = "INSERT INTO User (FirstName, LastName, DateAdded) VALUES(@FirstName, @LastName, @DateAdded);";
            List<MySqlParameter> parameters = new List<MySqlParameter>();

            mySqlParameter = new MySqlParameter("@FirstName", System.Data.SqlDbType.VarChar);
            mySqlParameter.Value = firstName;
            parameters.Add(mySqlParameter);
            
            mySqlParameter = new MySqlParameter("@LastName", System.Data.SqlDbType.VarChar);
            mySqlParameter.Value = lastName;
            parameters.Add(mySqlParameter);

            mySqlParameter = new MySqlParameter("@DateAdded", System.Data.SqlDbType.DateTime);
            mySqlParameter.Value = DateTime.Now;
            parameters.Add(mySqlParameter);

            return rowsAffected = Insert(queryStatement, parameters);
        }

        public int CreateNewProject(string projectName)
        {
            int rowsAffected = 0;
            MySqlParameter mySqlParameter;
            string queryStatement = "INSERT INTO project (ProjectName, DateAdded) SELECT @ProjectName, @DateAdded WHERE NOT EXISTS (SELECT * FROM project WHERE ProjectName = @ProjectName);";
            List<MySqlParameter> parameters = new List<MySqlParameter>();

            mySqlParameter = new MySqlParameter("@ProjectName", System.Data.SqlDbType.VarChar);
            mySqlParameter.Value = projectName;
            parameters.Add(mySqlParameter);            

            mySqlParameter = new MySqlParameter("@DateAdded", System.Data.SqlDbType.DateTime);
            mySqlParameter.Value = DateTime.Now;
            parameters.Add(mySqlParameter);

            return rowsAffected = Insert(queryStatement, parameters);  
        }

        public int AddBug(string projectName, string bugDescription, string firstName, string lastName, string status = "Open")
        {
            MySqlParameter mySqlParameter;

            //mysql> INSERT INTO joke(joke_text, joke_date, author_id)
            //VALUES (‘Humpty Dumpty had a great fall.’, ‘1899–03–13’, (SELECT id FROM author WHERE author_name = ‘Famous Anthony’));
            string subQueryStatementProjectID = "(SELECT ProjectID FROM project WHERE ProjectName = @ProjectName)";
            string subQueryStatementUserID = "(SELECT UserID FROM user WHERE FirstName = @FirstName AND LastName = @LastName)";
            string queryStatement = $"INSERT INTO bug (ProjectID, UserID, Description, Status, DateAdded) VALUES({subQueryStatementProjectID}, {subQueryStatementUserID}, @Description, @Status, @DateAdded)";
            List<MySqlParameter> parameters = new List<MySqlParameter>();

            mySqlParameter = new MySqlParameter("@ProjectName", System.Data.SqlDbType.VarChar);
            mySqlParameter.Value = projectName;
            parameters.Add(mySqlParameter);    
            
            mySqlParameter = new MySqlParameter("@FirstName", System.Data.SqlDbType.VarChar);
            mySqlParameter.Value = firstName;
            parameters.Add(mySqlParameter); 

            mySqlParameter = new MySqlParameter("@LastName", System.Data.SqlDbType.VarChar);
            mySqlParameter.Value = lastName;
            parameters.Add(mySqlParameter); 

            mySqlParameter = new MySqlParameter("@Description", System.Data.SqlDbType.Text);
            mySqlParameter.Value = bugDescription;
            parameters.Add(mySqlParameter);

            mySqlParameter = new MySqlParameter("@Status", System.Data.SqlDbType.VarChar);
            mySqlParameter.Value = status;
            parameters.Add(mySqlParameter);

            mySqlParameter = new MySqlParameter("@DateAdded", System.Data.SqlDbType.DateTime);
            mySqlParameter.Value = DateTime.Now;
            parameters.Add(mySqlParameter);

            return Insert(queryStatement, parameters);  
        }
    }
}
