using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Anonymous.Classes;

namespace Anonymous
{
    public partial class EmailVerify : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (!Security.IsLoggedIn())
                {
                    if (Request.QueryString["code"] != null)
                    {
                        Response.Redirect("Login?goAfter=EmailVerify&code=" + HttpUtility.UrlEncode(Request.QueryString["code"]));
                        Response.End();
                    }
                    Response.Redirect("Login?goAfter=EmailVerify");
                    Response.End();
                }

                if (Request.QueryString["code"] != null)
                {
                    verifyCode(Request.QueryString["code"].ToString());
                }
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (Security.IsLoggedIn())
            {
                verifyCode(tbxCode.Text);
            }
            else
            {
                Response.Redirect("Login?goAfter=EmailVerify");
                Response.End();
            }
        }

        private void verifyCode(string emailVerificationCode)
        {

            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            SqlParameter sqlParameter = new SqlParameter();

            sqlParameters = new List<SqlParameter>();

            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@anonId";
            sqlParameter.Value = Session["anonId"].ToString();
            sqlParameters.Add(sqlParameter);

            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@emailVerificationCode";
            sqlParameter.Value = emailVerificationCode;
            sqlParameters.Add(sqlParameter);

            DataSet dataSetFriend = Db.ExecuteQuery("SELECT emailVerified FROM anon WHERE anonId=@anonId AND emailVerificationCode=@emailVerificationCode", out bool boolDbError, out string strDbError, sqlParameters);
            if (dataSetFriend.Tables.Count > 0 && dataSetFriend.Tables[0].Rows.Count > 0)
            {

                sqlParameters = new List<SqlParameter>();

                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@anonId";
                sqlParameter.Value = Session["anonId"].ToString();
                sqlParameters.Add(sqlParameter);

                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@emailVerificationCode";
                sqlParameter.Value = emailVerificationCode;
                sqlParameters.Add(sqlParameter);

                Db.ExecuteNonQuery("UPDATE anon SET emailVerified = 1 WHERE anonId = @anonId AND emailVerificationCode = @emailVerificationCode", out boolDbError, out strDbError, sqlParameters);

                lblSystemMsg.Visible = false;
                pnlVerify.Visible = false;
                pnlVerified.Visible = true;
            }
            else
            {

                lblSystemMsg.Text = "Code did not match.";

            }

        }
    }
}