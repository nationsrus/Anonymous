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
    public partial class anon : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if ( Request.QueryString["anonId"]!=null)
                {
                    load(Request.QueryString["anonId"].ToString());
                }
                else
                {
                    Response.Redirect("/");
                }
            }
            
        }

        private void load(string anonId)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("anonId");
            dataTable.Columns.Add("postId");
            dataTable.Columns.Add("post150character");
            dataTable.Columns.Add("friendActivity");
            dataTable.Columns.Add("activityTimeStamp");

            //get list of friends
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            SqlParameter sqlParameter = new SqlParameter();



            //get friends recent posts
            sqlParameters = new List<SqlParameter>();
            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@anonId";
            sqlParameter.Value = anonId;
            sqlParameters.Add(sqlParameter);

            DataSet dataSetFriendPosts = Db.ExecuteQuery("SELECT TOP 30 postId, anonId, insertTimeStamp, msg FROM post WHERE anonId = @anonId ORDER BY insertTimeStamp DESC", out bool boolDbError, out string strDbError, sqlParameters);

            if (dataSetFriendPosts.Tables.Count > 0 && dataSetFriendPosts.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in dataSetFriendPosts.Tables[0].Rows)
                {
                    DataRow dataRow = dataTable.NewRow();

                    dataRow["anonId"] = row["anonId"].ToString();
                    dataRow["postId"] = row["postId"].ToString();
                    if (row["msg"].ToString().Length < 120)
                    {
                        dataRow["post150character"] = row["msg"].ToString();
                    }
                    else
                    {
                        dataRow["post150character"] = row["msg"].ToString().Substring(0, 120) + " ...";
                    }

                    dataRow["friendActivity"] = "Posted by friend";
                    dataRow["activityTimeStamp"] = row["insertTimeStamp"].ToString();

                    dataTable.Rows.InsertAt(dataRow, 0);
                }
            }

            rptrPosts.DataSource = dataTable;
            rptrPosts.DataBind();


            //get friends recent votes
            dataTable = new DataTable();
            dataTable.Columns.Add("anonId");
            dataTable.Columns.Add("postId");
            dataTable.Columns.Add("post150character");
            dataTable.Columns.Add("friendActivity");
            dataTable.Columns.Add("activityTimeStamp");

            sqlParameters = new List<SqlParameter>();
            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@anonId";
            sqlParameter.Value = anonId;
            sqlParameters.Add(sqlParameter);

            DataSet dataSetFriendVotes = Db.ExecuteQuery("SELECT TOP 10 v.postId, v.anonId, msg, v.insertTimeStamp, upVote FROM vote v JOIN post p ON v.postId = p.postId WHERE AND v.anonId = @anonId ORDER BY v.insertTimeStamp DESC", out boolDbError, out strDbError, sqlParameters);

            if (dataSetFriendVotes.Tables.Count > 0 && dataSetFriendVotes.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in dataSetFriendVotes.Tables[0].Rows)
                {
                    DataRow dataRow = dataTable.NewRow();

                    dataRow["anonId"] = row["anonId"].ToString();
                    dataRow["postId"] = row["postId"].ToString();
                    if (row["msg"].ToString().Length < 120)
                    {
                        dataRow["post150character"] = row["msg"].ToString();
                    }
                    else
                    {
                        dataRow["post150character"] = row["msg"].ToString().Substring(0, 120) + " ...";
                    }

                    string friendActivity = "Voted";
                    if (row["upVote"].ToString() == "-1")
                    {
                        friendActivity += " down";
                    }
                    else
                    {
                        friendActivity += " up";
                    }
                    dataRow["friendActivity"] = friendActivity;
                    dataRow["activityTimeStamp"] = row["insertTimeStamp"].ToString();

                    dataTable.Rows.InsertAt(dataRow, 0);
                }
            }

            rptrVotes.DataSource = dataTable;
            rptrVotes.DataBind();

        }

    }
}