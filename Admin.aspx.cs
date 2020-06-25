using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Anonymous.Classes;

namespace Anonymous
{
    public partial class Admin : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            { 
                Security.IsAdminOrKick();
            }
        }

        protected void btnDissambleDll_Click(object sender, EventArgs e)
        {
            
            
        }
    }
}