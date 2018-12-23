using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Communicator_LAN_Client
{
    public partial class Form1 : Form
    {
        TcpListener listener = null;
        TcpClient client = null;
        NetworkStream stream = null;
        BinaryWriter writer = null;
        BinaryReader reader = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void Connect_button_Click(object sender, EventArgs e)
        {
            Thread x = new Thread(()=>
            {
                try
                {
                    using (client = new TcpClient(IP_textBox.Text, 65505))
                    {
                        using (stream = client.GetStream())
                        {
                            string send, received;
                            send = "LOGIN:"+Username_textBox.Text+"|"+Password_textBox;
                            writer = new BinaryWriter(stream);
                            writer.Write(send);
                            reader = new BinaryReader(stream);
                        }
                    }
                }
                catch { }
            });
            x.IsBackground = true;
            x.Start();
            
        }
    }
}
