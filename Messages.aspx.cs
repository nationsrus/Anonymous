using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Anonymous.Classes;
using System.Data.SqlClient;
using System.Data;

namespace Anonymous
{
    public partial class Messages : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Security.IsLoggedInOrKick();
                ddlFriends_load();
                if (Request.QueryString["Action"] != null && Request.QueryString["Action"].ToString() == "NewMsg")
                {
                    if (Session["NewMsgAnonId"] != null)
                    {
                        ddlFriends.SelectedValue = Session["NewMsgAnonId"].ToString();
                    }
                }

                rptrMessages_load();
            }
        }

        private void ddlFriends_load()
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            SqlParameter sqlParameter = new SqlParameter();

            sqlParameters = new List<SqlParameter>();
            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@anonId";
            sqlParameter.Value = Session["anonId"];
            sqlParameters.Add(sqlParameter);

            DataSet dataSet = Db.ExecuteQuery("SELECT * FROM friend WHERE anonId= @anonId", out bool boolDbError, out string strDbError, sqlParameters);

            if (dataSet.Tables.Count > 0)
            {
                DataTable dataTable = dataSet.Tables[0];
                dataTable.Columns.Add("decryptedFriendAnonId", typeof(string));
                dataTable.Columns.Add("decryptedEmail", typeof(string));
                dataTable.Columns.Add("decryptedNickName", typeof(string));
                dataTable.Columns.Add("decryptedNickNameAndEmail", typeof(string));

                foreach (DataRow row in dataTable.Rows)
                {
                    string FriendAnonId = Security.DecryptStringAES(row["encryptedFriendAnonId"].ToString(), Session["hashedPassword"].ToString());
                    sqlParameters = new List<SqlParameter>();
                    sqlParameter = new SqlParameter();
                    sqlParameter.ParameterName = "@anonId";
                    sqlParameter.Value = FriendAnonId;
                    sqlParameters.Add(sqlParameter);

                    DataSet dataSetAnonFriend = Db.Common.AnonByAnonId(out boolDbError, out strDbError, sqlParameters);
                    if (dataSetAnonFriend.Tables.Count > 0 && dataSetAnonFriend.Tables[0].Rows.Count > 0)
                    {
                        row["decryptedFriendAnonId"] = FriendAnonId;

                        bool hasEmail = false, hasNickName = false;
                        if (row["email"] != null && row["email"].ToString() != string.Empty)
                        {
                            hasEmail = true;
                            row["decryptedEmail"] = Security.DecryptStringAES(row["email"].ToString(), Session["hashedPassword"].ToString());
                        }

                        if (row["nickName"] != null && row["nickName"].ToString() != string.Empty)
                        {
                            row["decryptedNickName"] = Security.DecryptStringAES(row["nickName"].ToString(), Session["hashedPassword"].ToString());
                            hasNickName = true;
                        }

                        string decryptedNickNameAndEmail = "[Anon-" + row["decryptedFriendAnonId"].ToString() + "] ";
                        if (hasNickName && hasEmail)
                        {
                            decryptedNickNameAndEmail += row["decryptedNickName"].ToString() + " (" + row["decryptedEmail"].ToString() + ")";
                        }
                        else if (hasNickName)
                        {
                            decryptedNickNameAndEmail += row["decryptedNickName"].ToString();
                        }
                        else if (hasEmail)
                        {
                            decryptedNickNameAndEmail += row["decryptedEmail"].ToString();
                        }

                        row["decryptedNickNameAndEmail"] = decryptedNickNameAndEmail;

                    }
                    else
                    {
                        row.Delete();
                    }

                }

                ddlFriends.DataSource = dataTable;
                ddlFriends.DataTextField = "decryptedNickNameAndEmail";
                ddlFriends.DataValueField = "decryptedFriendAnonId";
                ddlFriends.DataBind();
            }
        }

        protected void btnMessageSendOnClick(object sender, EventArgs e)
        {

            Common.MessageSend(ddlFriends.SelectedValue,tbxMsg.Text);
            rptrMessages_load();
            tbxMsg.Text = string.Empty;

        }

        private void rptrMessages_load()
        {
            // need to load just anonId,name and then count of messages? then go to next page with messages content
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            SqlParameter sqlParameter = new SqlParameter();

            sqlParameters = new List<SqlParameter>();
            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@anonId";
            sqlParameter.Value = Session["anonId"];
            sqlParameters.Add(sqlParameter);

            DataSet dataSet = Db.ExecuteQuery("SELECT * FROM privateMessage WHERE anonId= @anonId", out bool boolDbError, out string strDbError, sqlParameters);

            if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                lblNoMessages.Visible = false;

                DataTable dataTable = dataSet.Tables[0];

                dataTable.Columns.Add("decryptedFriendAnonId");
                //dataTable.Columns.Add("decryptedMsg");
                dataTable.Columns.Add("decryptedEncryptionKey");
                dataTable.Columns.Add("IsThisNewerThanLastLoginTimeStamp");

                foreach (DataRow row in dataTable.Rows)
                {

                    row["decryptedEncryptionKey"] = Security.Rsa.Decrypt(row["EncryptionKeyEncryptedByPublicKey"].ToString(), Session["PrivateKey"].ToString());
                    row["decryptedFriendAnonId"] = Security.DecryptStringAES(row["EncryptedFriendAnonId"].ToString(), row["decryptedEncryptionKey"].ToString());

                    if (DateTime.TryParse(Security.DecryptStringAES(row["EncryptedInsertTimeStamp"].ToString(), row["decryptedEncryptionKey"].ToString()), out DateTime decryptedLastLoginDateTime) && DateTime.TryParse(Session["LastLoginDateTime"].ToString(), out DateTime LastLoginDateTime) && decryptedLastLoginDateTime > LastLoginDateTime)
                    {
                        row["IsThisNewerThanLastLoginTimeStamp"] = "1";
                    }
                    else
                    {
                        row["IsThisNewerThanLastLoginTimeStamp"] = "0";
                    }

                }

                var dataTableSortedAndCounted = from row in dataTable.AsEnumerable()
                                                group row by row.Field<string>("decryptedFriendAnonId") into anonMessageList
                                                select new
                                                {
                                                    decryptedFriendAnonId = anonMessageList.Key,
                                                    CountOfMessages = anonMessageList.Count(),
                                                };
                var dataTableNewMessagesCount = from row in dataTable.AsEnumerable()
                                                where row.Field<string>("IsThisNewerThanLastLoginTimeStamp") == "1"
                                                group row by row.Field<string>("decryptedFriendAnonId") into anonMessageList
                                                select new
                                                {
                                                    decryptedFriendAnonId = anonMessageList.Key,
                                                    CountOfNewMessages = anonMessageList.Count(),
                                                };
                var finalDataTable = from t1 in dataTableSortedAndCounted.AsEnumerable()
                                     join t2 in dataTableNewMessagesCount.AsEnumerable()
                                     on t1.decryptedFriendAnonId equals t2.decryptedFriendAnonId
                                     select new
                                     {
                                         t1.decryptedFriendAnonId,
                                         t1.CountOfMessages,
                                         t2.CountOfNewMessages,
                                     };
                if (dataTableNewMessagesCount.ToList().Count() > 0)
                {
                    rptrMessages.DataSource = finalDataTable;
                }
                else
                {
                    rptrMessages.DataSource = dataTableSortedAndCounted;
                }
                rptrMessages.DataBind();
            }
            else
            {
                lblNoMessages.Visible = true;
            }

        }

        protected void rptrMessages_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                string decryptedFriendAnonId = DataBinder.Eval(e.Item.DataItem, "decryptedFriendAnonId").ToString();

                Literal ltlAnonFriend= e.Item.FindControl("ltlAnonFriend") as Literal;
                HiddenField hfDecryptedFriendAnonId = e.Item.FindControl("hfDecryptedFriendAnonId") as HiddenField;
                hfDecryptedFriendAnonId.Value = decryptedFriendAnonId;

                Label lblCountOfNewMessages = e.Item.FindControl("lblCountOfNewMessages") as Label;
                try
                {
                    if (DataBinder.Eval(e.Item.DataItem, "CountOfNewMessages") != null)
                    {
                        lblCountOfNewMessages.Text = DataBinder.Eval(e.Item.DataItem, "CountOfNewMessages").ToString();
                    }
                }
                catch {
                    lblCountOfNewMessages.Text = "0";
                }

                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                SqlParameter sqlParameter = new SqlParameter();

                sqlParameters = new List<SqlParameter>();
                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@anonId";
                sqlParameter.Value = Session["anonId"];
                sqlParameters.Add(sqlParameter);

                DataSet dataSet = Db.ExecuteQuery("SELECT * FROM friend WHERE anonId= @anonId", out bool boolDbError, out string strDbError, sqlParameters);

                dataSet.Tables[0].Columns.Add("decryptedEmail");
                dataSet.Tables[0].Columns.Add("decryptedNickName");
                dataSet.Tables[0].Columns.Add("decryptedFriendAnonId");

                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    if (Security.DecryptStringAES(row["encryptedFriendAnonId"].ToString(), Session["hashedPassword"].ToString()) == decryptedFriendAnonId)
                    {

                        bool hasEmail = false, hasNickName = false;
                        if (row["email"] != null && row["email"].ToString() != string.Empty)
                        {
                            hasEmail = true;
                            row["decryptedEmail"] = Security.DecryptStringAES(row["email"].ToString(), Session["hashedPassword"].ToString());
                        }

                        if (row["nickName"] != null && row["nickName"].ToString() != string.Empty)
                        {
                            row["decryptedNickName"] = Security.DecryptStringAES(row["nickName"].ToString(), Session["hashedPassword"].ToString());
                            hasNickName = true;
                        }

                        string decryptedNickNameAndEmail = "[Anon-" + decryptedFriendAnonId + "] ";
                        if (hasNickName && hasEmail)
                        {
                            decryptedNickNameAndEmail += row["decryptedNickName"].ToString() + " (" + row["decryptedEmail"].ToString() + ")";
                        }
                        else if (hasNickName)
                        {
                            decryptedNickNameAndEmail += row["decryptedNickName"].ToString();
                        }
                        else if (hasEmail)
                        {
                            decryptedNickNameAndEmail += row["decryptedEmail"].ToString();
                        }

                        ltlAnonFriend.Text = decryptedNickNameAndEmail;
                    }
                }
            }
        }

        protected void btnMessageView_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            RepeaterItem item = (RepeaterItem)btn.NamingContainer;
            HiddenField hfDecryptedFriendAnonId = item.FindControl("hfDecryptedFriendAnonId") as HiddenField;

            Session["FriendAnonId"] = hfDecryptedFriendAnonId.Value;

            Response.Redirect("FriendMessages");
            Response.End();
        }
    }
}