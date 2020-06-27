using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Anonymous.Classes
{
    public class Db
    {
        private static SqlConnection GetSqlConnection()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionString"].ConnectionString;
            var connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }
        public static int ExecuteNonQuery(string commandText, out bool boolDbError, out string strDbError, List<SqlParameter> sqlParameters = null, CommandType commandType = CommandType.Text)
        {
            boolDbError = false;
            strDbError = string.Empty;
            int affectedRows = 0;
            using (SqlConnection connection = GetSqlConnection())
            {
                using (SqlCommand command = new SqlCommand(commandText, connection))
                {
                    command.CommandType = commandType;

                    if (sqlParameters != null && sqlParameters.Count > 0)
                    {
                        foreach (SqlParameter sqlParameter in sqlParameters)
                        {
                            command.Parameters.Add(sqlParameter);
                        }
                    }

                    try { 
                        affectedRows = command.ExecuteNonQuery();
                    }
                    catch(Exception ex)
                    {
                        boolDbError = true;
                        strDbError = "Error executing command: " + ex.ToString();

                        List<SqlParameter> _sqlParameters = new List<SqlParameter>();
                        SqlParameter sqlParameter = new SqlParameter();
                        sqlParameter.ParameterName = "@errorText";
                        sqlParameter.Value = ex.ToString();
                        _sqlParameters.Add(sqlParameter);

                        if (sqlParameters == null)
                        {
                            sqlParameter = new SqlParameter();
                            sqlParameter.ParameterName = "@parameters";
                            sqlParameter.Value = DBNull.Value;
                            _sqlParameters.Add(sqlParameter);
                        }
                        else
                        {
                            string strParameters = string.Empty;
                            foreach (var s in sqlParameters)
                            {
                                strParameters += "name: " + s.ParameterName + " value: " + s.Value + Environment.NewLine;
                            }
                            sqlParameter = new SqlParameter();
                            sqlParameter.ParameterName = "@parameters";
                            sqlParameter.Value = strParameters;
                            _sqlParameters.Add(sqlParameter);
                        }

                        sqlParameter = new SqlParameter();
                        sqlParameter.ParameterName = "@strSql";
                        sqlParameter.Value = commandText;
                        _sqlParameters.Add(sqlParameter); 


                        LogDbError("INSERT INTO errorLog(errorText,parameters,strSql) VALUES(@errorText,@parameters,@strSql)", out bool bError, out string sError, _sqlParameters);
                    }
                }
            }
            return affectedRows;
        }
        public static int LogDbError(string commandText, out bool boolDbError, out string strDbError, List<SqlParameter> sqlParameters = null, CommandType commandType = CommandType.Text)
        {
            boolDbError = false;
            strDbError = string.Empty;
            int affectedRows = 0;
            using (SqlConnection connection = GetSqlConnection())
            {
                using (SqlCommand command = new SqlCommand(commandText, connection))
                {
                    command.CommandType = commandType;

                    if (sqlParameters != null && sqlParameters.Count > 0)
                    {
                        foreach (SqlParameter sqlParameter in sqlParameters)
                        {
                            command.Parameters.Add(sqlParameter);
                        }
                    }

                    try
                    {
                        affectedRows = command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        boolDbError = true;
                        strDbError = "Error executing command: " + ex.ToString();
                    }
                }
            }
            return affectedRows;
        }


        public static DataSet ExecuteQuery(string commandText, out bool boolDbError, out string strDbError, List<SqlParameter> sqlParameters = null, CommandType commandType = CommandType.Text)
        {
            boolDbError = false;
            strDbError = string.Empty;
            using (SqlConnection connection = GetSqlConnection())
            {
                using (SqlCommand command = new SqlCommand(commandText, connection))
                {
                    DataSet ds = new DataSet();
                    command.CommandType = commandType;

                    if (sqlParameters != null && sqlParameters.Count > 0)
                    {
                        foreach (SqlParameter sqlParameter in sqlParameters)
                        {
                            command.Parameters.Add(sqlParameter);
                        }
                    }

                    try
                    {
                        SqlDataAdapter da = new SqlDataAdapter(command);
                        da.Fill(ds);
                    }
                    catch (Exception ex)
                    {
                        boolDbError = true;
                        strDbError = "Error executing command: " + ex.ToString();
                    }
                    //connection.Close();// is this needed?
                    return ds;
                }
            }
        }

        public class StoredProcedures
        {
            public static DataSet AnonIdBySaltedGoogleUserId(out bool boolDbError, out string strDbError, List<SqlParameter> sqlParameters= null)
            {
                DataSet ds = new DataSet();
                ds = ExecuteQuery("AnonIdBySaltedGoogleUserId", out boolDbError, out strDbError, sqlParameters, CommandType.StoredProcedure);
                return ds;
            }

            public static DataSet AnonCreate(out bool boolDbError, out string strDbError, List<SqlParameter> sqlParameters = null)
            {
                DataSet ds = new DataSet();
                ds = ExecuteQuery("AnonCreate", out boolDbError, out strDbError, sqlParameters, CommandType.StoredProcedure);
                return ds;
            }

            public static DataSet AnonUpdateSaltedGoogleUserId(out bool boolDbError, out string strDbError, List<SqlParameter> sqlParameters = null)
            {
                DataSet ds = new DataSet();
                ds = ExecuteQuery("AnonUpdateSaltedGoogleUserId", out boolDbError, out strDbError, sqlParameters, CommandType.StoredProcedure);
                return ds;
            }
            
        }

        public class Common
        {
            public static bool DoesEmailAlreadyExistInDb(string strUnecryptedEmail, out byte[] hashedEmail, out DataSet dataSet)
            {
                hashedEmail = Security.Hash(strUnecryptedEmail);
                //check if email already exists in system
                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                SqlParameter sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@email";
                sqlParameter.Value = hashedEmail;
                sqlParameters.Add(sqlParameter);
                dataSet = Db.ExecuteQuery("SELECT anonId, encryptedPassword FROM anon WHERE email = @email", out bool boolDbError, out string strDbError, sqlParameters);
                if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count>0)
                { 
                    return true; 
                }
                else
                { 
                    return false; 
                }
            }

            public static bool IsAdmin()
            {
                //check if email already exists in system
                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                SqlParameter sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@anonId";
                sqlParameter.Value = HttpContext.Current.Session["anonId"].ToString();
                sqlParameters.Add(sqlParameter);
                DataSet dataSet = Db.ExecuteQuery("SELECT isAdmin FROM anon WHERE anonId = @anonId AND isAdmin = 1", out bool boolDbError, out string strDbError, sqlParameters);
                if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public static bool AreEmailAndPasswordCorrect(string strUnecryptedEmail, string strPassword, out DataSet dataSet)
            {
                byte[] hashedEmail = Security.Hash(strUnecryptedEmail);
                byte[] hashedPassword = Security.Hash(strPassword);
                //check if email already exists in system
                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                SqlParameter sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@email";
                sqlParameter.Value = hashedEmail;
                sqlParameters.Add(sqlParameter);
                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@encryptedPassword";
                sqlParameter.Value = hashedPassword;
                sqlParameters.Add(sqlParameter);
                dataSet = Db.ExecuteQuery("SELECT anonId FROM anon WHERE email = @email AND encryptedPassword = @encryptedPassword", out bool boolDbError, out string strDbError, sqlParameters);
                if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            /// <summary>
            /// Gets full record from anon table
            /// </summary>
            /// <param name="boolDbError"></param>
            /// <param name="strDbError"></param>
            /// <param name="sqlParameters">@anonId</param>
            /// <returns></returns>
            public static DataSet AnonByAnonId(out bool boolDbError, out string strDbError, List<SqlParameter> sqlParameters = null)
            {

                return ExecuteQuery("SELECT * FROM anon WHERE anonId = @anonId", out boolDbError, out strDbError, sqlParameters);

            }

            public static DataSet AnonByAnonId(out bool boolDbError, out string strDbError, string anonId)
            {
                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                SqlParameter sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "anonId";
                sqlParameter.Value = anonId;
                sqlParameters.Add(sqlParameter);

                return ExecuteQuery("SELECT * FROM anon WHERE anonId = @anonId", out boolDbError, out strDbError, sqlParameters);

            }

            public static bool IsEmailVerified
            {
                get
                {
                    if (HttpContext.Current.Session["anonId"] != null)
                    {
                        List<SqlParameter> sqlParameters = new List<SqlParameter>();
                        SqlParameter sqlParameter = new SqlParameter();
                        sqlParameter.ParameterName = "@anonId";
                        sqlParameter.Value = HttpContext.Current.Session["anonId"].ToString();
                        sqlParameters.Add(sqlParameter);

                        DataSet dataSet = AnonByAnonId(out bool boolDbError, out string strDbError, sqlParameters);

                        if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
                        {
                            bool emailVerified = false;
                            bool.TryParse(dataSet.Tables[0].Rows[0]["emailVerified"].ToString(), out emailVerified);
                            if (emailVerified)
                            {
                                return true;
                            }

                        }
                    }
                    return false;
                }

            }
        }

    }
}