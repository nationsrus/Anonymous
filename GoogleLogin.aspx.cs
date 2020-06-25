﻿using System;
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
    public partial class GoogleLogin : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {


            if (!IsPostBack)
            {
                var serv = Request.Url.GetLeftPart(UriPartial.Authority);
                var str = "https://accounts.google.com/o/oauth2/v2/auth" +
                    "?scope=" + HttpUtility.UrlEncode("https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/userinfo.email") +
                    "&response_type=" + "code" +
                    "&access_type=offline" +
                    "&client_id=" + ConfigurationManager.AppSettings["google_app_id"] +
                    "&state=" + "test" +
                    "&redirect_uri=" + HttpUtility.UrlEncode(serv + "/GoogleLogin2?action=google");

                Response.Redirect(str);

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