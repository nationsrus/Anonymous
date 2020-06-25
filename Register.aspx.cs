using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Anonymous.Classes;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Anonymous
{
    public partial class Register : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString["code"] != null)
                {
                    tbxCode.Text = Request.QueryString["code"].ToString();
                    tbxCode.Enabled = false;


                }
            }
        }

        protected void btnRegisterOnClick(object sender, EventArgs e)
        {
            registration();
        }

        private void registration()
        {
            string strDbError;
            bool boolDbError;
            bool invitedFriendHasCorrectEmailAddress = false;
            string friendAnonId = string.Empty, nickname = string.Empty, EmailAddress = string.Empty;
            string email = tbxEmail.Text.ToLower().Trim();
            string emailAlternate = tbxInvitedEmailAddress.Text.ToLower().Trim();

            if (Db.Common.DoesEmailAlreadyExistInDb(email, out byte[] encryptedEmail, out DataSet dataSet))
            {
                lblRegistrationError.Visible = true;
            }
            else
            {
                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                SqlParameter sqlParameter = new SqlParameter();

                if (tbxCode.Text != string.Empty)
                {

                    sqlParameters = new List<SqlParameter>();
                    sqlParameter = new SqlParameter();
                    sqlParameter.ParameterName = "@code";
                    sqlParameter.Value = tbxCode.Text;
                    sqlParameters.Add(sqlParameter);

                    DataSet dataSetFriendInvite = Db.ExecuteQuery("SELECT * FROM friendInvite WHERE code=@code", out boolDbError, out strDbError, sqlParameters);

                    if (dataSetFriendInvite.Tables.Count > 0 && dataSetFriendInvite.Tables[0].Rows.Count > 0)
                    {
                        //string friendHashKey = Encoding.Default.GetString(Security.Hash(tbxEmail.Text + tbxCode.Text + ConfigurationManager.AppSettings["AppSalt"]));

                        string code = tbxCode.Text.ToUpper().Trim();
                        //string friendHashKey = Security.EncryptStringAES(code, ConfigurationManager.AppSettings["AppSalt"].ToString());
                        string friendSecurityKey = email + code + ConfigurationManager.AppSettings["AppSalt"].ToString();
                        string friendSecurityKey2 = emailAlternate + code + ConfigurationManager.AppSettings["AppSalt"].ToString();

                        bool doesEmailMatch = false, doesAlternativeEmailMatch = false;

                        if (email != string.Empty)
                        {
                            if (dataSetFriendInvite.Tables[0].Rows[0]["FriendEmailAddress"].ToString() == Security.EncryptStringAES(email, friendSecurityKey))
                            { 
                                doesEmailMatch = true; 
                            }
                            if (Security.DecryptStringAES(dataSetFriendInvite.Tables[0].Rows[0]["FriendEmailAddress"].ToString(), friendSecurityKey) == email)
                            {
                                doesEmailMatch = true;
                            }
                        }
                        if (emailAlternate != string.Empty)
                        {
                            if (dataSetFriendInvite.Tables[0].Rows[0]["FriendEmailAddress"].ToString() == Security.EncryptStringAES(emailAlternate, friendSecurityKey2))
                            { 
                                doesAlternativeEmailMatch = true; 
                            }
                            if (Security.DecryptStringAES(dataSetFriendInvite.Tables[0].Rows[0]["FriendEmailAddress"].ToString(), friendSecurityKey2) == emailAlternate)
                            {
                                doesEmailMatch = true;
                            }

                        }

                            
                        if (doesEmailMatch || doesAlternativeEmailMatch)
                        {
                            invitedFriendHasCorrectEmailAddress = true;
                            friendAnonId = Security.DecryptStringAES(dataSetFriendInvite.Tables[0].Rows[0]["anonId"].ToString(), friendSecurityKey);
                            if (dataSetFriendInvite.Tables[0].Rows[0]["NickName"] != DBNull.Value)
                            {
                                nickname = Security.DecryptStringAES(dataSetFriendInvite.Tables[0].Rows[0]["NickName"].ToString(), friendSecurityKey);
                            }
                            if (dataSetFriendInvite.Tables[0].Rows[0]["EmailAddress"] != DBNull.Value)
                            {
                                EmailAddress = Security.DecryptStringAES(dataSetFriendInvite.Tables[0].Rows[0]["EmailAddress"].ToString(), friendSecurityKey);
                            }

                            sqlParameters = new List<SqlParameter>();
                            sqlParameter = new SqlParameter();
                            sqlParameter.ParameterName = "@code";
                            sqlParameter.Value = tbxCode.Text;
                            sqlParameters.Add(sqlParameter);

                            Db.ExecuteNonQuery("DELETE friendInvite WHERE code=@code OR insertTimeStamp < GetDate()-60", out boolDbError, out strDbError, sqlParameters);
                        }
                        else
                        {
                            SkipInvitedEmailRow.Visible = true;
                            InvitedEmailAddressRow.Visible = true;
                            lblRegistrationError.Text = "The code doesn't match the email address.";
                        }
                    }
                    else
                    {
                        lblRegistrationError.Text = "Invite code does not exist. Re-enter the code and try again.";
                    }

                }

                if (
                    tbxCode.Text != string.Empty
                    || cbxSkipInvitedEmailAddress.Checked
                    || invitedFriendHasCorrectEmailAddress)
                {

                    string emailVerificationCode = System.Web.Security.Membership.GeneratePassword(7, 3);

                    sqlParameters = new List<SqlParameter>();
                    sqlParameter = new SqlParameter();
                    sqlParameter.ParameterName = "@email";
                    sqlParameter.Value = encryptedEmail;
                    sqlParameters.Add(sqlParameter);

                    sqlParameter = new SqlParameter();
                    sqlParameter.ParameterName = "@encryptedPassword";
                    sqlParameter.Value = BCrypt.Net.BCrypt.EnhancedHashPassword(tbxPassword1.Text); ;
                    sqlParameters.Add(sqlParameter);

                    string hashedPassword = System.Text.Encoding.Default.GetString(Security.Hash(tbxPassword1.Text, tbxEmail.Text.ToLower().Trim() + ConfigurationManager.AppSettings["AppSalt"]));

                    string publicOnlyKeyXML = string.Empty, strEncryptedPublicPrivateKeyXML = string.Empty;

                    using (var rsa = new RSACryptoServiceProvider(1024))
                    {
                        try
                        {
                            string publicPrivateKeyXML = rsa.ToXmlString(true);
                            strEncryptedPublicPrivateKeyXML = Security.EncryptStringAES(rsa.ToXmlString(true), hashedPassword);
                            publicOnlyKeyXML = rsa.ToXmlString(false);
                        }
                        finally
                        {
                            rsa.PersistKeyInCsp = false;
                        }
                    }

                    sqlParameter = new SqlParameter();
                    sqlParameter.ParameterName = "@encryptedPrivateKey";
                    sqlParameter.Value = strEncryptedPublicPrivateKeyXML;
                    sqlParameters.Add(sqlParameter);

                    sqlParameter = new SqlParameter();
                    sqlParameter.ParameterName = "@PublicKey";
                    sqlParameter.Value = publicOnlyKeyXML;
                    sqlParameters.Add(sqlParameter);

                    sqlParameter = new SqlParameter();
                    sqlParameter.ParameterName = "@emailVerificationCode";
                    sqlParameter.Value = emailVerificationCode;
                    sqlParameters.Add(sqlParameter);


                    DataSet dataSetNewAnon =Db.ExecuteQuery("INSERT INTO anon(email,encryptedPassword,encryptedPrivateKey,PublicKey,emailVerificationCode) VALUES(@email,@encryptedPassword,@encryptedPrivateKey,@PublicKey,@emailVerificationCode); SELECT SCOPE_IDENTITY();", out boolDbError, out strDbError, sqlParameters);

                    if (invitedFriendHasCorrectEmailAddress)
                    {
                        Session["anonId"] = dataSetNewAnon.Tables[0].Rows[0][0].ToString();
                        Session["email"] = tbxEmail.Text.ToLower().Trim();
                        Session["hashedPassword"] = System.Text.Encoding.Default.GetString(Security.Hash(tbxPassword1.Text, tbxEmail.Text.ToLower().Trim() + ConfigurationManager.AppSettings["AppSalt"]));

                        sqlParameters = new List<SqlParameter>();

                        sqlParameter = new SqlParameter();
                        sqlParameter.ParameterName = "@anonId";
                        sqlParameter.Value = dataSetNewAnon.Tables[0].Rows[0][0].ToString();
                        sqlParameters.Add(sqlParameter);

                        sqlParameter = new SqlParameter();
                        sqlParameter.ParameterName = "@nickName";
                        if (nickname == string.Empty)
                        { 
                            sqlParameter.Value = DBNull.Value;
                        }
                        else
                        {
                            sqlParameter.Value = Security.EncryptStringAES(nickname, Session["hashedPassword"].ToString());
                        }
                        sqlParameters.Add(sqlParameter);


                        sqlParameter = new SqlParameter();
                        sqlParameter.ParameterName = "@email";
                        if (EmailAddress == string.Empty)
                        {
                            sqlParameter.Value = DBNull.Value;
                        }
                        else
                        {
                            sqlParameter.Value = Security.EncryptStringAES(EmailAddress, Session["hashedPassword"].ToString());
                        }
                        sqlParameters.Add(sqlParameter);


                        sqlParameter = new SqlParameter();
                        sqlParameter.ParameterName = "@encryptedFriendAnonId";
                        sqlParameter.Value = Security.EncryptStringAES(friendAnonId, Session["hashedPassword"].ToString());
                        sqlParameters.Add(sqlParameter);


                        Db.ExecuteNonQuery("INSERT INTO friend(anonId,nickName,email,encryptedFriendAnonId) VALUES(@anonId,@nickName,@email,@encryptedFriendAnonId)", out boolDbError, out strDbError, sqlParameters);

                        //insert self row for friendRequest

                        sqlParameters = new List<SqlParameter>();

                        sqlParameter = new SqlParameter();
                        sqlParameter.ParameterName = "@anonId";
                        sqlParameter.Value = friendAnonId;
                        sqlParameters.Add(sqlParameter);

                        string randomAutoGeneratedPassword = System.Web.Security.Membership.GeneratePassword(40, 12);

                        DataSet dsFriendAnon = Db.Common.AnonByAnonId(out boolDbError, out strDbError, sqlParameters);

                        sqlParameters = new List<SqlParameter>();

                        sqlParameter = new SqlParameter();
                        sqlParameter.ParameterName = "@anonId";
                        sqlParameter.Value = friendAnonId;
                        sqlParameters.Add(sqlParameter);

                        sqlParameter = new SqlParameter();
                        sqlParameter.ParameterName = "@EncryptionKeyEncryptedByPublicKey";
                        sqlParameter.Value = Security.Rsa.Encrypt(randomAutoGeneratedPassword, dsFriendAnon.Tables[0].Rows[0]["PublicKey"].ToString()); // encrypted with friend's public key
                        sqlParameters.Add(sqlParameter);

                        sqlParameter = new SqlParameter();
                        sqlParameter.ParameterName = "@encryptedAnonIdRequestingFriendship";
                        sqlParameter.Value = Security.EncryptStringAES(Session["anonId"].ToString(), randomAutoGeneratedPassword);
                        sqlParameters.Add(sqlParameter);

                        Db.ExecuteNonQuery("INSERT INTO friendRequest(anonId,encryptedAnonIdRequestingFriendship,EncryptionKeyEncryptedByPublicKey,Accepted) VALUES(@anonId,@encryptedAnonIdRequestingFriendship,@EncryptionKeyEncryptedByPublicKey,1);", out boolDbError, out strDbError, sqlParameters);

                        Session.Clear();
                    }

                    pnlRegistration.Visible = false;
                    pnlAccountCreated.Visible = true;

                    string emailText = string.Empty;
                    string emailHtml = string.Empty;
                    emailText = "Your email verification code for " + ConfigurationManager.AppSettings["websiteName"].ToString() + " is: " + emailVerificationCode + @" . Please go to " + ConfigurationManager.AppSettings["url"].ToString() + "/EmailVerify?code=" + HttpUtility.UrlEncode(emailVerificationCode) + " or " + ConfigurationManager.AppSettings["url"].ToString() + "/EmailVerify and use the code to verify your email address.";

                    Utility.SendEmail(tbxEmail.Text.ToLower().Trim(), string.Empty, "NationsRUs.com Email Verification", emailText);
                }
            }

        }

    }
}