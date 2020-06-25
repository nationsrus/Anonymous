using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Anonymous.Classes;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Configuration;

namespace Anonymous
{
    public partial class Me : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Security.IsLoggedInOrKick();

                loadAnonInfo();

                lblEmail.Text = Session["email"].ToString();
                ltlAnonId.Text = Session["anonId"].ToString();

                rptrMyActivity_load();
            }
        }

        protected void btnSaveOnClick(object sender, EventArgs e)
        {

            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@anonId";
            sqlParameter.Value = Session["anonId"];
            sqlParameters.Add(sqlParameter);

            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@FriendRequestRequireEmailAddress";
            if (cbxFriendRequestsRequireEmailAddress.Checked)
            {
                sqlParameter.Value = 1;
            }
            else
            {
                sqlParameter.Value = 0;
            }
            sqlParameters.Add(sqlParameter);

            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@PublicName";
            if (tbxPublicName.Text == string.Empty)
            {
                sqlParameter.Value = DBNull.Value;
            }
            else
            {
                sqlParameter.Value = tbxPublicName.Text;
            }
            sqlParameters.Add(sqlParameter);

            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@PublicCity";
            if (tbxPublicCity.Text == string.Empty)
            {
                sqlParameter.Value = DBNull.Value;
            }
            else
            {
                sqlParameter.Value = tbxPublicCity.Text;
            }
            sqlParameters.Add(sqlParameter);

            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@PublicNation";
            if (tbxPublicNation.Text == string.Empty)
            {
                sqlParameter.Value = DBNull.Value;
            }
            else
            {
                sqlParameter.Value = tbxPublicNation.Text;
            }
            sqlParameters.Add(sqlParameter);

            Db.ExecuteNonQuery("UPDATE anon SET FriendRequestRequireEmailAddress = @FriendRequestRequireEmailAddress, PublicName=@PublicName,PublicCity=@PublicCity,PublicNation=@PublicNation WHERE anonId = @anonId", out bool boolDbError, out string strDbError, sqlParameters);

            //lblSystemMsg.Text = "Saved at " + DateTime.Now.ToString();
            lblPublicInfoSave.Text= "Saved at " + DateTime.Now.ToString();
        }

        private void loadAnonInfo()
        {

            List<SqlParameter> sqlParameters = new List<SqlParameter>();

            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@anonId";
            sqlParameter.Value = Session["anonId"];
            sqlParameters.Add(sqlParameter);
            DataSet dataSet = Db.ExecuteQuery("SELECT * FROM anon WHERE anonId = @anonId", out bool boolDbError, out string strDbError, sqlParameters);

            if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                bool FriendRequestRequireEmailAddress = false;
                bool.TryParse(dataSet.Tables[0].Rows[0]["FriendRequestRequireEmailAddress"].ToString(), out FriendRequestRequireEmailAddress);
                cbxFriendRequestsRequireEmailAddress.Checked = FriendRequestRequireEmailAddress;
                tbxPublicName.Text = dataSet.Tables[0].Rows[0]["PublicName"].ToString();
                tbxPublicCity.Text = dataSet.Tables[0].Rows[0]["PublicCity"].ToString();
                tbxPublicNation.Text = dataSet.Tables[0].Rows[0]["PublicNation"].ToString();

                if (bool.TryParse(dataSet.Tables[0].Rows[0]["emailVerified"].ToString(), out bool emailVerified) && emailVerified)
                { 
                    lblVerified.Visible = true;
                    lblNotVerified.Visible = false;
                }
                else
                {
                    lblVerified.Visible = false;
                    lblNotVerified.Visible = true;
                }

            }

        }

        private void rptrMyActivity_load()
        {

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("anonId");
            dataTable.Columns.Add("postId");
            dataTable.Columns.Add("post150character");
            dataTable.Columns.Add("friendActivity");
            dataTable.Columns.Add("activityTimeStamp");


            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            SqlParameter sqlParameter = new SqlParameter();

            //get votes
            sqlParameters = new List<SqlParameter>();
            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@anonId";
            sqlParameter.Value = Session["anonId"].ToString();
            sqlParameters.Add(sqlParameter);

            DataSet dataMyVotes = Db.ExecuteQuery("SELECT TOP 20 v.postId, v.anonId, msg, v.insertTimeStamp, upVote FROM vote v JOIN post p ON v.postId = p.postId WHERE v.anonId = @anonId ORDER BY v.insertTimeStamp DESC", out bool boolDbError, out string strDbError, sqlParameters);

            if (dataMyVotes.Tables.Count > 0 && dataMyVotes.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in dataMyVotes.Tables[0].Rows)
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

            // get posts
            sqlParameters = new List<SqlParameter>();
            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@anonId";
            sqlParameter.Value = Session["anonId"].ToString();
            sqlParameters.Add(sqlParameter);

            DataSet dataSetMyPosts = Db.ExecuteQuery("SELECT TOP 20 postId, anonId, insertTimeStamp, msg FROM post WHERE anonId = @anonId ORDER BY insertTimeStamp DESC", out boolDbError, out strDbError, sqlParameters);

            if (dataSetMyPosts.Tables.Count > 0 && dataSetMyPosts.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in dataSetMyPosts.Tables[0].Rows)
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

                    dataRow["friendActivity"] = "Posted";
                    dataRow["activityTimeStamp"] = row["insertTimeStamp"].ToString();

                    dataTable.Rows.InsertAt(dataRow, 0);
                }
            }


            rptrMyActivity.DataSource = dataTable;
            rptrMyActivity.DataBind();

        }

    }
}