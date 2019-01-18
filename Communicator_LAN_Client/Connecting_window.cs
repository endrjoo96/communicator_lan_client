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
    public partial class Connecting_window : Form
    {
        public TcpListener listener = null;
        public TcpClient client = null;
        public NetworkStream stream = null;
        public int port = 0;
        BinaryWriter writer = null;
        BinaryReader reader = null;
        Communicator_window window;

        public Connecting_window()
        {
            InitializeComponent();
            Info_label.Text = "";
        }

        private void Connect_button_Click(object sender, EventArgs e)
        {
            Info_label.ForeColor = Color.Black;
            if (Username_textBox.Text == "")
            {
                Info_label.ForeColor = Color.DarkRed;
                Info_label.Text = "Wpisz nazwę użytkownika.";
            }
            else
            {
                Info_label.Text = "Łączenie z serwerem...";
                Thread x = new Thread(() =>
                {
                    try
                    {
                        client = new TcpClient();
                        if (client.ConnectAsync(IP_textBox.Text, 45000).Wait(500))
                        {
                            Invoke(new MethodInvoker(() =>
                            {
                                Info_label.ForeColor = Color.DarkGreen;
                                Info_label.Text = "Połączono.";
                            }));
                            using (stream = client.GetStream())
                            {
                                string toSend, received;
                                toSend = COMMUNICATION_VALUES.CONNECTION_CLIENT + COMMUNICATION_VALUES.SENDING.USERNAME_AND_PASSWORD + Username_textBox.Text + "|" + Password_textBox.Text;
                                writer = new BinaryWriter(stream);
                                writer.Write(toSend);
                                reader = new BinaryReader(stream);
                                received = reader.ReadString();
                                string header = received.Substring(0, received.IndexOf(':') + 1);
                                if (header == COMMUNICATION_VALUES.CONNECTION_SERVER)
                                {
                                    string reason = received.Substring(received.IndexOf(':') + 1, (received.IndexOf('|')) - received.IndexOf(':'));
                                    switch (reason)
                                    {
                                        case COMMUNICATION_VALUES.RECEIVING.SERVER_NAME:
                                        {
                                            string name = received.Substring(received.IndexOf('|') + 1, received.LastIndexOf('|') - 1 - received.IndexOf('|'));
                                            int _port = Convert.ToInt32(received.Substring(received.LastIndexOf('|') + 1));
                                            Invoke(new MethodInvoker(() =>
                                            {
                                                port = _port;
                                                window = new Communicator_window(name, this, Username_textBox.Text);
                                                window.Show();
                                                Hide();
                                            }));
                                            return;
                                        }
                                        case COMMUNICATION_VALUES.RECEIVING.SERVER_FULL:
                                        {
                                            Invoke(new MethodInvoker(() =>
                                            {
                                                Info_label.ForeColor = Color.DarkRed;
                                                Info_label.Text = "Serwer jest pełny.";
                                            }));
                                            break;
                                        }
                                        case COMMUNICATION_VALUES.RECEIVING.PASSWORD_INCORRECT:
                                        {
                                            Invoke(new MethodInvoker(() =>
                                            {
                                                Info_label.ForeColor = Color.DarkRed;
                                                Info_label.Text = "Niepoprawne hasło.";
                                            }));
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else throw new InvalidOperationException();
                    }
                    catch (InvalidOperationException ex1)
                    {
                        Invoke(new MethodInvoker(() =>
                        {
                            Info_label.ForeColor = Color.Red;
                            Info_label.Text = "Nie udało się połączyć z serwerem.";
                        }));
                    }
                    catch (Exception ex2)
                    {
                        Invoke(new MethodInvoker(() =>
                        {
                            Console.WriteLine(ex2.Message);
                            Console.Write(ex2.StackTrace);
                            Info_label.ForeColor = Color.Red;
                            Info_label.Text = "Błąd połączenia.";
                        }));
                    }
                })
                {
                    IsBackground = true
                };
                x.Start();
            }
        }
    }
}
