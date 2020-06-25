using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Anonymous.Classes;

namespace Anonymous
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            { 
                DisplayBuildChecksum();

                if (Security.IsLoggedIn())
                {
                    liMe.Visible = true;
                    liFriends.Visible = true;
                    liMessages.Visible = true;
                    liLogout.Visible = true;
                    liLogin.Visible = false;

                    Common.checkNewlyAcceptedFriendRequests();
                }
            }
        }

        private void DisplayBuildChecksum()
        {
            //ltlBuildChecksum.Text = Security.AppDllHash();
        }
    }
}