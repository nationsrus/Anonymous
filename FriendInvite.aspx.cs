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
    public partial class FriendInvite : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Security.IsLoggedInOrKick("FriendInvite");
            }
        }

        protected void btnSendInvite_Click(object sender, EventArgs e)
        {
            //TODO check if email address
            if (Security.isRecaptchaChallengeSuccesful())
            {
                if (Db.Common.DoesEmailAlreadyExistInDb(tbxFriendEmail.Text.ToLower().Trim(), out byte[] encryptedEmail, out DataSet dataSet))
                {
                    //lblInviteError.Visible = true;
                    //lblInviteError.Text = "Due to security of the system, a failure or success message will not be displayed.";

                    // send friendship request???

                }
                else
                {
                    //lblInviteError.Visible = false;

                    List<SqlParameter> sqlParameters = new List<SqlParameter>();
                    SqlParameter sqlParameter = new SqlParameter();

                    string FriendEmail = tbxFriendEmail.Text.ToLower().Trim();

                    string randomPassword = System.Web.Security.Membership.GeneratePassword(12, 2).ToUpper().Trim();

                    //string friendHashKey = Security.EncryptStringAES(randomPassword, ConfigurationManager.AppSettings["AppSalt"].ToString());
                    string friendSecurityKey = FriendEmail + randomPassword + ConfigurationManager.AppSettings["AppSalt"].ToString();

                    sqlParameter = new SqlParameter();
                    sqlParameter.ParameterName = "@FriendEmailAddress";
                    sqlParameter.Value = Security.EncryptStringAES(FriendEmail, friendSecurityKey);
                    sqlParameters.Add(sqlParameter);

                    sqlParameter = new SqlParameter();
                    sqlParameter.ParameterName = "@NickName";
                    sqlParameter.Value = Security.EncryptStringAES(tbxNickname.Text, friendSecurityKey);
                    sqlParameters.Add(sqlParameter);

                    sqlParameter = new SqlParameter();
                    sqlParameter.ParameterName = "@EmailAddress";
                    if (tbxInviteShareEmailAddress.Text == string.Empty)
                    {
                        sqlParameter.Value = DBNull.Value;
                    }
                    else
                    {
                        sqlParameter.Value = Security.EncryptStringAES(tbxInviteShareEmailAddress.Text, friendSecurityKey);
                    }
                    sqlParameters.Add(sqlParameter);

                    sqlParameter = new SqlParameter();
                    sqlParameter.ParameterName = "@Msg";
                    if (tbxCustomMessage.Text == string.Empty)
                    {
                        sqlParameter.Value = DBNull.Value;
                    }
                    else
                    {
                        sqlParameter.Value = Security.EncryptStringAES(tbxCustomMessage.Text, friendSecurityKey);
                    }
                    sqlParameters.Add(sqlParameter);

                    sqlParameter = new SqlParameter();
                    sqlParameter.ParameterName = "@anonId";
                    sqlParameter.Value = Security.EncryptStringAES(Session["anonId"].ToString(), friendSecurityKey);
                    sqlParameters.Add(sqlParameter);

                    sqlParameter = new SqlParameter();
                    sqlParameter.ParameterName = "@code";
                    sqlParameter.Value = randomPassword;
                    sqlParameters.Add(sqlParameter);

                    Db.ExecuteNonQuery("INSERT INTO friendInvite(FriendEmailAddress,NickName,EmailAddress,Msg,anonId,code) VALUES(@FriendEmailAddress,@NickName,@EmailAddress,@Msg,@anonId,@code)", out bool boolDbError, out string strDbError, sqlParameters);


                    string strTextEmailBody = string.Empty;
                    strTextEmailBody = "Hi there, your friend, " + tbxNickname.Text;
                    if (tbxInviteShareEmailAddress.Text != string.Empty)
                    {
                        strTextEmailBody += " (" + tbxInviteShareEmailAddress.Text + ")";
                    }
                    strTextEmailBody += ", sent you an invite to join " + ConfigurationManager.AppSettings["url"].ToString() + " a secure open source forum for discussions regarding governments, culture, foreign and local affairs. Use this link to register " + ConfigurationManager.AppSettings["url"].ToString() + "/Register?code=" + HttpUtility.UrlEncode(randomPassword) + " or go to " + ConfigurationManager.AppSettings["url"].ToString() + "/Register and use this code " + randomPassword + " when registering.";

                    if (tbxCustomMessage.Text != string.Empty)
                    {
                        strTextEmailBody += "Custom message from your friend: " + tbxCustomMessage.Text;
                    }

                    Utility.SendEmail(FriendEmail, "", ConfigurationManager.AppSettings["websiteName"].ToString() + " Friend Invite", strTextEmailBody);

                }

                lblInviteError.Text = "Invite logic executed. No status will be shown for security reasons.";
                lblInviteError.Visible = true;
            }
            else
            {
                lblInviteError.Text = "Recaptcha error. Please try again or stop being like a robot :p";
                lblInviteError.Visible = true;
            }
        }


    }
}