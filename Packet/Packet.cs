using System.Text;
using System.Text.Json;

namespace Packet
{
    public enum PacketType
    {
        Base,
        User,
        Chatting,
        NewUser,
        DeleteUser,
        MoveUser,
    }

    public class Base
    {
        public PacketType Type { get; protected set; }

        public Base(PacketType type)
        {
            Type = type;
        }

        public byte[] ToBytes()
        {
            string json = JsonSerializer.Serialize(this);
            return Encoding.UTF8.GetBytes(json);
        }

        public static Base? FromBytes(byte[] buffer, PacketType type)
        {
            string json = Encoding.UTF8.GetString(buffer);

            switch (type)
            {
                case PacketType.Base: return JsonSerializer.Deserialize<Base>(json);
                case PacketType.User: return JsonSerializer.Deserialize<User>(json);
                default: return null;
            }
        }
    }

    public class User : Base
    {
        public User(PacketType type) : base(type) { }

        public class Color { public int R { get; set; } public int G { get; set; } public int B { get; set; } }

        public required string Name { get; set; }
    }

    public class NewUser : User
    {
        public NewUser() : base(PacketType.NewUser) { }

        public required Color UserColor { get; set; }
        public required int X { get; set; }
        public required int Y { get; set; }
    }

    public class DeleteUser : User
    {
        public DeleteUser() : base(PacketType.DeleteUser) { }
    }

    public class MoveUser : User
    {
        public MoveUser() : base(PacketType.MoveUser) { }

        public required int X { get; set; }
        public required int Y { get; set; }
    }

    public class Chatting : Base
    {
        public Chatting() : base(PacketType.Chatting) { }

        public required string Message { get; set; }
    }
}
