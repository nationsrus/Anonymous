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
    public partial class AccountDestruction : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
            {
                Security.IsLoggedInOrKick();
            }

        }

        protected void btnDestroy_Click(object sender, EventArgs e)
        {
            // delete friends
            // inform friends?
            // delete private messages

            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            SqlParameter sqlParameter = new SqlParameter();
            bool boolDbError = false;
            string strDbError = string.Empty;

            if (cbxInformFriends.Checked)
            {
                sqlParameters = new List<SqlParameter>();

                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@anonId";
                sqlParameter.Value = Session["anonId"].ToString();
                sqlParameters.Add(sqlParameter);

                DataSet dataSetFriend = Db.ExecuteQuery("SELECT * FROM friend WHERE anonId=@anonId", out boolDbError, out strDbError, sqlParameters);
                if (dataSetFriend.Tables.Count > 0 && dataSetFriend.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in dataSetFriend.Tables[0].Rows)
                    {

                        Common.MessageSend(Security.DecryptStringAES(row["encryptedFriendAnonId"].ToString(), Session["hashedPassword"].ToString()), "[*SYSTEM MESSAGE* - This account has been deleted or purged all its data.]");

                    }
                }
            }

            if (cbxDeletePublicMessages.Checked)
            {
                sqlParameters = new List<SqlParameter>();

                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@anonId";
                sqlParameter.Value = Session["anonId"].ToString();
                sqlParameters.Add(sqlParameter);

                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@msg";
                sqlParameter.Value = "[*SYSTEM MESSAGE* - This account has been deleted.]";
                sqlParameters.Add(sqlParameter);

                Db.ExecuteQuery("UPDATE post SET msg = @msg WHERE anonId = @anonId", out boolDbError, out strDbError, sqlParameters);
            }

            if (cbxTotalAnnihilation.Checked)
            {
                sqlParameters = new List<SqlParameter>();

                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@anonId";
                sqlParameter.Value = Session["anonId"].ToString();
                sqlParameters.Add(sqlParameter);

                Db.ExecuteQuery("DELETE FROM anon WHERE anonId = @anonId", out boolDbError, out strDbError, sqlParameters);

                Session.Clear();
                Response.Redirect("Login");
                Response.End();
            }

            lblSystemMsg.Text = "Account data deleted.";

        }

    }
}