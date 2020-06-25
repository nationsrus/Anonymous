using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Anonymous.Classes
{
    public class Common
    {

        //TODO: check if new friend requests were accepted, then add correct database records? (due to encrypted data, that needs to be decrypted with user's private key - don't have access to that when friend accepted)
        public static void checkNewlyAcceptedFriendRequests()
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@anonId";
            sqlParameter.Value = HttpContext.Current.Session["anonId"];
            sqlParameters.Add(sqlParameter);
            DataSet dataSet = Db.ExecuteQuery("SELECT * FROM friendRequest WHERE anonId = @anonId AND Accepted = 1", out bool boolDbError, out string strDbError, sqlParameters);

            if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow r in dataSet.Tables[0].Rows)
                {
                    friendResponse(r["friendRequestId"].ToString(), true, string.Empty, string.Empty, true);
                }
            }
        }

        public static void friendResponse(string FriendRequestId, bool accepted = true, string FriendAnonId = "", string encryptedRequesterEmail = "", bool friendAlreadyAccepted=false)
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            SqlParameter sqlParameter = new SqlParameter();

            sqlParameter.ParameterName = "@friendRequestId";
            sqlParameter.Value = FriendRequestId;
            sqlParameters.Add(sqlParameter);

            if (FriendAnonId == string.Empty)
            {
                DataSet dataSet = Db.ExecuteQuery("SELECT EncryptionKeyEncryptedByPublicKey,encryptedAnonIdRequestingFriendship FROM friendRequest WHERE FriendRequestId=@FriendRequestId", out bool _boolDbError, out string _strDbError, sqlParameters);
                if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
                {
                    string EncryptionKey = Security.Rsa.Decrypt(dataSet.Tables[0].Rows[0]["EncryptionKeyEncryptedByPublicKey"].ToString(), HttpContext.Current.Session["PrivateKey"].ToString());

                    FriendAnonId = Security.DecryptStringAES(dataSet.Tables[0].Rows[0]["encryptedAnonIdRequestingFriendship"].ToString(), EncryptionKey);
                }
            }

            sqlParameters = new List<SqlParameter>();
            sqlParameter = new SqlParameter();

            sqlParameter.ParameterName = "@friendRequestId";
            sqlParameter.Value = FriendRequestId;
            sqlParameters.Add(sqlParameter);

            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@Accepted";
            sqlParameter.Value = accepted;
            sqlParameters.Add(sqlParameter);

            //TODO: delete instead?
            //Db.ExecuteNonQuery("UPDATE friendRequest SET Accepted = @Accepted WHERE friendRequestId = @friendRequestId", out bool boolDbError, out string strDbError, sqlParameters);
            Db.ExecuteNonQuery("DELETE FROM friendRequest WHERE friendRequestId = @friendRequestId", out bool boolDbError, out string strDbError, sqlParameters);

            if (accepted || friendAlreadyAccepted)
            {
                sqlParameters = new List<SqlParameter>();
                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@anonId";
                sqlParameter.Value = HttpContext.Current.Session["anonId"];
                sqlParameters.Add(sqlParameter);

                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@email";
                if (encryptedRequesterEmail != string.Empty)
                {
                    sqlParameter.Value = Security.EncryptStringAES(encryptedRequesterEmail, HttpContext.Current.Session["hashedPassword"].ToString());
                }
                else
                {
                    sqlParameter.Value = DBNull.Value;
                }
                sqlParameters.Add(sqlParameter);

                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@encryptedFriendAnonId";
                sqlParameter.Value = Security.EncryptStringAES(FriendAnonId, HttpContext.Current.Session["hashedPassword"].ToString());
                sqlParameters.Add(sqlParameter);

                Db.ExecuteNonQuery("INSERT INTO friend(anonId,email,encryptedFriendAnonId) VALUES(@anonId,@email,@encryptedFriendAnonId)", out boolDbError, out strDbError, sqlParameters);
                //TODO: add a friendRequest that's accepted for other anon to have friend record added?

            }

            if (accepted && !friendAlreadyAccepted)
            {
                // insert reverse friendRequest
                FriendRequestAdd(FriendAnonId, string.Empty, string.Empty,true);
            }

            if (FriendAnonId != string.Empty)
            {
                DeleteOtherRequestsFromSameAnon(FriendAnonId);
            }
        }

        private static void DeleteOtherRequestsFromSameAnon(string FriendAnonId)
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@anonId";
            sqlParameter.Value = HttpContext.Current.Session["anonId"];
            sqlParameters.Add(sqlParameter);
            DataSet dataSet = Db.ExecuteQuery("SELECT * FROM friendRequest WHERE anonId = @anonId AND Accepted IS NULL", out bool boolDbError, out string strDbError, sqlParameters);

            if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow r in dataSet.Tables[0].Rows)
                {
                    string EncryptionKey = Security.Rsa.Decrypt(r["EncryptionKeyEncryptedByPublicKey"].ToString(), HttpContext.Current.Session["PrivateKey"].ToString());
                    string encryptedAnonIdRequestingFriendship = Security.DecryptStringAES(r["encryptedAnonIdRequestingFriendship"].ToString(), EncryptionKey);
                    if (encryptedAnonIdRequestingFriendship == FriendAnonId)
                    {
                        sqlParameters = new List<SqlParameter>();
                        sqlParameter = new SqlParameter();
                        sqlParameter.ParameterName = "@friendRequestId";
                        sqlParameter.Value = r["friendRequestId"].ToString();
                        sqlParameters.Add(sqlParameter);

                        //TODO: delete instead?
                        //Db.ExecuteNonQuery("UPDATE friendRequest SET Accepted = 0 WHERE friendRequestId=@friendRequestId", out boolDbError, out strDbError, sqlParameters);
                        Db.ExecuteNonQuery("DELETE FROM friendRequest WHERE Accepted IS NULL AND friendRequestId=@friendRequestId", out boolDbError, out strDbError, sqlParameters);
                    }


                }
            }
        }

        public static void FriendRequestAdd(string AnonId, string friendEmail, string friendAddMsg, bool ThisIsReverseAcceptedAdd=false)
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@anonId";
            sqlParameter.Value = AnonId;
            sqlParameters.Add(sqlParameter);
            DataSet dataSet = Db.Common.AnonByAnonId(out bool boolDbError, out string strDbError, sqlParameters);

            if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                //string friendAnonId = dataSet.Tables[0].Rows[0]["anonId"].ToString();
                bool CorrectAnonIdAndEmailAddress = false;
                //string friendPublicKey = dataSet.Tables[0].Rows[0]["PublicKey"].ToString();

                string randomAutoGeneratedPassword = System.Web.Security.Membership.GeneratePassword(40, 12);

                string encryptedAnonId = Security.EncryptStringAES(HttpContext.Current.Session["anonId"].ToString(), randomAutoGeneratedPassword);
                string encryptedMsg = string.Empty;
                if (friendAddMsg != string.Empty)
                {
                    encryptedMsg = Security.EncryptStringAES(friendAddMsg, randomAutoGeneratedPassword);
                }
                string encryptedRequesterEmail = Security.EncryptStringAES(HttpContext.Current.Session["email"].ToString(), randomAutoGeneratedPassword);

                sqlParameters = new List<SqlParameter>();
                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@anonId";
                sqlParameter.Value = AnonId;
                sqlParameters.Add(sqlParameter);
                sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = "@email";
                sqlParameter.Value = Security.Hash(friendEmail.ToLower().Trim());
                sqlParameters.Add(sqlParameter);
                DataSet dataSet1 = Db.ExecuteQuery("SELECT anonId FROM anon WHERE anonId=@anonId AND email=@email", out boolDbError, out strDbError, sqlParameters);
                if (dataSet1.Tables.Count > 0 && dataSet1.Tables[0].Rows.Count > 0)
                {
                    CorrectAnonIdAndEmailAddress = true;
                }

                if (dataSet.Tables[0].Rows[0]["FriendRequestRequireEmailAddress"].ToString() == "0" || dataSet.Tables[0].Rows[0]["FriendRequestRequireEmailAddress"].ToString().ToLower() == "false" || CorrectAnonIdAndEmailAddress || ThisIsReverseAcceptedAdd)
                {
                    // insert record
                    sqlParameters = new List<SqlParameter>();
                    sqlParameter = new SqlParameter();
                    sqlParameter.ParameterName = "@anonId";
                    sqlParameter.Value = AnonId;
                    sqlParameters.Add(sqlParameter);

                    sqlParameter = new SqlParameter();
                    sqlParameter.ParameterName = "@encryptedAnonIdRequestingFriendship";
                    sqlParameter.Value = encryptedAnonId;
                    sqlParameters.Add(sqlParameter);

                    sqlParameter = new SqlParameter();
                    sqlParameter.ParameterName = "@encryptedMsg";
                    if (encryptedMsg != string.Empty)
                    {
                        sqlParameter.Value = encryptedMsg;
                    }
                    else
                    {
                        sqlParameter.Value = DBNull.Value;
                    }
                    sqlParameters.Add(sqlParameter);

                    sqlParameter = new SqlParameter();
                    sqlParameter.ParameterName = "@EncryptionKeyEncryptedByPublicKey";
                    sqlParameter.Value = Security.Rsa.Encrypt(randomAutoGeneratedPassword, dataSet.Tables[0].Rows[0]["PublicKey"].ToString()); // encrypted with friend's public key
                    sqlParameters.Add(sqlParameter);

                    sqlParameter = new SqlParameter();
                    sqlParameter.ParameterName = "@friendEmail";
                    if (CorrectAnonIdAndEmailAddress)
                    {
                        sqlParameter.Value = Security.EncryptStringAES(friendEmail, HttpContext.Current.Session["hashedPassword"].ToString());
                    }
                    else
                    {
                        sqlParameter.Value = DBNull.Value;
                    }
                    sqlParameters.Add(sqlParameter);

                    sqlParameter = new SqlParameter();
                    sqlParameter.ParameterName = "@encryptedRequesterEmail";
                    if (CorrectAnonIdAndEmailAddress)
                    {
                        sqlParameter.Value = encryptedRequesterEmail;
                    }
                    else
                    {
                        sqlParameter.Value = DBNull.Value;
                    }
                    sqlParameters.Add(sqlParameter);

                    sqlParameter = new SqlParameter();
                    sqlParameter.ParameterName = "@accepted";
                    if (ThisIsReverseAcceptedAdd)
                    {
                        sqlParameter.Value = 1;
                    }
                    else
                    {
                        sqlParameter.Value = DBNull.Value;
                    }
                    sqlParameters.Add(sqlParameter);


                    Db.ExecuteNonQuery("INSERT INTO friendRequest(anonId,encryptedAnonIdRequestingFriendship,encryptedMsg,EncryptionKeyEncryptedByPublicKey,friendEmail,encryptedRequesterEmail,accepted) VALUES(@anonId,@encryptedAnonIdRequestingFriendship,@encryptedMsg,@EncryptionKeyEncryptedByPublicKey,@friendEmail,@encryptedRequesterEmail,@accepted)", out boolDbError, out strDbError, sqlParameters);

                    //lblSystemMsg.Text = String.Empty;
                }
                else
                {
                    //lblSystemMsg.Text = "Unable to find record or user does not allow friend request without email address.";
                }

            }
            else
            {
                //lblSystemMsg.Text = "Unable to find record";
            }
        }

        public static void MessageSend(string friendAnonId, string msg)
        {

            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            SqlParameter sqlParameter = new SqlParameter();

            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@anonId";
            sqlParameter.Value = friendAnonId;
            sqlParameters.Add(sqlParameter);

            DataSet dataSetAnonFriend = Db.ExecuteQuery("SELECT * FROM anon WHERE anonId= @anonId", out bool boolDbError, out string strDbError, sqlParameters);

            string insertDateTime = DateTime.Now.ToString();

            // insert record for friend
            string randomAutoGeneratedPassword = System.Web.Security.Membership.GeneratePassword(40, 12);

            string encryptedRandomAutoGeneratedPassword = Security.Rsa.Encrypt(randomAutoGeneratedPassword, dataSetAnonFriend.Tables[0].Rows[0]["PublicKey"].ToString());

            string encryptedMsg = Security.EncryptStringAES(msg, randomAutoGeneratedPassword);

            string EncryptedFriendAnonId = Security.EncryptStringAES(HttpContext.Current.Session["anonId"].ToString(), randomAutoGeneratedPassword);

            string EncryptedInsertTimeStamp = Security.EncryptStringAES(insertDateTime, randomAutoGeneratedPassword);

            string EncryptedSenderAnonId = Security.EncryptStringAES(HttpContext.Current.Session["anonId"].ToString(), randomAutoGeneratedPassword);

            sqlParameters = new List<SqlParameter>();

            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@anonId";
            sqlParameter.Value = friendAnonId;
            sqlParameters.Add(sqlParameter);

            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@encryptedMsg";
            sqlParameter.Value = encryptedMsg;
            sqlParameters.Add(sqlParameter);

            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@EncryptionKeyEncryptedByPublicKey";
            sqlParameter.Value = encryptedRandomAutoGeneratedPassword;
            sqlParameters.Add(sqlParameter);

            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@EncryptedFriendAnonId";
            sqlParameter.Value = EncryptedFriendAnonId;
            sqlParameters.Add(sqlParameter);

            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@EncryptedInsertTimeStamp";
            sqlParameter.Value = EncryptedInsertTimeStamp;
            sqlParameters.Add(sqlParameter);

            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@EncryptedSenderAnonId";
            sqlParameter.Value = EncryptedSenderAnonId;
            sqlParameters.Add(sqlParameter);

            Db.ExecuteNonQuery("INSERT INTO privateMessage(anonId,encryptedMsg,EncryptionKeyEncryptedByPublicKey,EncryptedFriendAnonId,EncryptedInsertTimeStamp,EncryptedSenderAnonId) VALUES(@anonId,@encryptedMsg,@EncryptionKeyEncryptedByPublicKey,@EncryptedFriendAnonId,@EncryptedInsertTimeStamp,@EncryptedSenderAnonId)", out boolDbError, out strDbError, sqlParameters);

            // insert record for self

            randomAutoGeneratedPassword = System.Web.Security.Membership.GeneratePassword(40, 12);

            encryptedRandomAutoGeneratedPassword = Security.Rsa.Encrypt(randomAutoGeneratedPassword, HttpContext.Current.Session["PublicKey"].ToString());

            encryptedMsg = Security.EncryptStringAES(msg, randomAutoGeneratedPassword);

            EncryptedFriendAnonId = Security.EncryptStringAES(friendAnonId, randomAutoGeneratedPassword);

            EncryptedInsertTimeStamp = Security.EncryptStringAES(insertDateTime, randomAutoGeneratedPassword);

            EncryptedSenderAnonId = Security.EncryptStringAES(HttpContext.Current.Session["anonId"].ToString(), randomAutoGeneratedPassword);

            sqlParameters = new List<SqlParameter>();
            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@anonId";
            sqlParameter.Value = HttpContext.Current.Session["anonId"];
            sqlParameters.Add(sqlParameter);

            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@encryptedMsg";
            sqlParameter.Value = encryptedMsg;
            sqlParameters.Add(sqlParameter);

            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@EncryptionKeyEncryptedByPublicKey";
            sqlParameter.Value = encryptedRandomAutoGeneratedPassword;
            sqlParameters.Add(sqlParameter);

            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@EncryptedFriendAnonId";
            sqlParameter.Value = EncryptedFriendAnonId;
            sqlParameters.Add(sqlParameter);

            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@EncryptedInsertTimeStamp";
            sqlParameter.Value = EncryptedInsertTimeStamp;
            sqlParameters.Add(sqlParameter);

            sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@EncryptedSenderAnonId";
            sqlParameter.Value = EncryptedSenderAnonId;
            sqlParameters.Add(sqlParameter);

            Db.ExecuteNonQuery("INSERT INTO privateMessage(anonId,encryptedMsg,EncryptionKeyEncryptedByPublicKey,EncryptedFriendAnonId,EncryptedInsertTimeStamp,EncryptedSenderAnonId) VALUES(@anonId,@encryptedMsg,@EncryptionKeyEncryptedByPublicKey,@EncryptedFriendAnonId,@EncryptedInsertTimeStamp,@EncryptedSenderAnonId)", out boolDbError, out strDbError, sqlParameters);

        }

    }
}