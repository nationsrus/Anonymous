using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Anonymous
{
    public partial class GoogleLogin2 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var access_code = Request.QueryString["code"];

            if (access_code == null)
            {
                return;
            }

            var serv = Request.Url.GetLeftPart(UriPartial.Authority);
            var access_token = "";

            if (!GetAccessToken(access_code, HttpUtility.UrlEncode(serv + "/GoogleLogin2?action=google"), out access_token))
            {
                return;
            }

            var res = "";
            var web = new WebClient();

            web.Encoding = System.Text.Encoding.UTF8;

            try
            {
                res = web.DownloadString("https://www.googleapis.com/oauth2/v1/userinfo?access_token=" + access_token);
                //Response.Write("success...?: "+res);
                Response.Write("Success! Awaiting for google authorization for next modules.");
            }
            catch (Exception ex)
            {
                Response.Write("error: " + ex.ToString());
            }

        }

        public bool GetAccessToken(string access_code, string redirect_url, out string token)
        {
            try
            {
                var clien_id = ConfigurationManager.AppSettings["google_app_id"];
                var clien_secret = ConfigurationManager.AppSettings["google_app_secret"];

                var webRequest = (HttpWebRequest)WebRequest.Create("https://www.googleapis.com/oauth2/v4/token");

                webRequest.Method = "POST";

                // https://nationsrus.com/GoogleLogin2?action=google&state=test&code=4%2F0gE91zVpqToFkq__jb8OzP3h_5T8_Kb0uxIao3TFwvmrNblFH_ifrQhl43EYg8NNIRDMcdo1phQs9URsitMbZFU&scope=email+profile+openid+https%3A%2F%2Fwww.googleapis.com%2Fauth%2Fuserinfo.email+https%3A%2F%2Fwww.googleapis.com%2Fauth%2Fuserinfo.profile&authuser=0&prompt=none#
                string parameters = $"code={access_code}&client_id={clien_id}&client_secret={clien_secret}&redirect_uri={redirect_url}&grant_type=authorization_code";

                var byteArray = Encoding.UTF8.GetBytes(parameters);

                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.ContentLength = byteArray.Length;

                var postStream = webRequest.GetRequestStream();

                // Add the post data to the web request
                postStream.Write(byteArray, 0, byteArray.Length);
                postStream.Close();

                var response = webRequest.GetResponse();
                postStream = response.GetResponseStream();

                var reader = new StreamReader(postStream);
                var tmp = reader.ReadToEnd();

                var pat = "\"access_token\"";
                var ind = tmp.IndexOf(pat);

                if (ind != -1)
                {
                    ind += pat.Length;

                    ind = tmp.IndexOf("\"", ind);

                    if (ind != -1)
                    {
                        var end = tmp.IndexOf("\"", ind + 1);

                        if (end != -1)
                        {
                            token = tmp.Substring(ind + 1, end - ind - 1);

                            return true;
                        }
                    }
                }

                token = tmp;
            }
            catch (Exception e)
            {
                Response.Write(e);

                token = e.Message;
            }

            return false;
        }
    }
}