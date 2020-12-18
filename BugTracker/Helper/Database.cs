using System;
using System.Diagnostics;
using System.Text;
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
        private int Query(string sql, List<MySqlParameter> parameters)
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
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private int Query(string sql, List<MySqlParameter> parameters, ref List<string> list)
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

                    MySqlDataReader mySqlDataReader = command.ExecuteReader();                   
                    
                    while(mySqlDataReader.Read())
                    {
                        StringBuilder stringBuilder = new StringBuilder();
                        int fieldCount = mySqlDataReader.FieldCount;

                        for(int i = 0; i < fieldCount; i++)
                        {
                            stringBuilder.Append(Convert.ToString(mySqlDataReader[i]));       
                        }
                        list.Add(stringBuilder.ToString());
                        rowsAffected++;
                    }
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

            return rowsAffected = Query(queryStatement, parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
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

            return rowsAffected = Query(queryStatement, parameters);  
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ListProjects"></param>
        /// <returns></returns>
        public int GetProjects(ref List<string> ListProjects)
        {
            int rowsAffected = 0;
            string queryStatement = "SELECT ProjectName FROM project WHERE active = 0";  
            rowsAffected = Query(queryStatement, new List<MySqlParameter>(), ref ListProjects);
            return ListProjects.Count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        public int SetProjectActiveState(string projectName, Boolean active)
        {
            int rowsAffected = 0;
            MySqlParameter mySqlParameter;
            string queryStatement = "UPDATE project SET active = @Active WHERE ProjectName = @ProjectName";
            List<MySqlParameter> parameters = new List<MySqlParameter>();

            mySqlParameter = new MySqlParameter("@ProjectName", System.Data.SqlDbType.VarChar);
            mySqlParameter.Value = projectName;
            parameters.Add(mySqlParameter);            

            mySqlParameter = new MySqlParameter("@Active", System.Data.SqlDbType.TinyInt);
            mySqlParameter.Value = active ? 0 : 1;
            parameters.Add(mySqlParameter);

            return rowsAffected = Query(queryStatement, parameters); 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="bugDescription"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public int AddBug(string projectName, string bugDescription, string firstName, string lastName, string status = "Open")
        {
            MySqlParameter mySqlParameter;

            string subQueryStatementProjectID = "(SELECT ProjectID FROM project WHERE ProjectName = @ProjectName)";
            string subQueryStatementUserID = "(SELECT UserID FROM user WHERE FirstName = @FirstName AND LastName = @LastName)";
            string queryStatement = $"INSERT INTO bug (ProjectID, UserID, BugIDAlpha, Description, Progress, DateAdded) VALUES({subQueryStatementProjectID}, {subQueryStatementUserID}, @BugIDAlpha, @Description, @Status, @DateAdded)";
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

            mySqlParameter = new MySqlParameter("@BugIDAlpha", System.Data.SqlDbType.Text);
            mySqlParameter.Value = "";
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

            return Query(queryStatement, parameters);  
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="bugIdAlpha"></param>
        /// <param name="bugComment"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <returns></returns>
        public int AddBugComment(string projectName, string bugIdAlpha, string bugComment, string firstName, string lastName)
        {
            MySqlParameter mySqlParameter;

            string subQueryStatementBugID = $"SELECT BugID FROM bug WHERE ProjectID=(SELECT ProjectID FROM Project WHERE ProjectName = @ProjectName)";
            string subQueryStatementUserID = "(SELECT UserID FROM user WHERE FirstName = @FirstName AND LastName = @LastName)";
            string queryStatement = $"INSERT INTO bugreport (BugID, UserID, Comments, DateAdded) VALUES({subQueryStatementBugID}, {subQueryStatementUserID}, @Comment, @DateAdded)";
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

            mySqlParameter = new MySqlParameter("@Comment", System.Data.SqlDbType.Text);
            mySqlParameter.Value = bugComment;
            parameters.Add(mySqlParameter);
       
            mySqlParameter = new MySqlParameter("@DateAdded", System.Data.SqlDbType.DateTime);
            mySqlParameter.Value = DateTime.Now;
            parameters.Add(mySqlParameter);

            return Query(queryStatement, parameters);  
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ListComments"></param>
        /// <returns></returns>
        public int GetComments(ref List<string> ListComments)
        {
            int rowsAffected = 0;
            string queryStatement = "SELECT ProjectName  FROM project WHERE active = 0";  
            rowsAffected = Query(queryStatement, new List<MySqlParameter>(), ref ListComments);
            return ListComments.Count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="bugIdAlpha"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        public int SetBugActiveState(string projectName, string bugIdAlpha, Boolean active)
        {
            int rowsAffected = 0;
            MySqlParameter mySqlParameter;
            string queryStatement = "UPDATE bug SET active = @Active WHERE ProjectName = @ProjectName AND BugIDAlpha = @BugIDAlpha;";
            List<MySqlParameter> parameters = new List<MySqlParameter>();

            mySqlParameter = new MySqlParameter("@ProjectName", System.Data.SqlDbType.VarChar);
            mySqlParameter.Value = projectName;
            parameters.Add(mySqlParameter);   
            
            mySqlParameter = new MySqlParameter("@BugIDAlpha", System.Data.SqlDbType.VarChar);
            mySqlParameter.Value = bugIdAlpha;
            parameters.Add(mySqlParameter);  

            mySqlParameter = new MySqlParameter("@Active", System.Data.SqlDbType.TinyInt);
            mySqlParameter.Value = active ? 0 : 1;
            parameters.Add(mySqlParameter);

            return rowsAffected = Query(queryStatement, parameters); 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="project"></param>
        /// <param name="ts"></param>
        /// <returns></returns>
        public int GetBugComments(string projectName, string bugIdAlpha, string fName, string lName, ref List<string> ts)
        {
            int rowsAffected = 0;
            MySqlParameter mySqlParameter;
            string queryStatement = "UPDATE bug SET active = @Active WHERE ProjectName = @ProjectName AND BugIDAlpha = @BugIDAlpha;";
            List<MySqlParameter> parameters = new List<MySqlParameter>();

            mySqlParameter = new MySqlParameter("@ProjectName", System.Data.SqlDbType.VarChar);
            mySqlParameter.Value = projectName;
            parameters.Add(mySqlParameter);   
            
            mySqlParameter = new MySqlParameter("@BugIDAlpha", System.Data.SqlDbType.VarChar);
            mySqlParameter.Value = bugIdAlpha;
            parameters.Add(mySqlParameter);  

            mySqlParameter = new MySqlParameter("@FirstName", System.Data.SqlDbType.VarChar);
            mySqlParameter.Value = fName;
            parameters.Add(mySqlParameter);

            mySqlParameter = new MySqlParameter("@LastName", System.Data.SqlDbType.VarChar);
            mySqlParameter.Value = lName;
            parameters.Add(mySqlParameter);

            rowsAffected = Query(queryStatement, parameters, ref ts); 
            return ts.Count;
        }
    }
}
