using Anonymous.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Anonymous
{
    public partial class PostAdd : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
            {
                Utility.ddlNationLoad(ddlNation);

                if (Security.IsLoggedIn())
                {
                    if (!Security.IsEmailVerified)
                    {
                        tbxDescription.Enabled = false;
                        tbxDescription.Text = "[Must have email verified to post]";
                    }
                }
                else
                {
                    //tbxPostTitle.Enabled = false;
                    //tbxPostTitle.Text = "[Must be logged in to post]";
                    tbxDescription.Enabled = false;
                    tbxDescription.Text = "[Must be logged in to post]";

                }
            
            }

        }

        protected void btnPostOnClick(object sender, EventArgs e)
        {
            if (Security.IsLoggedIn())
            {
                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                SqlParameter sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@anonId";
                sqlParameter.Value = Session["anonId"].ToString();
                sqlParameters.Add(sqlParameter);

                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@nationId";
                sqlParameter.Value = ddlNation.SelectedValue;
                sqlParameters.Add(sqlParameter);

                //sqlParameter = new SqlParameter();
                //sqlParameter.ParameterName = "@Title";
                //sqlParameter.Value = tbxPostTitle.Text;
                //sqlParameters.Add(sqlParameter);

                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@msg";
                sqlParameter.Value = tbxDescription.Text;
                sqlParameters.Add(sqlParameter);

                //DataSet ds = Db.ExecuteQuery("postCreate", out bool boolDbError, out string strDbError, sqlParameters, CommandType.StoredProcedure);
                DataSet ds = Db.ExecuteQuery("INSERT INTO post(nationId,anonId,msg) VALUES(@nationId,@anonId,@msg); SELECT SCOPE_IDENTITY();", out bool boolDbError, out string strDbError, sqlParameters);

                Response.Redirect("post?postId=" + ds.Tables[0].Rows[0][0].ToString());
            }
            else
            {
                Response.Redirect("Login");
            }
        }


    }
}