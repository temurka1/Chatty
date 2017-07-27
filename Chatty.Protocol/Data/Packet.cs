using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Chatty.Protocol.Data
{
    [Serializable]
    public class Packet
    {
        public readonly List<string> data;
        public readonly string       senderID;
        public readonly PacketType   type;

        public Packet(PacketType type, string senderID, List<string> data)
        {
            this.type       = type;
            this.senderID   = senderID;
            this.data       = data;
        }

        public Packet(byte[] packetBytes)
        {
            BinaryFormatter bf = new BinaryFormatter();

            using (MemoryStream ms = new MemoryStream(packetBytes))
            {
                Packet pack = (Packet) bf.Deserialize(ms);

                data       = pack.data;
                senderID   = pack.senderID;
                type       = pack.type;
            }
        }

        public byte[] ToBytes()
        {
            BinaryFormatter bf = new BinaryFormatter();

            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, this);
                return ms.ToArray();
            }
        }
    }
}
