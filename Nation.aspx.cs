using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Anonymous.Classes;
using System.Data;
using System.Data.SqlClient;

namespace Anonymous
{
    public partial class Nation : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Utility.ddlNationLoad(ddlNation);

                if (Session["nationId"] == null)
                {
                    Session["nationId"] = ddlNation.SelectedValue;
                }
                else
                {
                    ddlNation.SelectedValue = Session["nationId"].ToString();
                }

                rptrPosts_load();
                rptrRecentPosts_load();

            }
        }


        protected void btnNationChangeOnClick(object sender, EventArgs e)
        {
            Session["nationId"] = ddlNation.SelectedValue;
            rptrPosts_load();
            rptrRecentPosts_load();
        }

        private void rptrPosts_load()
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@nationId";
            sqlParameter.Value = Session["nationId"].ToString();
            sqlParameters.Add(sqlParameter);
            DataSet ds = Db.ExecuteQuery("SELECT TOP 50 * FROM post WHERE nationId = @nationId ORDER BY views DESC, insertTimeStamp DESC", out bool boolDbError, out string strDbError, sqlParameters);

            if (Security.IsLoggedIn())
            {
                sqlParameters = new List<SqlParameter>();
                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@nationId";
                sqlParameter.Value = Session["nationId"].ToString();
                sqlParameters.Add(sqlParameter);

                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@LastLoginDateTime";
                sqlParameter.Value = Session["LastLoginDateTime"].ToString();
                sqlParameters.Add(sqlParameter);

                DataSet ds2 = new DataSet();
                ds2 = Db.ExecuteQuery("SELECT TOP 50 * FROM post WHERE nationId = @nationId AND insertTimeStamp>@LastLoginDateTime ORDER BY views DESC,insertTimeStamp DESC", out boolDbError, out strDbError, sqlParameters);

                if (ds2.Tables.Count > 0 && ds2.Tables[0].Rows.Count > 0)
                { 
                    ds = ds2; 
                } 
            }

            rptrPosts.DataSource = ds;
            rptrPosts.DataBind();
        }

        private void rptrRecentPosts_load()
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@nationId";
            sqlParameter.Value = Session["nationId"].ToString();
            sqlParameters.Add(sqlParameter);
            DataSet ds = Db.ExecuteQuery("SELECT TOP 50 * FROM post WHERE nationId = @nationId ORDER BY insertTimeStamp DESC", out bool boolDbError, out string strDbError, sqlParameters);

            rptrRecentPosts.DataSource = ds;
            rptrRecentPosts.DataBind();
        }

        protected void rptrPosts_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                string msg = DataBinder.Eval(e.Item.DataItem, "msg").ToString();
                Literal ltlMsgFirstBit = e.Item.FindControl("ltlMsgFirstBit") as Literal;
                if (msg.Length > 150)
                {
                    ltlMsgFirstBit.Text = msg.Substring(0,150) + " ... ";
                }
                else
                {
                    ltlMsgFirstBit.Text = msg;
                }

            }
        }
    }
}