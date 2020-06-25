using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Anonymous.Classes;
using System.Data;

namespace Anonymous
{
    public partial class Friend : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
            {

                Security.IsLoggedInOrKick();

                if (Session["FriendId"] != null)
                {
                    friendAnonLoad();
                }

            }

        }

        private void friendAnonLoad()
        {

            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            SqlParameter sqlParameter = new SqlParameter();

            sqlParameter.ParameterName = "@FriendId";
            sqlParameter.Value = Session["FriendId"].ToString();
            sqlParameters.Add(sqlParameter);

            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@anonId";
            sqlParameter.Value = Session["anonId"].ToString();
            sqlParameters.Add(sqlParameter);

            DataSet dataSet = Db.ExecuteQuery("SELECT * FROM friend WHERE FriendId = @FriendId AND anonId = @anonId", out bool boolDbError, out string strDbError, sqlParameters);

            if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {

                ltlFriendAnonId.Text = Security.DecryptStringAES(dataSet.Tables[0].Rows[0]["encryptedFriendAnonId"].ToString(), Session["hashedPassword"].ToString());
                if (dataSet.Tables[0].Rows[0]["nickName"] != null && dataSet.Tables[0].Rows[0]["nickName"].ToString() != string.Empty)
                {
                    tbxNickname.Text = Security.DecryptStringAES(dataSet.Tables[0].Rows[0]["nickName"].ToString(), Session["hashedPassword"].ToString());
                }
                if (dataSet.Tables[0].Rows[0]["email"] != null && dataSet.Tables[0].Rows[0]["email"].ToString() != string.Empty)
                {
                    tbxEmail.Text = Security.DecryptStringAES(dataSet.Tables[0].Rows[0]["email"].ToString(), Session["hashedPassword"].ToString());
                }
                lblSystemMsg.Visible = false;
            }
            else
            {
                lblSystemMsg.Visible = true;
                lblSystemMsg.Text = "Error loading anon-friend data.";
            }

        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            SqlParameter sqlParameter = new SqlParameter();

            //sqlParameter.ParameterName = "@encryptedFriendAnonId";
            //sqlParameter.Value = Security.EncryptStringAES(Session["FriendAnonId"].ToString(), Session["hashedPassword"].ToString());
            //sqlParameters.Add(sqlParameter); // strange this value doesn't match? but yet decrypt works and correct value is retrieved???

            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@FriendId";
            sqlParameter.Value = Session["FriendId"].ToString();
            sqlParameters.Add(sqlParameter);

            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@anonId";
            sqlParameter.Value = Session["anonId"].ToString();
            sqlParameters.Add(sqlParameter);

            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@nickName";
            if (tbxNickname.Text != string.Empty)
            {
                sqlParameter.Value = Security.EncryptStringAES(tbxNickname.Text, Session["hashedPassword"].ToString());
            }
            else
            { 
                sqlParameter.Value = DBNull.Value;
            }
            sqlParameters.Add(sqlParameter);

            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@email";
            if (tbxEmail.Text != string.Empty)
            {
                sqlParameter.Value = Security.EncryptStringAES(tbxEmail.Text, Session["hashedPassword"].ToString());
            }
            else
            {
                sqlParameter.Value = DBNull.Value;
            }
            sqlParameters.Add(sqlParameter);

            Db.ExecuteNonQuery("UPDATE friend SET nickName=@nickName, email=@email WHERE anonId = @anonId AND FriendId = @FriendId", out bool boolDbError, out string strDbError, sqlParameters);
            //Db.ExecuteNonQuery("UPDATE friend SET nickName=@nickName, email=@email WHERE encryptedFriendAnonId = @encryptedFriendAnonId AND anonId = @anonId AND firendId = @firendId", out bool boolDbError, out string strDbError, sqlParameters);

            Session.Remove("FriendId");
            Session.Remove("FriendAnonId");
            Response.Redirect("Friends");
            Response.End();
        }
    }
}