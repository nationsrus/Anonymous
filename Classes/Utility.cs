using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Anonymous.Classes;
using System.Data;
using System.Net.Mail;
using System.Configuration;
using System.Net.Mime;

namespace Anonymous.Classes
{
    public class Utility
    {
        public static void ddlNationLoad(System.Web.UI.WebControls.DropDownList ddl)
        {
            ddl.DataSource = Db.ExecuteQuery("SELECT nationId,commonName FROM nation ORDER BY nationId", out bool boolDbError, out string strDbError);
            ddl.DataTextField = "commonName";
            ddl.DataValueField = "nationId";
            ddl.DataBind();
        }

        public static bool SendEmail(string emailAddress, string name, string subjectTitle, string bodyText, string bodyHtml = "")
        {
            try
            {
                if (bodyHtml == string.Empty)
                {
                    bodyHtml = bodyText;
                }

                MailMessage mailMsg = new MailMessage();

                // To
                mailMsg.To.Add(new MailAddress(emailAddress, name));

                // From
                mailMsg.From = new MailAddress("admin@nationsrus.com", "NationsRUs");

                // Subject and multipart/alternative Body
                mailMsg.Subject = subjectTitle;
                string text = bodyText;
                string html = bodyHtml;
                mailMsg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, null, MediaTypeNames.Text.Plain));
                mailMsg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html, null, MediaTypeNames.Text.Html));

                // Init SmtpClient and send
                SmtpClient smtpClient = new SmtpClient("smtp.sendgrid.net", Convert.ToInt32(587));
                System.Net.NetworkCredential credentials = new System.Net.NetworkCredential("apikey", ConfigurationManager.AppSettings["SendGridKey"].ToString());
                smtpClient.Credentials = credentials;

                smtpClient.Send(mailMsg);

                //Response.Write("<br><br>success SendGrid()");

                return true;
            }
            catch (Exception ex)
            {
                //Response.Write("<br><br>SendGrid() error: " + ex.Message);

                return false;
            }
        }
     }
}