using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace SyncPractice
{
    public class MainForm : Form
    {
        public Label Player { get; private set; }
        public List<Label> OtherPlayers { get; private set; } = new List<Label> ();
        public IPEndPoint IP { get; private set; } = new IPEndPoint(IPAddress.Loopback, 0);
        public TcpClient Client { get; private set; } = new TcpClient();
        public int X { get; private set; }
        public int Y { get; private set; }
        public bool Locker { get; private set; } = true;

        public MainForm()
        {
            ClientSize = new Size(800, 600);

            int size = 50;
            int x = ClientSize.Width / 2 - size / 2;
            int y = ClientSize.Height / 2 - size / 2;

            Player = new Label()
            {
                Parent = this,
                Visible = true,
                Location = new Point(x, y),
                Size = new Size(size, size),
                BackColor = Color.Black,
                AutoSize = false,
            };

            InitData();

            KeyDown += MainForm_KeyDown;
            FormClosing += MainForm_FormClosing;
        }

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            Locker = false;
        }

        private void MainForm_KeyDown(object? sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                    {
                        Y -= 1;

                        Invoke(() =>
                        {
                            foreach (var u in OtherPlayers)
                            {
                                u.Location = new Point(u.Location.X, u.Location.Y + 1);
                            }
                        });
                    }
                    break;
                case Keys.Down:
                    {
                        Y += 1;

                        Invoke(() =>
                        {
                            foreach (var u in OtherPlayers)
                            {
                                u.Location = new Point(u.Location.X, u.Location.Y - 1);
                            }
                        });
                    }
                    break;
                case Keys.Left:
                    {
                        X -= 1;

                        Invoke(() =>
                        {
                            foreach (var u in OtherPlayers)
                            {
                                u.Location = new Point(u.Location.X + 1, u.Location.Y);
                            }
                        });
                    }
                    break;
                case Keys.Right:
                    {
                        X += 1;

                        Invoke(() =>
                        {
                            foreach (var u in OtherPlayers)
                            {
                                u.Location = new Point(u.Location.X - 1, u.Location.Y);
                            }
                        });
                    }
                    break;
            }

            Packet.MoveUser user = new Packet.MoveUser() 
            {
                Name = Player.Text,
                X = X,
                Y = Y,
            };

            string json = JsonSerializer.Serialize(user);

            Client.GetStream().Write(BytesPadding(Encoding.UTF8.GetBytes(json)));
        }

        private void InitServer()
        {
            Client.Connect(IP);

            Thread t = new Thread(() =>
            {
                while (Locker == true)
                {
                    byte[] buffer = new byte[1024];
                    int offset = 0;

                    while (offset < buffer.Length)
                    {
                        offset += Client.GetStream().Read(buffer, offset, buffer.Length - offset);
                    }

                    string data = Encoding.UTF8.GetString(buffer).Replace("\0", "");

                    Packet.Base? packet = JsonSerializer.Deserialize<Packet.Base>(data);

                    if (packet == null)
                    {
                        continue;
                    }
                    else
                    {
                        switch (packet.Type)
                        {
                            case Packet.PacketType.NewUser:
                                {
                                    Packet.NewUser user = JsonSerializer.Deserialize<Packet.NewUser>(data)!;

                                    int size = 50;
                                    int x = user.X - X + Player.Location.X;
                                    int y = user.Y - Y + Player.Location.Y;

                                    Invoke(() =>
                                    {
                                        Label label = new Label()
                                        {
                                            Parent = this,
                                            Visible = true,
                                            Location = new Point(x, y),
                                            Size = new Size(size, size),
                                            BackColor = Color.FromArgb(user.UserColor.R, user.UserColor.G, user.UserColor.B),
                                            AutoSize = false,
                                            Text = user.Name,
                                        };

                                        OtherPlayers.Add(label);
                                    });
                                }
                                break;
                            case Packet.PacketType.DeleteUser:
                                {
                                    Packet.DeleteUser user = JsonSerializer.Deserialize<Packet.DeleteUser>(data)!;
                                    Label? target = OtherPlayers.FirstOrDefault(x => x.Text == user.Name);

                                    if (target != null)
                                    {
                                        OtherPlayers.Remove(target);
                                        target.Dispose();
                                    }
                                }
                                break;
                            case Packet.PacketType.MoveUser:
                                {
                                    Packet.MoveUser user = JsonSerializer.Deserialize<Packet.MoveUser>(data)!;
                                    Label? target = OtherPlayers.FirstOrDefault(x => x.Text == user.Name);

                                    if (target != null)
                                    {
                                        Invoke(() =>
                                        {
                                            target.Location = new Point(user.X - X + Player.Location.X, user.Y - Y + Player.Location.Y);
                                        });
                                    }
                                }
                                break;
                            case Packet.PacketType.Chatting:
                                {
                                    Packet.Chatting chatting = JsonSerializer.Deserialize<Packet.Chatting>(data)!;
                                

                                }
                                break;
                        }
                    }
                }
            });

            t.Start();
        }

        private void InitData()
        {
            string file = "init-data.json";

            if (File.Exists(file) == false)
            {
                using (StreamWriter sw = new StreamWriter(file))
                {
                    InitDataJson data = InitDataJson.Default();
                    string json = JsonSerializer.Serialize(data);

                    sw.Write(json);
                }
            }

            using (StreamReader sr = new StreamReader(file))
            {
                InitDataJson? data1 = JsonSerializer.Deserialize<InitDataJson>(sr.ReadToEnd());

                if (data1 == null)
                {
                    using (StreamWriter sw = new StreamWriter(sr.BaseStream))
                    {
                        InitDataJson newData = InitDataJson.Default();
                        string json = JsonSerializer.Serialize(newData);

                        sw.Write(json);
                    }

                    data1 = JsonSerializer.Deserialize<InitDataJson>(sr.ReadToEnd());
                }

                Player.Text = data1!.Name;
                Player.BackColor = Color.FromArgb(data1.Color.R, data1.Color.G, data1.Color.B);
                IP = IPEndPoint.Parse(data1.IpEndPoint);

                string data2 = JsonSerializer.Serialize(new Packet.NewUser()
                {
                    Name = Player.Text,
                    UserColor = data1.Color,
                    X = 0,
                    Y = 0,
                });

                InitServer();

                Client.GetStream().Write(BytesPadding(Encoding.UTF8.GetBytes(data2)));
            }
        }

        private byte[] BytesPadding(byte[] data, int size = 1024)
        {
            byte[] result = new byte[size];

            for (int i = 0; i < data.Length; i++)
            {
                result[i] = data[i];
            }

            return result;
        }

        private class InitDataJson
        {
            public required string IpEndPoint { get; set; }
            public required string Name { get; set; }
            public required Packet.User.Color Color { get; set; }

            public static InitDataJson Default()
            {
                return new InitDataJson()
                {
                    IpEndPoint = Resource.Ip,
                    Name = "User-" + new Random().Next(0, 10000),
                    Color = new Packet.User.Color()
                    {
                        R = new Random().Next(0, 256),
                        G = new Random().Next(0, 256),
                        B = new Random().Next(0, 256),
                    },
                };
            }
        }
    }
}
