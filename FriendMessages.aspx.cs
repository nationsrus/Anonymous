using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Anonymous.Classes;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.HtmlControls;

namespace Anonymous
{
    public partial class FriendMessages : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Security.IsLoggedInOrKick();
                rptrMessages_load();
                ltlAnonFriend_load();
                checkAnonRecord();
            }
        }

        protected void rptrMessages_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                string decryptedSenderAnonId = DataBinder.Eval(e.Item.DataItem, "decryptedSenderAnonId").ToString();
                string decryptedInsertTimeStamp = DataBinder.Eval(e.Item.DataItem, "decryptedInsertTimeStamp").ToString();
                string decryptedMsg = DataBinder.Eval(e.Item.DataItem, "decryptedMsg").ToString();
                HtmlGenericControl divMsg = e.Item.FindControl("divMsg") as HtmlGenericControl;

                if (DateTime.TryParse(decryptedInsertTimeStamp, out DateTime InsertTimeStamp) && DateTime.TryParse(Session["LastLoginDateTime"].ToString(), out DateTime LastLoginDateTime) && InsertTimeStamp < LastLoginDateTime && decryptedMsg.Length>200)
                {
                    divMsg.Style.Add("height", "32px");
                    divMsg.Style.Add("border-bottom", "black dashed 1px");
                }

                Label lblWho = e.Item.FindControl("lblWho") as Label;
                if (decryptedSenderAnonId == Session["anonId"].ToString())
                {
                    lblWho.Text = "You";
                    divMsg.Attributes.Add("class","PrivateMsgSent");
                }
                else
                {
                    lblWho.Text = "Anon-" + Session["FriendAnonId"].ToString();
                    divMsg.Attributes.Add("class", "PrivateMsgReceived");
                }
            }

        }

        private void ltlAnonFriend_load()
        {
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
                if (Security.DecryptStringAES(row["encryptedFriendAnonId"].ToString(), Session["hashedPassword"].ToString()) == Session["FriendAnonId"].ToString())
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

                    string decryptedNickNameAndEmail = "[Anon-" + Session["FriendAnonId"].ToString() + "] ";
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

        private void rptrMessages_load()
        {
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

                DataTable dataTable = dataSet.Tables[0];

                dataTable.Columns.Add("decryptedFriendAnonId");
                dataTable.Columns.Add("decryptedMsg");
                dataTable.Columns.Add("decryptedEncryptionKey");
                dataTable.Columns.Add("decryptedInsertTimeStamp");
                dataTable.Columns.Add("decryptedSenderAnonId"); 

                foreach (DataRow row in dataTable.Rows)
                {

                    row["decryptedEncryptionKey"] = Security.Rsa.Decrypt(row["EncryptionKeyEncryptedByPublicKey"].ToString(), Session["PrivateKey"].ToString());
                    row["decryptedFriendAnonId"] = Security.DecryptStringAES(row["EncryptedFriendAnonId"].ToString(), row["decryptedEncryptionKey"].ToString());
                    row["decryptedMsg"] = Security.DecryptStringAES(row["encryptedMsg"].ToString(), row["decryptedEncryptionKey"].ToString());
                    row["decryptedInsertTimeStamp"] = Security.DecryptStringAES(row["EncryptedInsertTimeStamp"].ToString(), row["decryptedEncryptionKey"].ToString());
                    row["decryptedSenderAnonId"] = Security.DecryptStringAES(row["EncryptedSenderAnonId"].ToString(), row["decryptedEncryptionKey"].ToString());
                }

                DataView dv = new DataView(dataTable);
                dv.RowFilter = "(decryptedFriendAnonId = '" + Session["FriendAnonId"].ToString() + "')";
                dv.Sort = "decryptedInsertTimeStamp";

                rptrMessages.DataSource = dv;
                rptrMessages.DataBind();
            }
            else
            {
            }



        }

        protected void btnMessageSend_Click(object sender, EventArgs e)
        {
            Common.MessageSend(Session["FriendAnonId"].ToString(), tbxMsg.Text);
            rptrMessages_load();
        }

        private void checkAnonRecord()
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            SqlParameter sqlParameter = new SqlParameter();

            sqlParameters = new List<SqlParameter>();
            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@anonId";
            sqlParameter.Value = Session["FriendAnonId"].ToString();
            sqlParameters.Add(sqlParameter);

            DataSet dataSetAnonFriend = Db.Common.AnonByAnonId(out bool boolDbError, out string strDbError, sqlParameters);
            if (dataSetAnonFriend.Tables.Count == 0 || dataSetAnonFriend.Tables[0].Rows.Count == 0)
            {
                tbxMsg.Text = "[Friend's Anon account no longer exists. Messaging is disabled.]";
                tbxMsg.Enabled = false;
                btnMessageSend.Enabled = false;
                btnMessageSend.ToolTip = "Friend's Anon account no longer exists. Messaging is disabled.";
            }

        }
    }
}