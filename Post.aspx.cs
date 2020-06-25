using Anonymous.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace Anonymous
{
    public partial class Post : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString["postId"] != null)
                {
                    Session["postId"] = Request.QueryString["postId"].ToString();
                }
                if (Session["postId"] == null)
                {
                    Response.Redirect("Nation");
                }
                if (!Security.IsLoggedIn())
                {
                    tbxMesssage.Enabled = false;
                    tbxMesssage.Text = "[Must be logged in to post]";

                    if (Request.QueryString["action"] != null && Request.QueryString["postId"] != null)
                    {
                        //action=reply&postId=
                        Response.Redirect("Login?goAfter=Post&action=reply&postId=" + Request.QueryString["postId"].ToString());
                        Response.End();
                    }

                }
                else
                {
                    if (!Security.IsEmailVerified)
                    {
                        tbxMesssage.Enabled = false;
                        tbxMesssage.Text = "[Must have email verified to post]";
                    }
                }
                postData();
                bindDynamicRepeater(rptrMessages);

                if (Request.QueryString["action"] != null && Request.QueryString["action"].ToString() == "reply")
                {
                    tbxMesssage.Focus();
                }

                if (Security.IsLoggedIn())
                {
                    IncrementViewCount();
                }

                if (Security.IsLoggedIn() && Request.QueryString["Vote"] != null && Request.QueryString["VotePostId"] != null)
                {
                    bool isVoteUp = true;
                    if (Request.QueryString["Vote"].ToString() == "down")
                    {
                        isVoteUp = false;
                    }
                    vote(Request.QueryString["VotePostId"].ToString(), isVoteUp);
                }
                else if (Request.QueryString["Vote"] != null && Request.QueryString["VotePostId"] != null)
                {
                    Response.Redirect("Login");
                    Response.End();
                }

            }
        }

        protected void btnMessageAddOnClick(object sender, EventArgs e)
        {
            if (Security.IsLoggedIn())
            {
                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                SqlParameter sqlParameter = new SqlParameter();

                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@nationId";
                sqlParameter.Value = Session["nationId"].ToString();
                sqlParameters.Add(sqlParameter);

                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@anonId";
                sqlParameter.Value = Session["anonId"].ToString();
                sqlParameters.Add(sqlParameter);

                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@msg";
                sqlParameter.Value = tbxMesssage.Text;
                sqlParameters.Add(sqlParameter);

                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@replyToPostId";
                sqlParameter.Value = Session["postId"].ToString();
                sqlParameters.Add(sqlParameter);


                Db.ExecuteNonQuery("INSERT INTO post(nationId,anonId,msg,replyToPostId) VALUES(@nationId,@anonId,@msg,@replyToPostId)", out bool boolDbError, out string strDbError, sqlParameters);

                sqlParameters = new List<SqlParameter>();

                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@postId";
                sqlParameter.Value = Session["postId"].ToString();
                sqlParameters.Add(sqlParameter);

                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@lastActivityTimeStamp";
                sqlParameter.Value = DateTime.Now.ToString();
                sqlParameters.Add(sqlParameter);

                Db.ExecuteNonQuery("UPDATE post SET lastActivityTimeStamp=@lastActivityTimeStamp WHERE postId=@postId", out boolDbError, out strDbError, sqlParameters);

                bindDynamicRepeater(rptrMessages);

                tbxMesssage.Text = string.Empty;
            }
            else
            {
                Response.Redirect("Login");
                Response.End();
            }
        }

        private void postData()
        {

            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@postId";
            sqlParameter.Value = Session["postId"].ToString();
            sqlParameters.Add(sqlParameter);
            DataSet ds = Db.ExecuteQuery("SELECT * FROM post WHERE postId = @postId ORDER BY insertTimeStamp DESC", out bool boolDbError, out string strDbError, sqlParameters);

            //Session["postId"]= ds.Tables[0].Rows[0]["postId"].ToString();
            //lblTitle.Text = ds.Tables[0].Rows[0]["Title"].ToString();
            lblMsg.Text = ds.Tables[0].Rows[0]["msg"].ToString();
            Session["nationId"] = ds.Tables[0].Rows[0]["nationId"].ToString();
            if (ds.Tables[0].Rows[0]["replyToPostId"] != null && ds.Tables[0].Rows[0]["replyToPostId"].ToString() != string.Empty)
            {
                hlParent.NavigateUrl = "Post?postId=" + ds.Tables[0].Rows[0]["replyToPostId"].ToString();
            }
            else
            {
                hlParent.Visible = false;
            }
        }

        private void bindDynamicRepeater(Repeater repeater, string replyToPostId = "")
        {

            string strSQL = string.Empty;

            strSQL = "SELECT * FROM post WHERE postId = @postId ORDER BY insertTimeStamp DESC";

            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@postId";
            sqlParameter.Value = Session["postId"].ToString();
            sqlParameters.Add(sqlParameter);

            if (replyToPostId != string.Empty)
            {
                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@replyToPostId";
                sqlParameter.Value = replyToPostId;
                sqlParameters.Add(sqlParameter);

                strSQL = "SELECT * FROM post WHERE postId = @postId AND replyToPostId = @replyToPostId ORDER BY insertTimeStamp DESC";

            }

            DataSet ds = Db.ExecuteQuery(strSQL, out bool boolDbError, out string strDbError, sqlParameters);

            repeater.ItemDataBound += ItemDataBoundRepeater;
            repeater.DataSource = ds;
            repeater.DataBind();

        }

        private void ItemDataBoundRepeater(Object Sender, RepeaterItemEventArgs e)
        {

            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                SqlParameter sqlParameter = new SqlParameter();

                #region displayed data in repeater

                Table table = new Table();

                TableRow tr = new TableRow();
                TableCell tc = new TableCell();

                string postId = DataBinder.Eval(e.Item.DataItem, "postId").ToString();

                #region should be indented and how much

                string replyToPostId = DataBinder.Eval(e.Item.DataItem, "replyToPostId").ToString();
                int counterHowManyMessagesDeep = 0;

                string _replyToPostId = replyToPostId;
                while (_replyToPostId != string.Empty)
                {
                    sqlParameters = new List<SqlParameter>();
                    sqlParameter = new SqlParameter();
                    sqlParameter.ParameterName = "@postId";
                    sqlParameter.Value = Session["postId"].ToString();
                    sqlParameters.Add(sqlParameter);

                    sqlParameter = new SqlParameter();
                    sqlParameter.ParameterName = "@replyToPostId";
                    sqlParameter.Value = _replyToPostId;
                    sqlParameters.Add(sqlParameter);

                    DataSet _ds = Db.ExecuteQuery("SELECT * FROM post WHERE postId = @replyToPostId ORDER BY insertTimeStamp DESC", out bool _boolDbError, out string _strDbError, sqlParameters);
                    if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                    {
                        _replyToPostId = _ds.Tables[0].Rows[0]["replyToPostId"].ToString();
                        counterHowManyMessagesDeep++;
                    }
                    else
                    {
                        _replyToPostId = string.Empty;
                    }
                    
                }

                #endregion

                Literal ltlDivIndent1 = new Literal();
                Literal ltlDivIndent2 = new Literal();

                int spacerIndent = counterHowManyMessagesDeep * 15;

                ltlDivIndent1.Text = "<div style='width:" + spacerIndent.ToString() + "px;height:1px;float:left;'>&nbsp;</div>";
                ltlDivIndent2.Text = ltlDivIndent1.Text;

                //tr = new TableRow();
                //tc = new TableCell();
                //tc.Controls.Add(ltlDivIndent1);
                //tc.CssClass = "Message Message-UserTimeStamp";
                HyperLink hyperLink = new HyperLink();
                hyperLink.Text = "Anon" + DataBinder.Eval(e.Item.DataItem, "anonId").ToString();
                hyperLink.NavigateUrl="Anon?anonId=" + DataBinder.Eval(e.Item.DataItem, "anonId").ToString();
                Label lblAnon = new Label();
                lblAnon.Text = "<a href='Anon?anonId=" + DataBinder.Eval(e.Item.DataItem, "anonId").ToString() + "'>Anon-" + DataBinder.Eval(e.Item.DataItem, "anonId").ToString() + "</a>";
                Label lblTimeStamp = new Label();
                lblTimeStamp.CssClass = "PostTimeStamp";
                lblTimeStamp.Text = DataBinder.Eval(e.Item.DataItem, "insertTimeStamp").ToString();
                //tc.Controls.Add(lblAnonTimeStamp);
                //tr.Cells.Add(tc);
                //table.Rows.Add(tr);


                //tr = new TableRow();
                //tc = new TableCell();
                //tc.Controls.Add(ltlDivIndent2);
                //tc.CssClass = "Message Message-Text";
                Label lblMessage = new Label();
                //lblMessage.CssClass = "PostText";
                lblMessage.Text = DataBinder.Eval(e.Item.DataItem, "msg").ToString();
                Literal ltlMsg = new Literal();
                ltlMsg.Text= DataBinder.Eval(e.Item.DataItem, "msg").ToString().Replace(Environment.NewLine, "<br />");
                Label lblReplyToThis = new Label();
                lblReplyToThis.Text= "&nbsp;&nbsp;<a class='postReplyLink' href='Post?action=reply&postId=" + postId + "'>[Reply to this]</a>&nbsp;&nbsp;";
                //System.Web.UI.HtmlControls.HtmlGenericControl divWithPostText = new System.Web.UI.HtmlControls.HtmlGenericControl();
                //divWithPostText.Attributes.Add("class", "DivPostText");
                //System.Web.UI.HtmlControls.HtmlGenericControl divWithPostTextInner = new System.Web.UI.HtmlControls.HtmlGenericControl();
                //divWithPostTextInner.Attributes.Add("class", "DivPostTextInner");
                //divWithPostTextInner.Controls.Add(ltlMsg);
                //divWithPostText.Controls.Add(divWithPostTextInner);
                HtmlTable tableWithMsg = new HtmlTable();
                HtmlTableRow tableWithMsgRow = new HtmlTableRow();
                HtmlTableCell tableWithMsgCell = new HtmlTableCell();
                tableWithMsgCell.Attributes.Add("class", "PostMsgTableTd");
                tableWithMsgCell.Controls.Add(ltlMsg);
                tableWithMsgRow.Attributes.Add("class", "PostMsgTableTr");
                tableWithMsgRow.Controls.Add(tableWithMsgCell);
                tableWithMsg.Controls.Add(tableWithMsgRow);
                tableWithMsg.Attributes.Add("class", "PostMsgTable");

                Label lblReplyToThisIcon = new Label();
                lblReplyToThisIcon.Text = "<a class='postReplyLink' href='Post?action=reply&postId=" + postId + "' title='Reply to this'><img src='images/reply-icon.png' class='voteImage'></a>";


                //htmlTableRow.Controls.Add(htmlTableCell);
                //htmlTable.Controls.Add(htmlTableRow);
                //tc.Controls.Add(htmlTable);

                //check if already voted
                sqlParameters = new List<SqlParameter>();
                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@postId";
                sqlParameter.Value = postId;
                sqlParameters.Add(sqlParameter);

                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@anonId";
                sqlParameter.Value = Session["anonId"];
                sqlParameters.Add(sqlParameter);


                DataSet dataSet = Db.ExecuteQuery("SELECT upVote FROM vote WHERE postId=@postId AND anonId=@anonId", out bool boolDbError, out string strDbError, sqlParameters);

                bool alreadyVoted = false, upVote = true;
                if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
                {
                    alreadyVoted = true;
                    if (dataSet.Tables[0].Rows[0]["upVote"].ToString().ToLower() == "false" || dataSet.Tables[0].Rows[0]["upVote"].ToString().ToLower() == "-1")
                    {
                        upVote = false;
                    }
                }

                sqlParameters = new List<SqlParameter>();
                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@postId";
                sqlParameter.Value = postId;
                sqlParameters.Add(sqlParameter);

                DataSet dataSetCount = Db.ExecuteQuery("SELECT SUM(upVote) FROM vote WHERE postId=@postId", out boolDbError, out strDbError, sqlParameters);
                Literal ltlVoteCount = new Literal();

                string voteCount = "+0";
                if (dataSetCount.Tables.Count > 0 && dataSetCount.Tables[0].Rows.Count > 0)
                {

                    if (dataSetCount.Tables[0].Rows[0][0].ToString().IndexOf("-") >= 0)
                    {
                        voteCount = "<span style='color:red;'>" + dataSetCount.Tables[0].Rows[0][0].ToString() + "</span>";
                    }
                    else
                    {
                        voteCount = "<span style='color:green;'>+" + dataSetCount.Tables[0].Rows[0][0].ToString() + "</span>";
                        if (dataSetCount.Tables[0].Rows[0][0] == DBNull.Value || dataSetCount.Tables[0].Rows[0][0].ToString() == "0")
                        {
                            voteCount = "<span style='color:gray;'>+0</span>";
                        }
                    }
                }

                Literal ltlVoteUp = new Literal();
                Literal ltlVoteDown = new Literal();
                ltlVoteCount.Text = voteCount;

                if (alreadyVoted)
                {
                    if (upVote)
                    {
                        ltlVoteUp.Text = "<a href='Post?Vote=up&VotePostId=" + postId + "' title='Click here to un-upvote'><img src='images/vote-up.png' class='voteImage' /></a>";
                        ltlVoteDown.Text = "<a href='Post?Vote=down&VotePostId=" + postId + "' title='Click here to change to downvote'><img src='images/vote-down-gray.png' class='voteImage'  /></a>";
                    }
                    else
                    {
                        ltlVoteUp.Text = "<a href='Post?Vote=up&VotePostId=" + postId + "' title='Click here to change to upvote'><img src='images/vote-up-gray.png' class='voteImage' class='voteImage' /></a>";
                        ltlVoteDown.Text = "<a href='Post?Vote=down&VotePostId=" + postId + "' title='Click here to un-downvote'><img src='images/vote-down.png' class='voteImage' /></a>";
                    }
                }
                else
                {
                    ltlVoteUp.Text = "<a href='Post?Vote=up&VotePostId=" + postId + "' title='Login to upvote'><img src='images/vote-up-gray.png' class='voteImage' /></a>";
                    ltlVoteDown.Text = "<a href='Post?Vote=down&VotePostId=" + postId + "' title='Login to downvote'><img src='images/vote-down-gray.png' class='voteImage' /></a>";
                }

                string strSpacer = "&nbsp;&nbsp;&nbsp;";
                Label lblSpacer = new Label();
                Label lblSpacer2 = new Label();
                lblSpacer.Text = strSpacer;
                lblSpacer2.Text = strSpacer;

                //HtmlTable htmlTable = new HtmlTable();
                //HtmlTableRow htmlTableRow = new HtmlTableRow();
                //htmlTableRow.Attributes.Add("class", "PostHeader");
                //HtmlTableCell htmlTableCell = new HtmlTableCell();
                //htmlTableCell.Controls.Add(ltlDivIndent1);
                //htmlTableRow.Controls.Add(htmlTableCell);
                //htmlTableCell = new HtmlTableCell();
                //htmlTableCell.Controls.Add(lblAnon);
                //htmlTableCell.Controls.Add(lblSpacer);

                //htmlTableCell.Controls.Add(ltlVoteUp);
                //htmlTableCell.Controls.Add(ltlVoteCount);
                //htmlTableCell.Controls.Add(ltlVoteDown);
                //htmlTableCell.Controls.Add(lblSpacer2);

                //htmlTableCell.Controls.Add(lblTimeStamp);
                //htmlTableRow.Controls.Add(htmlTableCell);
                //htmlTable.Controls.Add(htmlTableRow);
                //tc.Controls.Add(htmlTable);



                //htmlTable = new HtmlTable();
                //htmlTableRow = new HtmlTableRow();
                //htmlTableCell = new HtmlTableCell();
                //htmlTableCell.Controls.Add(ltlDivIndent2);
                //htmlTableRow.Controls.Add(htmlTableCell);
                //htmlTableCell = new HtmlTableCell();
                //htmlTableCell.Controls.Add(div);
                ////tc.Controls.Add(ltlVoteUp);
                ////tc.Controls.Add(ltlVoteCount);
                ////tc.Controls.Add(ltlVoteDown);
                //htmlTableRow.Controls.Add(htmlTableCell);
                //htmlTable.Controls.Add(htmlTableRow);

                HtmlTable htmlTableFrame = new HtmlTable();
                HtmlTable htmlTableVoteButtonsAndCount = new HtmlTable();
                HtmlTable htmlTableAnonTimeStampMsg = new HtmlTable();

                HtmlTableRow htmlTableVoteButtonsAndCountRow = new HtmlTableRow();
                HtmlTableCell htmlTableVoteButtonsAndCountCell = new HtmlTableCell();
                htmlTableVoteButtonsAndCountCell.Controls.Add(ltlVoteUp);
                htmlTableVoteButtonsAndCountRow.Controls.Add(htmlTableVoteButtonsAndCountCell);
                htmlTableVoteButtonsAndCount.Controls.Add(htmlTableVoteButtonsAndCountRow);
                htmlTableVoteButtonsAndCountRow = new HtmlTableRow();
                htmlTableVoteButtonsAndCountCell = new HtmlTableCell();
                htmlTableVoteButtonsAndCountCell.Controls.Add(ltlVoteCount);
                htmlTableVoteButtonsAndCountRow.Controls.Add(htmlTableVoteButtonsAndCountCell);
                htmlTableVoteButtonsAndCount.Controls.Add(htmlTableVoteButtonsAndCountRow);
                htmlTableVoteButtonsAndCountCell = new HtmlTableCell();
                htmlTableVoteButtonsAndCountCell.Controls.Add(lblReplyToThisIcon);
                htmlTableVoteButtonsAndCountRow.Controls.Add(htmlTableVoteButtonsAndCountCell);
                htmlTableVoteButtonsAndCountRow = new HtmlTableRow();
                htmlTableVoteButtonsAndCountCell = new HtmlTableCell();
                htmlTableVoteButtonsAndCountCell.Controls.Add(ltlVoteDown);
                htmlTableVoteButtonsAndCountRow.Controls.Add(htmlTableVoteButtonsAndCountCell);
                htmlTableVoteButtonsAndCount.Controls.Add(htmlTableVoteButtonsAndCountRow);

                HtmlTableRow htmlTableAnonTimeStampMsgRow = new HtmlTableRow();
                HtmlTableCell htmlTableAnonTimeStampMsgCell = new HtmlTableCell();
                htmlTableAnonTimeStampMsgCell.Controls.Add(lblAnon);
                htmlTableAnonTimeStampMsgCell.Controls.Add(lblReplyToThis);
                htmlTableAnonTimeStampMsgCell.Controls.Add(lblTimeStamp);
                htmlTableAnonTimeStampMsgRow.Controls.Add(htmlTableAnonTimeStampMsgCell);
                htmlTableAnonTimeStampMsg.Controls.Add(htmlTableAnonTimeStampMsgRow);
                htmlTableAnonTimeStampMsgRow = new HtmlTableRow();
                htmlTableAnonTimeStampMsgCell = new HtmlTableCell();
                htmlTableAnonTimeStampMsgCell.Controls.Add(tableWithMsg);
                htmlTableAnonTimeStampMsgRow.Controls.Add(htmlTableAnonTimeStampMsgCell);
                htmlTableAnonTimeStampMsg.Controls.Add(htmlTableAnonTimeStampMsgRow);

                HtmlTableRow HtmlTableFrameRow = new HtmlTableRow();
                HtmlTableCell HtmlTableFrameCell = new HtmlTableCell();
                HtmlTableFrameCell.Controls.Add(htmlTableVoteButtonsAndCount);
                HtmlTableFrameCell.VAlign = "top";
                HtmlTableFrameCell.Attributes.Add("class", "PostTextTable");
                HtmlTableFrameRow.Controls.Add(HtmlTableFrameCell);
                HtmlTableFrameCell = new HtmlTableCell();
                HtmlTableFrameCell.Controls.Add(htmlTableAnonTimeStampMsg);
                HtmlTableFrameCell.Attributes.Add("class", "PostTextTable");
                HtmlTableFrameRow.Controls.Add(HtmlTableFrameCell);
                htmlTableFrame.Controls.Add(HtmlTableFrameRow);
                tc.Controls.Add(ltlDivIndent1);
                tc.Controls.Add(htmlTableFrame);

                tr.Cells.Add(tc);
                table.Rows.Add(tr);

                #endregion

                sqlParameters = new List<SqlParameter>();
                //sqlParameter = new SqlParameter();
                //sqlParameter.ParameterName = "@postId";
                //sqlParameter.Value = Session["postId"].ToString();
                //sqlParameters.Add(sqlParameter);

                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@replyToPostId";
                sqlParameter.Value = postId;
                sqlParameters.Add(sqlParameter);

                DataSet ds = Db.ExecuteQuery("SELECT * FROM post WHERE replyToPostId = @replyToPostId ORDER BY insertTimeStamp DESC", out  boolDbError, out  strDbError, sqlParameters);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    tr = new TableRow();
                    tc = new TableCell();
                    Repeater repeater = new Repeater();
                    repeater.ItemCommand += rptrMessages_ItemCommand;
                    repeater.ItemDataBound += ItemDataBoundRepeater;
                    repeater.DataSource = ds;
                    repeater.DataBind();
                    tc.Controls.Add(repeater);
                    tr.Cells.Add(tc);
                    table.Controls.Add(tr);
                }

                e.Item.Controls.Add(table);
            }

        }

        private void BindingRepeater(object sender, System.EventArgs e) // RepeaterItemEventArgs
        {
            
            //var name = (Repeater)sender;
            //var container = (RepeaterItem)name.NamingContainer;
            //name.DataSource = DataBinder.Eval(container.DataItem, "messageId");
            //string messageId = DataBinder.Eval(container.DataItem, "messageId").ToString();
        }
        protected void VoteBtnClick(object sender, System.EventArgs e)
        {

            var selectedImage = sender as ImageButton;
            string imageUrl = selectedImage.ImageUrl;

        }

        protected void rptrMessages_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            string postId = DataBinder.Eval(e.Item.DataItem, "postId").ToString();

            if (e.CommandName == "VoteUp")
            { 
                
            }
            else if(e.CommandName=="VoteDown")
            { 
            }
        }

        private void IncrementViewCount()
        {

            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            SqlParameter sqlParameter = new SqlParameter();

            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@postId";
            sqlParameter.Value = Session["postId"].ToString();
            sqlParameters.Add(sqlParameter);


            Db.ExecuteNonQuery("UPDATE post SET views=views+1 WHERE postId = @postId", out bool boolDbError, out string strDbError, sqlParameters);

        }

        private void vote(string postId, bool isVoteUp = true)
        {
            //if vote already exist, and the same vote, they wanted to erase their vote. if it's a different vote, they want to change their vote


            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            SqlParameter sqlParameter = new SqlParameter();


            sqlParameters = new List<SqlParameter>();
            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@postId";
            sqlParameter.Value = postId;
            sqlParameters.Add(sqlParameter);

            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@anonId";
            sqlParameter.Value = Session["anonId"];
            sqlParameters.Add(sqlParameter);


            DataSet dataSet = Db.ExecuteQuery("SELECT upVote FROM vote WHERE postId=@postId AND anonId=@anonId", out bool boolDbError, out string strDbError, sqlParameters);

            bool alreadyVoted = false, upVoted = true;
            if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                alreadyVoted = true;
                if (dataSet.Tables[0].Rows[0]["upVote"].ToString().ToLower() == "false" || dataSet.Tables[0].Rows[0]["upVote"].ToString().ToLower() == "-1")
                {
                    upVoted = false;
                }
            }
            if (alreadyVoted)
            {

                sqlParameters = new List<SqlParameter>();
                sqlParameters = new List<SqlParameter>();
                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@postId";
                sqlParameter.Value = postId;
                sqlParameters.Add(sqlParameter);

                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@anonId";
                sqlParameter.Value = Session["anonId"];
                sqlParameters.Add(sqlParameter);

                Db.ExecuteNonQuery("DELETE FROM vote WHERE postId=@postId AND anonId=@anonId", out boolDbError, out strDbError, sqlParameters);
            }


            if (!alreadyVoted || isVoteUp != upVoted)
            {
                sqlParameters = new List<SqlParameter>();

                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@postId";
                sqlParameter.Value = postId;
                sqlParameters.Add(sqlParameter);

                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@anonId";
                sqlParameter.Value = Session["anonId"].ToString();
                sqlParameters.Add(sqlParameter);

                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@upVote";
                if (isVoteUp)
                {
                    sqlParameter.Value = 1;
                }
                else
                {
                    sqlParameter.Value = -1;
                }
                sqlParameters.Add(sqlParameter);

                Db.ExecuteNonQuery("INSERT INTO vote(postId,anonId,upVote) VALUES(@postId,@anonId,@upVote)", out boolDbError, out strDbError, sqlParameters);
            }

            Response.Redirect("Post");
            Response.End();

        }

    }
}