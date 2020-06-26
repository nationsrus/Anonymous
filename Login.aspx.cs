using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Anonymous.Classes;
using System.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Security;
using System.Net;
using System.IO;

namespace Anonymous
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ltlWebsiteName.Text = ConfigurationManager.AppSettings["websiteName"].ToString();
            }
        }

        private void createAccount(string googleUserId)
        {
            // generate salted googleUserId 

            byte[] saltedGoogleUserId = Security.Hash(googleUserId, Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["AppSalt"]));

            // check if account already exists
            if (login(googleUserId))
            {
                Response.Write("account already exists!!!");
            }
            else
            {

                // create anon record and get anonId from database
                DataSet ds = Db.StoredProcedures.AnonCreate(out bool boolDbError, out string strDbError);
                Session["anonId"] = ds.Tables[0].Rows[0][0].ToString(); //TODO: get this from database

                // update profile with salted googleUserId

                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                SqlParameter sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@anonId";
                sqlParameter.Value = Session["anonId"].ToString();
                sqlParameters.Add(sqlParameter);

                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@saltedGoogleUserId";
                sqlParameter.Value = saltedGoogleUserId;
                sqlParameters.Add(sqlParameter);

                Db.StoredProcedures.AnonUpdateSaltedGoogleUserId(out boolDbError, out strDbError, sqlParameters);

                //Response.Write("strSaltedGoogleUserId: " + strSaltedGoogleUserId);
            }
        }

        private bool login(string googleUserId)
        {

            byte[] saltedGoogleUserId = Security.Hash(googleUserId, Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["AppSalt"]));

            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@saltedGoogleUserId";
            sqlParameter.Value = saltedGoogleUserId;
            sqlParameters.Add(sqlParameter);

            DataSet ds = Db.StoredProcedures.AnonIdBySaltedGoogleUserId(out bool boolDbError, out string strDbError, sqlParameters);

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count == 1)
            {
                Session["anonId"] = ds.Tables[0].Rows[0][0].ToString(); //TODO: get this from database

                Response.Write("anonId: " + Session["anonId"].ToString());

                return true;
            }

            return false;


        }

        protected void btnSubmitOnClick(object sender, EventArgs e)
        {
            createAccount(tbxUserId.Text);
        }

        protected void btnLoginOnClick(object sender, EventArgs e)
        {
            if (login(tbxLoginAccount.Text))
            {
                Response.Write(" success");
            }
            else
            {
                Response.Write(" account does not exist");
            }
        }

        protected void btnRsaLoginOnClick(object sender, EventArgs e)
        {

            //look for account in database
            //create password hash
            string encryptedPassword = Security.EncryptStringAES(tbxPassword.Text, tbxPassword.Text);

            //compare password hashes
            //generate secret 
            string encryptedEmailAndPassword = Security.EncryptStringAES(tbxPassword.Text, tbxPassword.Text);
            //push private+public keys into memory
            Session["encryptedEmailAndPassword"] = encryptedEmailAndPassword;

            string encryptedMsg = Security.EncryptStringAES(tbxMsg.Text, encryptedEmailAndPassword);
            string decryptedMsg = Security.DecryptStringAES(encryptedMsg, encryptedEmailAndPassword);


            //Debug.Assert(decrypted == original);
            //Response.Write("<br>encryptedPassword: " + encryptedPassword);
            //Response.Write("<br>encryptedEmailAndPassword: " + encryptedEmailAndPassword);
            //Response.Write("<br>encryptedMsg: " + encryptedMsg);
            //Response.Write("<br>decryptedMsg: " + decryptedMsg);
        }

        protected void btnSecureLoginOnClick(object sender, EventArgs e)
        {
            if (Session["lastLoginAttemptTimestamp"] != null && Session["loginAttempts"] != null)
            {
                if (DateTime.TryParse(Session["lastLoginAttemptTimestamp"].ToString(), out DateTime lastLoginAttemptTimestamp))
                {
                    if (int.TryParse(Session["loginAttempts"].ToString(), out int loginAttempts))
                    {
                        if (lastLoginAttemptTimestamp < DateTime.Now.AddMinutes(-15) && loginAttempts > 1)
                        {
                            Session["loginAttempts"] = 0;
                        }
                    }
                }
            }

            if (Db.Common.DoesEmailAlreadyExistInDb(tbxEmail.Text.ToLower().Trim(), out byte[] encryptedEmail, out DataSet dataSet))
            {
                if(BCrypt.Net.BCrypt.EnhancedVerify(tbxPswd.Text.Trim(), dataSet.Tables[0].Rows[0]["encryptedPassword"].ToString()))
                {
                    Session["anonId"] = dataSet.Tables[0].Rows[0]["anonId"].ToString();

                    List<SqlParameter> sqlParameters = new List<SqlParameter>();
                    SqlParameter sqlParameter = new SqlParameter();
                    sqlParameter.ParameterName = "@anonId";
                    sqlParameter.Value = Session["anonId"].ToString();
                    sqlParameters.Add(sqlParameter);
                    dataSet = Db.Common.AnonByAnonId(out bool boolDbError, out string strDbError, sqlParameters);
                    
                    //Session["encryptedEmailAndPassword"] = Security.Hash(tbxEmail.Text + tbxPswd.Text);
                    Session["hashedPassword"] = System.Text.Encoding.Default.GetString(Security.Hash(tbxPswd.Text, tbxEmail.Text.ToLower().Trim() + ConfigurationManager.AppSettings["AppSalt"]));
                    Session["PrivateKey"] = Security.DecryptStringAES(dataSet.Tables[0].Rows[0]["encryptedPrivateKey"].ToString(), Session["hashedPassword"].ToString());
                    Session["PublicKey"] = dataSet.Tables[0].Rows[0]["PublicKey"].ToString();
                    Session["email"] = tbxEmail.Text.ToLower().Trim();

                    //Get and update entryptedLastLoginDateTime
                    if (dataSet.Tables[0].Rows[0]["encryptedLastLoginDateTime"] != null && dataSet.Tables[0].Rows[0]["encryptedLastLoginDateTime"].ToString() != string.Empty)
                    {
                        Session["LastLoginDateTime"] = Security.DecryptStringAES(dataSet.Tables[0].Rows[0]["encryptedLastLoginDateTime"].ToString(), Session["hashedPassword"].ToString());
                    }
                    else
                    {
                        Session["LastLoginDateTime"] = DateTime.Now.ToString();
                    }

                    sqlParameters = new List<SqlParameter>();
                    sqlParameter = new SqlParameter();
                    sqlParameter.ParameterName = "@anonId";
                    sqlParameter.Value = Session["anonId"].ToString();
                    sqlParameters.Add(sqlParameter);

                    sqlParameter = new SqlParameter();
                    sqlParameter.ParameterName = "@encryptedLastLoginDateTime";
                    sqlParameter.Value = Security.EncryptStringAES(DateTime.Now.ToString(), Session["hashedPassword"].ToString());
                    sqlParameters.Add(sqlParameter);

                    Db.ExecuteNonQuery("UPDATE anon SET encryptedLastLoginDateTime=@encryptedLastLoginDateTime WHERE anonId=@anonId", out boolDbError, out strDbError, sqlParameters);

                    if (Request.QueryString["goAfter"] != null)
                    {
                        if (Request.QueryString["code"] != null)
                        {
                            Response.Redirect(Request.QueryString["goAfter"].ToString() + "?code=" + HttpUtility.UrlEncode(Request.QueryString["code"].ToString()));
                            Response.End();
                        }
                        else if (Request.QueryString["action"] != null&& Request.QueryString["postId"] != null)
                        {
                            if (Request.QueryString["action"].ToString() == "reply") ;
                            {
                                //"Login?goAfter=Post&action=reply&postId=" + Request.QueryString["postId"].ToString()
                                Response.Redirect("Post?action=reply&postid=" + Request.QueryString["postId"].ToString());
                            }
                            
                        }
                        Response.Redirect(Request.QueryString["goAfter"].ToString());
                        Response.End();
                    }

                    Response.Redirect("Nation");
                    Response.End();
                }
                else
                {
                    lblError.Text = "Incorrect email/password. Code: 2";



                }
            }
            else
            {
                lblError.Text = "Incorrect email/password. Code: 1";
            }

            if (Session["loginAttempts"] == null)
            {
                Session["loginAttempts"] = "1";
                Session["lastLoginAttemptTimestamp"] = DateTime.Now.ToString();
            }
            else
            {
                if (int.TryParse(Session["loginAttempts"].ToString(), out int intLoginAttempts))
                {
                    if (intLoginAttempts < 4)
                    {
                        intLoginAttempts++;
                        Session["loginAttempts"] = intLoginAttempts.ToString();
                        Session["lastLoginAttemptTimestamp"] = DateTime.Now.ToString();
                    }
                    else
                    {
                        //Session["lastLoginAttemptTimestamp"] = DateTime.Now.ToString();
                        lblError.Text = "Too many attempts, try again in 15 minutes.";
                    }
                }
                else
                {
                    //shouldn't reach here???
                    Session["loginAttempts"] = "0";
                }

            }

        }

    }
}