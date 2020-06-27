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
    public partial class Friends : System.Web.UI.Page
    {
        //TODO if user deletes their account, need to delete record from friend table
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Security.IsLoggedInOrKick("Friends");

                rptrFriendshipRequests_load();
                rptrFriends_load();
                rptrRecentFriendActivty_load();

                //TODO: if user's friend already accepted friend request, but this user has friend's request pending, should delete it
            }
        }

        protected void btnFriendAcceptOnClick(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            RepeaterItem item = (RepeaterItem)btn.NamingContainer;
            HiddenField hfFriendRequestId = item.FindControl("hfFriendRequestId") as HiddenField;
            HiddenField hfEncryptedRequesterEmail = item.FindControl("hfEncryptedRequesterEmail") as HiddenField;
            HiddenField hfFriendAnonId = item.FindControl("hfFriendAnonId") as HiddenField;
            HiddenField hfEncryptedAnonIdRequestingFriendship = item.FindControl("hfEncryptedAnonIdRequestingFriendship") as HiddenField;
            Label lblFriendAnonId = item.FindControl("lblFriendAnonId") as Label;

            friendResponse(hfFriendRequestId.Value, true, lblFriendAnonId.Text, hfEncryptedRequesterEmail.Value);

        }

        protected void btnFriendRejectOnClick(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            RepeaterItem item = (RepeaterItem)btn.NamingContainer;
            HiddenField hfFriendRequestId = item.FindControl("hfFriendRequestId") as HiddenField;
            HiddenField hfKnewEmailAddress = item.FindControl("hfKnewEmailAddress") as HiddenField;

            friendResponse(hfFriendRequestId.Value, false);

        }
        /// <summary>
        /// Send friendship request
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnSubmitOnClick(object sender, EventArgs e)
        {
            string anonId = tbxAnonId.Text.Replace("Anon-","").Replace("Anon","");

            if (int.TryParse(anonId, out int intAnonId))
            {
                Common.FriendRequestAdd(anonId, tbxEmail.Text, tbxMsg.Text);
                lblFriendRequestSent.Visible = true;
                lblFriendAddError.Visible = false;
            }
            else
            { 
                lblFriendAddError.Visible = true;
                lblFriendRequestSent.Visible = false;
            }


        }

        private void rptrFriendshipRequests_load()
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@anonId";
            sqlParameter.Value = Session["anonId"].ToString();
            sqlParameters.Add(sqlParameter);

            DataSet dataSet = Db.ExecuteQuery("SELECT * FROM friendRequest WHERE anonId = @anonId AND Accepted IS NULL", out bool boolDbError, out string strDbError, sqlParameters);
            if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                rptrFriendshipRequests.DataSource = dataSet;
                rptrFriendshipRequests.DataBind();
                lblNoPendingFriendshipRequests.Visible = false;
            }
            else 
            {
                lblNoPendingFriendshipRequests.Visible = true;
            }
        }

        protected void rptrFriendshipRequestsOnItemDataBound(Object Sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {

                string encryptedAnonIdRequestingFriendship = DataBinder.Eval(e.Item.DataItem, "encryptedAnonIdRequestingFriendship").ToString();
                string encryptedMsg = DataBinder.Eval(e.Item.DataItem, "encryptedMsg").ToString();
                string EncryptionKeyEncryptedByPublicKey = DataBinder.Eval(e.Item.DataItem, "EncryptionKeyEncryptedByPublicKey").ToString();
                string friendRequestId = DataBinder.Eval(e.Item.DataItem, "friendRequestId").ToString();
                string friendEmail = DataBinder.Eval(e.Item.DataItem, "friendEmail").ToString();
                string encryptedRequesterEmail = DataBinder.Eval(e.Item.DataItem, "encryptedRequesterEmail").ToString();
                //List<SqlParameter> sqlParameters = new List<SqlParameter>();
                //SqlParameter sqlParameter = new SqlParameter();
                //sqlParameter.ParameterName = "@friendRequestId";
                //sqlParameter.Value = friendRequestId;
                //sqlParameters.Add(sqlParameter);
                //DataSet dataSet=Db.ExecuteQuery("SELECT * FROM friendRequest WHERE friendRequestId = @friendRequestId", out bool boolDbError, out string strDbError, sqlParameters);
                //byte[] EncryptionKeyEncryptedByPublicKey = dataSet.Tables[0].Rows[0]["EncryptionKeyEncryptedByPublicKey"] as byte[];

                Label lblFriendAnonId = e.Item.FindControl("lblFriendAnonId") as Label;
                Button btnFriendAccept = e.Item.FindControl("btnFriendAccept") as Button;
                Button btnFriendReject = e.Item.FindControl("btnFriendReject") as Button;
                Label lblMsg = e.Item.FindControl("lblMsg") as Label;
                HiddenField hfFriendRequestId = e.Item.FindControl("hfFriendRequestId") as HiddenField;
                HiddenField hfFriendEmail = e.Item.FindControl("hfFriendEmail") as HiddenField;
                HiddenField hfEncryptedRequesterEmail = e.Item.FindControl("hfEncryptedRequesterEmail") as HiddenField;
                HiddenField hfFriendAnonId = e.Item.FindControl("hfFriendAnonId") as HiddenField;

                string EncryptionKey = Security.Rsa.Decrypt(EncryptionKeyEncryptedByPublicKey, Session["PrivateKey"].ToString());

                hfFriendRequestId.Value = friendRequestId;
                lblFriendAnonId.Text = Security.DecryptStringAES(encryptedAnonIdRequestingFriendship, EncryptionKey);
                if (encryptedMsg != string.Empty)
                {
                    lblMsg.Text = Security.DecryptStringAES(encryptedMsg, EncryptionKey);
                }
                hfFriendEmail.Value = friendEmail;
                if (encryptedRequesterEmail != string.Empty)
                {
                    hfEncryptedRequesterEmail.Value = Security.DecryptStringAES(encryptedRequesterEmail, EncryptionKey);
                }
                hfFriendAnonId.Value = DataBinder.Eval(e.Item.DataItem, "anonId").ToString();

                //decrypt data with private key

            }
        }

        private void rptrFriends_load()
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            SqlParameter sqlParameter = new SqlParameter();

            sqlParameter.ParameterName = "@anonId";
            sqlParameter.Value = Session["anonId"];
            sqlParameters.Add(sqlParameter);

            rptrFriends.DataSource = Db.ExecuteQuery("SELECT * FROM friend WHERE anonId = @anonId", out bool boolDbError, out string strDbError, sqlParameters);
            rptrFriends.DataBind();
        }

        private void friendResponse(string FriendRequestId, bool accepted = true, string FriendAnonId = "", string encryptedRequesterEmail = "")
        {
            Common.friendResponse(FriendRequestId, accepted, FriendAnonId, encryptedRequesterEmail);

            rptrFriends_load();
            rptrFriendshipRequests_load();
        }

        private void RejectRequestsFromSameAnon(string FriendAnonId)
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@anonId";
            sqlParameter.Value = Session["anonId"];
            sqlParameters.Add(sqlParameter);
            DataSet dataSet = Db.ExecuteQuery("SELECT * FROM friendRequest WHERE anonId = @anonId AND Accepted IS NULL", out bool boolDbError, out string strDbError, sqlParameters);

            if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow r in dataSet.Tables[0].Rows)
                {
                    string EncryptionKey = Security.Rsa.Decrypt(r["EncryptionKeyEncryptedByPublicKey"].ToString(), Session["PrivateKey"].ToString());
                    string encryptedAnonIdRequestingFriendship = Security.DecryptStringAES(r["encryptedAnonIdRequestingFriendship"].ToString(), EncryptionKey);
                    if (encryptedAnonIdRequestingFriendship == FriendAnonId)
                    {
                        sqlParameters = new List<SqlParameter>();
                        sqlParameter = new SqlParameter();
                        sqlParameter.ParameterName = "@friendRequestId";
                        sqlParameter.Value = r["friendRequestId"].ToString();
                        sqlParameters.Add(sqlParameter);

                        //TODO: delete instead?
                        Db.ExecuteNonQuery("UPDATE friendRequest SET Accepted = 0 WHERE friendRequestId=@friendRequestId", out boolDbError, out strDbError, sqlParameters);
                    }


                }
            }
        }

        protected void rptrFriendsOnItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                string encryptedNickName = DataBinder.Eval(e.Item.DataItem, "nickName").ToString();
                string encryptedEmail = DataBinder.Eval(e.Item.DataItem, "email").ToString();
                string encryptedAnonIdRequestingFriendship = DataBinder.Eval(e.Item.DataItem, "encryptedFriendAnonId").ToString();
                string friendId = DataBinder.Eval(e.Item.DataItem, "friendId").ToString();

                Label lblAnonId = e.Item.FindControl("lblAnonId") as Label;
                Label lblNickName = e.Item.FindControl("lblNickName") as Label;
                Label lblEmail = e.Item.FindControl("lblEmail") as Label;
                HiddenField hfFriendId = e.Item.FindControl("hfFriendId") as HiddenField;
                Label lblPublicNickName = e.Item.FindControl("lblPublicNickName") as Label;
                Label lblPublicCity = e.Item.FindControl("lblPublicCity") as Label;
                Label lblPublicNation = e.Item.FindControl("lblPublicNation") as Label;

                if (encryptedAnonIdRequestingFriendship != null && encryptedAnonIdRequestingFriendship != string.Empty)
                {
                    lblAnonId.Text = Security.DecryptStringAES(encryptedAnonIdRequestingFriendship, Session["hashedPassword"].ToString());
                }
                if (encryptedNickName != null && encryptedNickName != string.Empty)
                {
                    lblNickName.Text = Security.DecryptStringAES(encryptedNickName, Session["hashedPassword"].ToString());
                }
                if (encryptedEmail != null && encryptedEmail != string.Empty)
                {
                    lblEmail.Text = Security.DecryptStringAES(encryptedEmail, Session["hashedPassword"].ToString());
                }

                hfFriendId.Value = friendId;


                DataSet dataSet = Db.Common.AnonByAnonId(out bool boolDbError, out string strDbError, lblAnonId.Text);

                if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
                {
                    if (dataSet.Tables[0].Rows[0]["PublicName"] !=DBNull.Value)
                    {
                        lblPublicNickName.Text = dataSet.Tables[0].Rows[0]["PublicName"].ToString();
                    }
                    if (dataSet.Tables[0].Rows[0]["PublicCity"] != DBNull.Value)
                    {
                        lblPublicCity.Text = dataSet.Tables[0].Rows[0]["PublicCity"].ToString();
                    }
                    if (dataSet.Tables[0].Rows[0]["PublicNation"] != DBNull.Value)
                    {
                        lblPublicNation.Text = dataSet.Tables[0].Rows[0]["PublicNation"].ToString();
                    }
                }


            }

        }

        protected void btnSearchOnClick(object sender, EventArgs e)
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@PublicName";
            sqlParameter.Value = tbxName.Text;
            sqlParameters.Add(sqlParameter);

            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@PublicCity";
            sqlParameter.Value = tbxCity.Text;
            sqlParameters.Add(sqlParameter);

            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@PublicNation";
            sqlParameter.Value = tbxNation.Text;
            sqlParameters.Add(sqlParameter);

            DataSet dataSet = Db.ExecuteQuery("SELECT * FROM anon WHERE PublicName LIKE '%'+@PublicName+'%' OR PublicCity LIKE '%'+@PublicCity+'%' OR PublicNation LIKE '%'+@PublicNation+'%'", out bool boolDbError, out string strDbError, sqlParameters);
            if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                rptrSearch.DataSource = dataSet;
                rptrSearch.DataBind();

                lblSearchResults.Visible = false;
            }
            else
            {
                lblSearchResults.Visible = true;
                lblSearchResults.Text = "No results";
            }
            
        }

        protected void btnSearchFriendAddOnClick(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            RepeaterItem item = (RepeaterItem)btn.NamingContainer;
            //HiddenField hfFriendRequestId = item.FindControl("hfFriendRequestId") as HiddenField;
            //HiddenField hfEncryptedRequesterEmail = item.FindControl("hfEncryptedRequesterEmail") as HiddenField;
            HiddenField hfFriendAnonId = item.FindControl("hfFriendAnonId") as HiddenField;
            //HiddenField hfEncryptedAnonIdRequestingFriendship = item.FindControl("hfEncryptedAnonIdRequestingFriendship") as HiddenField;
            //Label lblFriendAnonId = item.FindControl("lblFriendAnonId") as Label;

            //friendResponse(hfFriendRequestId.Value, true, lblFriendAnonId.Text, hfEncryptedRequesterEmail.Value);

            tbxAnonId.Text = hfFriendAnonId.Value;
            tbxEmail.Focus();

        }

        protected void btnFriendMessageOnClick(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            RepeaterItem item = (RepeaterItem)btn.NamingContainer;
            Label lblAnonId = item.FindControl("lblAnonId") as Label;

            Session["NewMsgAnonId"] = lblAnonId.Text;

            Response.Redirect("Messages?Action=NewMsg");
            Response.End();
        }

        protected void btnFriendEdit_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            RepeaterItem item = (RepeaterItem)btn.NamingContainer;
            Label lblAnonId = item.FindControl("lblAnonId") as Label;
            HiddenField hfFriendId = item.FindControl("hfFriendId") as HiddenField;

            Session["FriendId"] = hfFriendId.Value;
            Session["FriendAnonId"] = lblAnonId.Text;

            Response.Redirect("Friend");
            Response.End();

        }

        private void rptrRecentFriendActivty_load()
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

            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@anonId";
            sqlParameter.Value = Session["anonId"].ToString();
            sqlParameters.Add(sqlParameter);

            DataSet dataSetFriends = Db.ExecuteQuery("SELECT encryptedFriendAnonId FROM friend WHERE anonId=@anonId", out bool boolDbError, out string strDbError, sqlParameters);

            if (dataSetFriends.Tables.Count > 0 && dataSetFriends.Tables[0].Rows.Count > 0)
            {

                string strIds = string.Empty;
                foreach (DataRow row in dataSetFriends.Tables[0].Rows)
                {
                    string FriendAnonId = Security.DecryptStringAES(row["encryptedFriendAnonId"].ToString(), Session["hashedPassword"].ToString());
                    if (strIds != string.Empty)
                    {
                        strIds += ",";
                    }
                    strIds += FriendAnonId;
                }

                string sqlIn = " IN (" + strIds + ")";

                //get friends recent posts
                sqlParameters = new List<SqlParameter>();
                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@lastLoginTimeStamp";
                sqlParameter.Value = Session["LastLoginDateTime"].ToString();
                sqlParameters.Add(sqlParameter);

                DataSet dataSetFriendPosts = Db.ExecuteQuery("SELECT TOP 10 postId, anonId, insertTimeStamp, msg FROM post WHERE insertTimeStamp> @lastLoginTimeStamp AND anonId " + sqlIn + " ORDER BY insertTimeStamp DESC", out boolDbError,out strDbError, sqlParameters);

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
                            dataRow["post150character"] = row["msg"].ToString().Substring(0,120) + " ...";
                        }
                        
                        dataRow["friendActivity"] = "Posted by friend";
                        dataRow["activityTimeStamp"] = row["insertTimeStamp"].ToString();

                        dataTable.Rows.InsertAt(dataRow, 0);
                    }
                }


                //get friends recent votes

                sqlParameters = new List<SqlParameter>();
                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@lastLoginTimeStamp";
                sqlParameter.Value = Session["LastLoginDateTime"].ToString();
                sqlParameters.Add(sqlParameter);

                DataSet dataSetFriendVotes = Db.ExecuteQuery("SELECT TOP 10 v.postId, v.anonId, msg, v.insertTimeStamp, upVote FROM vote v JOIN post p ON v.postId = p.postId WHERE v.insertTimeStamp> @lastLoginTimeStamp AND v.anonId " + sqlIn + " ORDER BY v.insertTimeStamp DESC", out boolDbError, out strDbError, sqlParameters);

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
                        else { 
                            friendActivity += " up"; 
                        }
                        dataRow["friendActivity"] = friendActivity;
                        dataRow["activityTimeStamp"] = row["insertTimeStamp"].ToString();

                        dataTable.Rows.InsertAt(dataRow, 0);
                    }
                }

                //TODO add activity on previous posts
                sqlParameters = new List<SqlParameter>();
                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@lastLoginTimeStamp";
                sqlParameter.Value = Session["LastLoginDateTime"].ToString();
                sqlParameters.Add(sqlParameter);

                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@anonId";
                sqlParameter.Value = Session["anonId"].ToString();
                sqlParameters.Add(sqlParameter);

                DataSet dataSetPostActivity = Db.ExecuteQuery("SELECT TOP 10 postId, anonId, insertTimeStamp, msg FROM post WHERE lastActivityTimeStamp > @lastLoginTimeStamp AND anonId = @anonId ORDER BY insertTimeStamp DESC", out boolDbError, out strDbError, sqlParameters);
                if (dataSetPostActivity.Tables.Count > 0 && dataSetPostActivity.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in dataSetPostActivity.Tables[0].Rows)
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

                        dataRow["friendActivity"] = "New Reply/Replies Added";
                        dataRow["activityTimeStamp"] = row["insertTimeStamp"].ToString();

                        dataTable.Rows.InsertAt(dataRow, 0);

                        // get any post/reply to parent post

                        sqlParameters = new List<SqlParameter>();
                        sqlParameter = new SqlParameter();
                        sqlParameter.ParameterName = "@lastLoginTimeStamp";
                        sqlParameter.Value = Session["LastLoginDateTime"].ToString();
                        sqlParameters.Add(sqlParameter);

                        sqlParameter = new SqlParameter();
                        sqlParameter.ParameterName = "@replyToPostId";
                        sqlParameter.Value = row["postId"].ToString();
                        sqlParameters.Add(sqlParameter);

                        DataSet dataSetSubPostActivity = Db.ExecuteQuery("SELECT TOP 5 postId, anonId, insertTimeStamp, msg FROM post WHERE lastActivityTimeStamp > @lastLoginTimeStamp AND replyToPostId=@replyToPostId ORDER BY insertTimeStamp DESC", out boolDbError, out strDbError, sqlParameters);
                        if (dataSetSubPostActivity.Tables.Count > 0 && dataSetSubPostActivity.Tables[0].Rows.Count > 0)
                        {
                            foreach (DataRow _row in dataSetSubPostActivity.Tables[0].Rows)
                            {
                                DataRow _dataRow = dataTable.NewRow();

                                _dataRow["anonId"] = _row["anonId"].ToString();
                                _dataRow["postId"] = _row["postId"].ToString();
                                if (_row["msg"].ToString().Length < 120)
                                {
                                    _dataRow["post150character"] = _row["msg"].ToString();
                                }
                                else
                                {
                                    _dataRow["post150character"] = _row["msg"].ToString().Substring(0, 120) + " ...";
                                }

                                _dataRow["friendActivity"] = "New Reply Added To Your Post";
                                _dataRow["activityTimeStamp"] = _row["insertTimeStamp"].ToString();

                                dataTable.Rows.InsertAt(_dataRow, 0);

                            }
                        }


                    }
                }

                rptrRecentFriendActivty.DataSource = dataTable;
                rptrRecentFriendActivty.DataBind();
                lblNoResults.Visible = false;
            }
            else
            {
                lblNoResults.Visible = true;
            }

        }

    }
}