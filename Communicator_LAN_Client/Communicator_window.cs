using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NetMQ;
using NAudio;
using NAudio.Wave;

namespace Communicator_LAN_Client
{
    public partial class Communicator_window : Form
    {
        public User thisUser;
        bool forcedClose = false;
        int RATE = 22100;
        int BUFFERSIZE = (int)Math.Pow(2, 12);
        public BufferedWaveProvider bwp;

        private Connecting_window parent;
        private string MyName;
        private Size delta;
        private Size speakButton_delta;

        private bool connectingAlready = true;
        private bool serverRequestListener_isRunning = false;


        byte[] exitstream;
        TcpListener listener = null;
        TcpClient client = null;
        NetworkStream stream = null;
        BinaryWriter writer = null;

        public Communicator_window(string serverName, Form _parent, string me)
        {
            InitializeComponent();
            exitstream = new byte[BUFFERSIZE];
            for (int i = 0; i < BUFFERSIZE; i++)
            {
                exitstream[i] = 127;
            }
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
                l = new TcpListener(parent.port);
                        l.Start();

                while (true)
                {
                    try
                    {
                        c = l.AcceptTcpClient();
                        s = c.GetStream();
                        r = new BinaryReader(s);
                        string received = r.ReadString();
                        string header = received.Substring(0, received.IndexOf(':') + 1);
                        if (header == COMMUNICATION_VALUES.CONNECTION_SERVER)
                        {
                            string reason = received.Substring(received.IndexOf(':') + 1, (received.IndexOf('|')) - received.IndexOf(':'));
                            switch (reason)
                            {
                                case COMMUNICATION_VALUES.RECEIVING.YOU_HAVE_BEEN_KICKED:
                                {
                                    MessageBox.Show("Zostałeś wyrzucony z sesji.", "Wyrzucono z serwera", MessageBoxButtons.OK);
                                    forcedClose = true;
                                    c.Close();
                                    c.Dispose();
                                    Invoke(new MethodInvoker(() => {
                                        parent.Info_label.Text = "Zostałeś wyrzucony z " + this.Text;
                                        Close();
                                    }));
                                    break;
                                }
                                case COMMUNICATION_VALUES.RECEIVING.SERVER_SHUT_DOWN:
                                {
                                    MessageBox.Show("Serwer z aktualnie otwartą sesją został zamknięty.", "Zamknięto serwer lub utracono połączenie", MessageBoxButtons.OK);
                                    forcedClose = true;
                                    c.Close();
                                    c.Dispose();
                                    Invoke(new MethodInvoker(() => {
                                        parent.Info_label.Text = "Serwer " + this.Text + " został zamknięty.";
                                        Close();
                                    }));
                                    break;
                                }
                                case COMMUNICATION_VALUES.RECEIVING.YOU_HAVE_BEEN_MUTED:
                                {
                                    Invoke(new MethodInvoker(() =>
                                    {
                                        thisUser.isMuted = true;
                                        defaultBackgroundColor = thisUser.BackColor;
                                        thisUser.BackColor = Color.LightSalmon;
                                        thisUser.Mute_button.Enabled = false;
                                        Speak_button.Enabled = false;
                                    }));
                                    break;
                                }
                                case COMMUNICATION_VALUES.RECEIVING.YOU_HAVE_BEEN_UNMUTED:
                                {
                                    Invoke(new MethodInvoker(() =>
                                    {
                                        thisUser.isMuted = false;
                                        thisUser.BackColor = defaultBackgroundColor;
                                        thisUser.Mute_button.Enabled = true;
                                        Speak_button.Enabled = true;
                                    }));
                                    break;
                                }
                                case COMMUNICATION_VALUES.RECEIVING.REFRESH_YOUR_LIST:
                                {
                                    RefreshClientsList();
                                    break;
                                }
                            }
                        }
                        else if (header==COMMUNICATION_VALUES.CONNECTION_CLIENT)
                        {
                            l.Stop();
                            break;
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            });
            listening.IsBackground = true;
            listening.Start();

            Thread udpListening = new Thread(() => {
                IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 46000);
                UdpClient udpc = new UdpClient(ipep);
                bool stopSignal = false;
                while (true)
                {
                    byte[] receivedbuffer = new byte[BUFFERSIZE];
                    receivedbuffer = udpc.Receive(ref ipep);
                    for (int i = 0; i < BUFFERSIZE; i++)
                    {
                        if (receivedbuffer[i] != exitstream[i])
                        {
                            stopSignal = false;
                            break;
                        }
                        else stopSignal = true;

                    }
                    if (stopSignal)
                    {
                        udpc.Close();
                        udpc.Dispose();
                        break;
                    }
                    WaveOutEvent player = new WaveOutEvent();
                    player.Init(new RawSourceWaveStream(receivedbuffer, 0, receivedbuffer.Length, new WaveFormat(RATE, 1)));
                    bool isMuted = false, isTalking=false;
                    Invoke(new MethodInvoker(() => {
                        isMuted = thisUser.isMuted;
                        isTalking = thisUser.isTalking;
                    }));
                    if(!(thisUser.isMuted || thisUser.isTalking))
                        player.Play();
                }
            }) { IsBackground = true };
            udpListening.Start();

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
            parent.Info_label.ForeColor = Color.Black;
            parent.Show();
            if (!forcedClose)
            {
                parent.Info_label.Text = "Rozłączono z serwerem " + this.Text;
                TcpClient client2 = new TcpClient();
                client2.ConnectAsync(parent.IP_textBox.Text, 45000).Wait(100);
                if (client2.Connected)
                {
                    NetworkStream ns = client2.GetStream();
                    string message = COMMUNICATION_VALUES.CONNECTION_CLIENT + COMMUNICATION_VALUES.SENDING.I_AM_DISCONNECTING + MyName;
                    BinaryWriter bw = new BinaryWriter(ns);
                    bw.Write(message);
                }
            }
            TcpClient c = new TcpClient();
            c.ConnectAsync("127.0.0.1", parent.port).Wait(100);
            if (c.Connected)
            {
                NetworkStream ns = c.GetStream();
                BinaryWriter bw = new BinaryWriter(ns);
                bw.Write(COMMUNICATION_VALUES.CONNECTION_CLIENT);
            }
            UdpClient u = new UdpClient();
            u.Connect("127.0.0.1", 46000);
            u.Send(exitstream, exitstream.Length);

        }

