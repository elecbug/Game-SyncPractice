using System.Text;
using System.Text.Json;

namespace Packet
{
    public enum PacketType
    {
        Base,
        User,
    }

    public class Base
    {
        public PacketType Type { get; protected set; } = PacketType.Base;
        public required int SenderId { get; set; }
        public required int ReceiverId { get; set; }

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
        public User() { Type = PacketType.User; }

        public class Color { public int R { get; set; } public int G { get; set; } public int B { get; set; } } 

        public required string Name { get; set; }
        public required Color UserColor { get; set; }
    }
}
