using System.Drawing;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;

public class Program
{
    private class User
    {
        public required string Name { get; set; }
        public required Packet.User.Color Color { get; set; }
        public required int X { get; set; }
        public required int Y { get; set; }
    }

    private static void Main(string[] args)
    {
        List<User> users = new List<User>();
        List<TcpClient> clients = new List<TcpClient>();

        TcpListener listener = TcpListener.Create(int.Parse(args[0]));
        listener.Start();

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            clients.Add(client);

            new Thread(() =>
            {
                TcpClient current = client;
                string name = "";

                try
                {
                    while (true)
                    {
                        byte[] buffer = new byte[1024];
                        int offset = 0;

                        while (offset < buffer.Length)
                        {
                            offset += client.GetStream().Read(buffer, offset, buffer.Length - offset);
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
                                        name = user.Name;

                                        if (users.Find(x => x.Name == name) != null)
                                        {
                                            break;
                                        }

                                        lock (users)
                                        {
                                            users.Add(new User()
                                            {
                                                Name = user.Name,
                                                Color = new Packet.User.Color()
                                                {
                                                    R = user.UserColor.R,
                                                    G = user.UserColor.G,
                                                    B = user.UserColor.B,
                                                },
                                                X = user.X,
                                                Y = user.Y,
                                            });

                                            Console.WriteLine(data);
                                        }

                                        foreach (var c in clients)
                                        {
                                            if (c.Connected == true && c != client)
                                            {
                                                c.GetStream().Write(BytesPadding(Encoding.UTF8.GetBytes(data)));
                                            }
                                        }

                                        foreach (var u in users)
                                        {
                                            if (u.Name != user.Name)
                                            {
                                                string json = JsonSerializer.Serialize(new Packet.NewUser()
                                                {
                                                    Name = u.Name,
                                                    UserColor = u.Color,
                                                    X = u.X,
                                                    Y = u.Y,
                                                });

                                                client.GetStream().Write(BytesPadding(Encoding.UTF8.GetBytes(json)));
                                            }
                                        }
                                    }
                                    break;
                                case Packet.PacketType.MoveUser:
                                    {
                                        Packet.MoveUser user = JsonSerializer.Deserialize<Packet.MoveUser>(data)!;

                                        User u = users.Find(x => x.Name == user.Name)!;

                                        u.X = user.X;
                                        u.Y = user.Y;

                                        Console.WriteLine(data);

                                        foreach (var c in clients)
                                        {
                                            if (c.Connected == true && c != client)
                                            {
                                                c.GetStream().Write(BytesPadding(Encoding.UTF8.GetBytes(data)));
                                            }
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
                }
                catch
                {
                    lock (clients)
                    {
                        clients.Remove(current);
                            
                        Packet.DeleteUser user = new Packet.DeleteUser()
                        {
                            Name = name,
                        };

                        string json = JsonSerializer.Serialize(user);

                        foreach (var c in clients)
                        {
                            c.GetStream().Write(BytesPadding(Encoding.UTF8.GetBytes(json)));
                        }
                    }
                    lock (users)
                    {
                        User? u = users.FirstOrDefault(x => x.Name == name);

                        if (u != null)
                        {
                            users.Remove(u);
                        }
                    }
                }
            }).Start();
        }
    }

    private static byte[] BytesPadding(byte[] data, int size = 1024)
    {
        byte[] result = new byte[size];

        for (int i = 0; i < data.Length; i++)
        {
            result[i] = data[i];
        }

        return result;
    }
}