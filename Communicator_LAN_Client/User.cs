using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Communicator_LAN_Client
{
    public partial class User : UserControl
    {
        public User()
        {
            InitializeComponent();
        }

        private void User_Resize(object sender, EventArgs e)
        {
            Mute_button.Location = new Point(Width - 47, Mute_button.Location.Y);
        }
    }
}
