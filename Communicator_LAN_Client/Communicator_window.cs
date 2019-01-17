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
    public partial class Communicator_window : Form
    {
        private Connecting_window parent;
        private string MyName;
        private Size delta;
        private Size speakButton_delta;

        TcpListener listener = null;
        TcpClient client = null;
        NetworkStream stream = null;
        BinaryWriter writer = null;
        BinaryReader reader = null;

        public Communicator_window(string serverName, Form _parent, string me)
        {
            InitializeComponent();
            this.Text = serverName;
            CurrentServer_label.Text = "Jesteś na serwerze " + serverName;
            parent = _parent as Connecting_window;

            Thread listening = new Thread(()=>
            {
                TcpListener l = null;
                TcpClient c = null;
                NetworkStream s = null;
                BinaryWriter w = null;
                BinaryReader r = null;

                c = new TcpClient();
                c.Connect(parent.IP_textBox.Text, 65505);
                s = c.GetStream();
                r = new BinaryReader(s);
                string receivedMessage = r.ReadString();
                int x = 0;

            });
            listening.IsBackground = true;
            listening.Start();

            listener = parent.listener;
            client = parent.client;
            stream = parent.stream;

            MyName = me;
            delta = new Size(Width - UsersPanel.Width, Height - UsersPanel.Height);
            speakButton_delta = new Size(Width - Speak_button.Width, Height - Speak_button.Location.Y);
            RefreshClientsList();
        }

        private void Communicator_window_FormClosing(object sender, FormClosingEventArgs e)
        {
            parent.Close();
        }

        private void Communicator_window_Resize(object sender, EventArgs e)
        {
            UsersPanel.Size = new Size(Width - delta.Width, Height - delta.Height);
            Speak_button.Size = new Size(Width - speakButton_delta.Width, Speak_button.Height);
            Speak_button.Location = new Point(Speak_button.Location.X, Height - speakButton_delta.Height);
            foreach (Control c in UsersPanel.Controls)
            {
                if (c.GetType() == typeof(User))
                {
                    if (UsersPanel.VerticalScroll.Visible)
                        c.Width = UsersPanel.Width - 20;
                    else
                        c.Width = UsersPanel.Width - 5;
                }
            }
        }

        public User createClient(string name, string address)
        {
            User u = new User();
            if (UsersPanel.VerticalScroll.Visible)
                u.Width = UsersPanel.Width - 20;
            else
                u.Width = UsersPanel.Width - 5;
            u.Location = new Point(u.Location.X, 27 * UsersPanel.Controls.Count);
            u.Username.Text = name;
            return u;
        }

        private void addClient(User u)
        {
            UsersPanel.Controls.Add(u);
            if (UsersPanel.HorizontalScroll.Visible)
                Communicator_window_Resize(this, null);
        }

        private void RefreshClientsList()
        {
            Thread getClientsListThread = new Thread(() =>
            {

                StreamReader reader;
                string toSend, received;
                bool isEnd = false;
                int iterator = 0;
                while (!isEnd)
                {
                    client = new TcpClient();
                    client.ConnectAsync(parent.IP_textBox.Text, 65505).Wait(500);
                    stream = client.GetStream();
                    toSend = COMMUNICATION_VALUES.CONNECTION_CLIENT +
                        COMMUNICATION_VALUES.SENDING.SEND_ME_CLIENT + iterator.ToString();
                    writer = new BinaryWriter(stream);
                    writer.Write(toSend);
                    
                    reader = new StreamReader(stream);
                    received = reader.ReadToEnd();
                    if (received == null)
                        continue;
                    string header = received.Substring(1, received.IndexOf(':'));
                    if (header == COMMUNICATION_VALUES.CONNECTION_SERVER)
                    {
                        string reason = received.Substring(received.IndexOf(':') + 1, (received.IndexOf('|')) - received.IndexOf(':'));
                        switch (reason)
                        {
                            case COMMUNICATION_VALUES.RECEIVING.NEXT_CLIENT_FROM_LIST:
                            {
                                string username = received.Substring(received.IndexOf('|') + 1, received.LastIndexOf('|') - 1 - received.IndexOf('|'));
                                string ip = received.Substring(received.LastIndexOf('|') + 1);
                                User u = createClient(username, ip);
                                if (username == MyName) { u.Username.Font = new Font(u.Username.Font, FontStyle.Bold); }
                                Invoke(new MethodInvoker(() =>
                                {
                                    addClient(u);
                                }));
                                break;
                            }
                            case COMMUNICATION_VALUES.RECEIVING.END_OF_LIST:
                            {
                                isEnd = true;
                                break;
                            }
                        }
                    }
                    iterator++;
                }

            });
            getClientsListThread.IsBackground = true;
            getClientsListThread.Start();
        }

        private static Color defaultBackgroundColor;

        private void Speak_button_MouseDown(object sender, MouseEventArgs e)
        {
            foreach (Control c in UsersPanel.Controls)
            {
                if (c.GetType() == typeof(User))
                {
                    if ((c as User).Username.Text == MyName)
                    {
                        defaultBackgroundColor = (c as User).BackColor;
                        (c as User).BackColor = Color.LightBlue;

                        Thread sendToServerThread = new Thread(() =>
                        {
                            client = new TcpClient(parent.IP_textBox.Text, 65505);
                            stream = client.GetStream();
                            writer = new BinaryWriter(stream);
                            writer.Write(COMMUNICATION_VALUES.CONNECTION_CLIENT +
                                COMMUNICATION_VALUES.SENDING.TALKING+(c as User).Username.Text);
                        })
                        { IsBackground = true };
                        sendToServerThread.Start();

                        break;
                    }
                }
            }
            
        }

        private void Speak_button_MouseUp(object sender, MouseEventArgs e)
        {
            foreach (Control c in UsersPanel.Controls)
            {
                if (c.GetType() == typeof(User))
                {
                    if ((c as User).Username.Text == MyName)
                    {
                        (c as User).BackColor = defaultBackgroundColor;

                        Thread sendToServerThread = new Thread(() =>
                        {
                            client = new TcpClient(parent.IP_textBox.Text, 65505);
                            stream = client.GetStream();
                            writer = new BinaryWriter(stream);
                            writer.Write(COMMUNICATION_VALUES.CONNECTION_CLIENT +
                                COMMUNICATION_VALUES.SENDING.NOT_TALKING + (c as User).Username.Text);
                        })
                        { IsBackground = true };
                        sendToServerThread.Start();
                    }
                }
            }
        }
    }
}
