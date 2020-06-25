using Anonymous.Classes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Anonymous
{
    public partial class ChangePassword : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Security.IsLoggedInOrKick();
            }

        }

        protected void btnChangePassword_Click(object sender, EventArgs e)
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@anonId";
            sqlParameter.Value = Session["anonId"].ToString();
            sqlParameters.Add(sqlParameter);

            DataSet dataSet = Db.Common.AnonByAnonId(out bool boolDbError, out string strDbError, sqlParameters);
            if (BCrypt.Net.BCrypt.EnhancedVerify(tbxPassword.Text.Trim(), dataSet.Tables[0].Rows[0]["encryptedPassword"].ToString()))
            {

                string newHashedPassword = System.Text.Encoding.Default.GetString(Security.Hash(tbxNewPassword.Text, Session["email"].ToString().ToLower().Trim() + ConfigurationManager.AppSettings["AppSalt"]));
                string encryptedPrivateKeyWithNewHash = Security.EncryptStringAES(Session["PrivateKey"].ToString(), newHashedPassword);
                string encryptedLastLoginDateTimeWithNewHash = Security.EncryptStringAES(DateTime.Now.ToString(), newHashedPassword);

                if (dataSet.Tables[0].Rows[0]["encryptedLastLoginDateTime"] != null && dataSet.Tables[0].Rows[0]["encryptedLastLoginDateTime"].ToString() != string.Empty)
                {
                    Security.EncryptStringAES(Security.DecryptStringAES(dataSet.Tables[0].Rows[0]["encryptedLastLoginDateTime"].ToString(), Session["hashedPassword"].ToString()), newHashedPassword);
                }

                //DataTable dataTableAnon = new DataTable();

                sqlParameters = new List<SqlParameter>();

                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@anonId";
                sqlParameter.Value = Session["anonId"].ToString();
                sqlParameters.Add(sqlParameter);

                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@encryptedPassword";
                sqlParameter.Value = BCrypt.Net.BCrypt.EnhancedHashPassword(tbxNewPassword.Text);
                sqlParameters.Add(sqlParameter);

                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@encryptedPrivateKey";
                sqlParameter.Value = encryptedPrivateKeyWithNewHash;
                sqlParameters.Add(sqlParameter);

                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@encryptedLastLoginDateTime";
                sqlParameter.Value = encryptedLastLoginDateTimeWithNewHash;
                sqlParameters.Add(sqlParameter);

                Db.ExecuteNonQuery("UPDATE anon SET encryptedPassword=@encryptedPassword, encryptedPrivateKey=@encryptedPrivateKey,encryptedLastLoginDateTime=@encryptedLastLoginDateTime WHERE anonId=@anonId", out boolDbError, out strDbError, sqlParameters);

                //Friend
                sqlParameters = new List<SqlParameter>();

                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@anonId";
                sqlParameter.Value = Session["anonId"].ToString();
                sqlParameters.Add(sqlParameter);

                DataSet dataSetFriend = Db.ExecuteQuery("SELECT * FROM friend WHERE anonId=@anonId", out boolDbError, out strDbError, sqlParameters);
                if (dataSetFriend.Tables.Count > 0 && dataSetFriend.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in dataSetFriend.Tables[0].Rows)
                    {

                        string nickName = string.Empty;
                        if (row["nickName"] != null && row["nickName"].ToString() != string.Empty)
                        {
                            nickName = Security.DecryptStringAES(row["nickName"].ToString(), Session["hashedPassword"].ToString());
                        }
                        string email = string.Empty;
                        if (row["email"] != null && row["email"].ToString() != string.Empty)
                        {
                            email = Security.DecryptStringAES(row["email"].ToString(), Session["hashedPassword"].ToString());
                        }
                        string encryptedFriendAnonId = string.Empty;
                        if (row["encryptedFriendAnonId"] != null && row["encryptedFriendAnonId"].ToString() != string.Empty)
                        {
                            encryptedFriendAnonId = Security.DecryptStringAES(row["encryptedFriendAnonId"].ToString(), Session["hashedPassword"].ToString());
                        }

                        sqlParameters = new List<SqlParameter>();

                        sqlParameter = new SqlParameter();
                        sqlParameter.ParameterName = "@nickName";
                        if (nickName == String.Empty)
                        {
                            sqlParameter.Value = DBNull.Value;
                        }
                        else
                        {
                            sqlParameter.Value = Security.EncryptStringAES(nickName, newHashedPassword);
                        }
                        sqlParameters.Add(sqlParameter);

                        sqlParameter = new SqlParameter();
                        sqlParameter.ParameterName = "@email";
                        if (email == String.Empty)
                        {
                            sqlParameter.Value = DBNull.Value;
                        }
                        else
                        {
                            sqlParameter.Value = Security.EncryptStringAES(email, newHashedPassword);
                        }
                        sqlParameters.Add(sqlParameter);

                        sqlParameter = new SqlParameter();
                        sqlParameter.ParameterName = "@encryptedFriendAnonId";
                        if (encryptedFriendAnonId == String.Empty)
                        {
                            sqlParameter.Value = DBNull.Value;
                        }
                        else
                        {
                            sqlParameter.Value = Security.EncryptStringAES(encryptedFriendAnonId, newHashedPassword);
                        }
                        sqlParameters.Add(sqlParameter);

                        sqlParameter = new SqlParameter();
                        sqlParameter.ParameterName = "@friendId";
                        sqlParameter.Value = row["friendId"].ToString();
                        sqlParameters.Add(sqlParameter);

                        Db.ExecuteNonQuery("UPDATE friend SET nickName = @nickName, email = @email, encryptedFriendAnonId = @encryptedFriendAnonId WHERE friendId = @friendId", out boolDbError, out strDbError, sqlParameters);

                    }

                }

                #region for deleting data

                //sqlParameters = new List<SqlParameter>();

                //sqlParameter = new SqlParameter();
                //sqlParameter.ParameterName = "@anonId";
                //sqlParameter.Value = Session["anonId"].ToString();
                //sqlParameters.Add(sqlParameter);

                //Db.ExecuteQuery("UPDATE post SET msg = '[This account was deleted]' WHERE anonId= @anonId", out boolDbError, out strDbError, sqlParameters);

                #endregion

                //TODO: friendRequest data cannot be found as there's no relationship on data to account creator

                //privateMessage
                //sqlParameters = new List<SqlParameter>();

                //sqlParameter = new SqlParameter();
                //sqlParameter.ParameterName = "@anonId";
                //sqlParameter.Value = Session["anonId"].ToString();
                //sqlParameters.Add(sqlParameter);

                //DataSet dataSetPrivateMessage = Db.ExecuteQuery("SELECT * FROM privateMessage WHERE anonId = @anonId", out boolDbError, out strDbError, sqlParameters);
                //if (dataSetPrivateMessage.Tables.Count > 0 && dataSetPrivateMessage.Tables[0].Rows.Count > 0)
                //{
                //    foreach (DataRow row in dataSetFriend.Tables[0].Rows)
                //    {
                //        sqlParameters = new List<SqlParameter>();

                //        sqlParameter = new SqlParameter();
                //        sqlParameter.ParameterName = "@privateMessageId";
                //        sqlParameter.Value = row["privateMessageId"].ToString();
                //        sqlParameters.Add(sqlParameter);

                //        sqlParameter = new SqlParameter();
                //        sqlParameter.ParameterName = "@EncryptionKeyEncryptedByPublicKey";
                //        sqlParameter.Value = Security.EncryptStringAES(Security.DecryptStringAES(row["EncryptionKeyEncryptedByPublicKey"].ToString(), Session["hashedPassword"].ToString()), newHashedPassword);
                //        sqlParameters.Add(sqlParameter);

                //        Db.ExecuteNonQuery("UPDATE privateMessage SET WHERE privateMessageId=@privateMessageId", out boolDbError, out strDbError, sqlParameters);

                //    }
                //}

                Session["hashedPassword"] = newHashedPassword;

                lblPasswordChangeSuccess.Text = "Password changed. Please update your password manager!";
                lblPasswordChangeError.Visible = false;
                lblPasswordChangeSuccess.Visible = true;
            }
            else
            {
                lblPasswordChangeError.Text = "Password is not correct";
                lblPasswordChangeError.Visible = true;
                lblPasswordChangeSuccess.Visible = false;
            }
        }
    }
}