        private void ServerRequestListener(int port)
        {
            serverRequestListener_isRunning = true;
            Thread serverListener = new Thread(()=>
            {
                TcpListener listener = null;
                TcpClient client = null;
                NetworkStream stream = null;
                BinaryReader reader = null;

                while (true)
                {
                    listener = new TcpListener(port);
                    try
                    {
                        listener.Start();
                        client = listener.AcceptTcpClient();
                        Random r = new Random();
                        Thread.Sleep(r.Next(1, 3000));
                        stream = client.GetStream();
                        reader = new BinaryReader(stream);
                        string received = reader.ReadString();
                        string header = received.Substring(0, received.IndexOf(':') + 1);
                        if (header == COMMUNICATION_VALUES.CONNECTION_SERVER)
                        {
                            string reason = received.Substring(received.IndexOf(':') + 1, (received.IndexOf('|')) - received.IndexOf(':'));
                            switch (reason)
                            {
                                case COMMUNICATION_VALUES.RECEIVING.REFRESH_YOUR_LIST:
                                {
                                    if(!connectingAlready)
                                        RefreshClientsList();
                                    break;
                                }
                            }
                        }
                        listener.Stop();
                    } catch (SocketException socketex)
                    {
                        Console.WriteLine("");
                        Console.Write(socketex.Message);
                        Console.Write(socketex.StackTrace);
                    }
                }
            });
            serverListener.IsBackground = true;
            serverListener.Start();
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
            if (name != MyName) u.Mute_button.Visible = false;

            u.Mute_button.Click += new EventHandler(delegate (object o, EventArgs e)
            {
                if (u.BackColor != Color.LightSalmon)
                {
                    u.isMuted= true;
                    defaultBackgroundColor = u.BackColor;
                    u.BackColor = Color.LightSalmon;
                    Speak_button.Enabled = false;
                }
                else
                {
                    u.isMuted = false;
                    u.BackColor = defaultBackgroundColor;
                    Speak_button.Enabled = true;
                }
            });

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
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(()=> {
                    clearList();
                }));
            }
            Thread getClientsListThread = new Thread(() =>
            {
                StreamReader reader;
                string toSend, received;
                bool isEnd = false;
                int iterator = 0;
                while (!isEnd)
                {
                    try { 
                    client = new TcpClient();
                    client.ConnectAsync(parent.IP_textBox.Text, 45000).Wait(500);
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
                                if (username == MyName) { u.Username.Font = new Font(u.Username.Font, FontStyle.Bold);
                                        thisUser = u;
                                    }
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
                    } catch { continue; }
                }
            });
            getClientsListThread.IsBackground = true;
            getClientsListThread.Start();
        }

        private static Color defaultBackgroundColor;

        UdpClient _ud;
        byte[] recordedBytes = new byte[] {0x0};
        bool breakThread = true;

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
                        breakThread = false;
                        thisUser.isTalking = true;
                        
                        Thread sendToServerThread = new Thread(() =>
                        {
                            client = new TcpClient(parent.IP_textBox.Text, 45000);
                            stream = client.GetStream();
                            writer = new BinaryWriter(stream);
                            writer.Write(COMMUNICATION_VALUES.CONNECTION_CLIENT +
                                COMMUNICATION_VALUES.SENDING.TALKING + (c as User).Username.Text);
                            Invoke(new MethodInvoker(() =>
                            {
                                Console.WriteLine("NADAJĘ: ");
                            }));

                            bool localStop = breakThread;
                            bool recording = false;
                            _ud = new UdpClient();
                            _ud.Connect(parent.IP_textBox.Text, parent.port + 1000);
                            WaveInEvent wi = new WaveInEvent();
                            wi.DeviceNumber = 0;
                            wi.WaveFormat = new WaveFormat(RATE, 1);
                            wi.BufferMilliseconds = (int)((double)BUFFERSIZE / (double)RATE * 1000.0);
                            wi.DataAvailable += new EventHandler<WaveInEventArgs>(AudioDataAvailable);
                            while (true)
                            {
                                Invoke(new MethodInvoker(() =>
                                {
                                    localStop = breakThread;
                                }));

                                if (localStop)
                                {
                                    if (recording)
                                    {
                                        _ud.Close();
                                        _ud.Dispose();
                                        wi.StopRecording();
                                        recording = false;
                                    }
                                    break;
                                }
                                else
                                {
                                    try
                                    {
                                        if (!recording)
                                        {
                                            wi.StartRecording();
                                            recording = true;
                                        }
                                    }
                                    catch
                                    {
                                        string msg = "Błąd nagrywania!\n\n";
                                        msg += "Czy mikrofon jest podłączony?\n";
                                        msg += "Czy Twój mikrofon jest domyślnym urządzeniem nagrywającym?";
                                        MessageBox.Show(msg, "Błąd nagrywania", MessageBoxButtons.OK);
                                        recording = false;
                                    }
                                
                                }
                                Thread.Sleep(50);
                            }
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
                        thisUser.isTalking = false;
                        Thread sendToServerThread = new Thread(() =>
                        {
                            try
                            {
                                client = new TcpClient(parent.IP_textBox.Text, 45000);
                                stream = client.GetStream();
                                writer = new BinaryWriter(stream);
                                writer.Write(COMMUNICATION_VALUES.CONNECTION_CLIENT +
                                    COMMUNICATION_VALUES.SENDING.NOT_TALKING + (c as User).Username.Text);
                            } catch { }
                        })
                        { IsBackground = true };
                        sendToServerThread.Start();
                        breakThread = true;
                    }
                }
            }
        }

        private byte[] stringToByte(string IP)
        {
            IPAddress address = IPAddress.Parse(IP);
            byte[] bytes = address.GetAddressBytes();
            return bytes;
        }

        private void clearList()
        {
            UsersPanel.Controls.Clear();
        }

        private void AudioDataAvailable(object sender, WaveInEventArgs e)
        {
            try
            {
                _ud.Send(e.Buffer, e.BytesRecorded);
            }
            catch { }
        }
    }
}
