using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.IO;
using System.Text;
using LiteNetworking;

/*
 *  Sorry this file is a mess. Was the first file in this project.
 *  Getting refactored soon
 */

public class LiteLocalOnly : Attribute
{

}
namespace LiteNetworking
{
    // Packet attiributes
    public class LitePacket
    {
        public static int executingClient;
        public virtual void Execute() { }
        public virtual bool Verify() { return true; }

        public bool CheckAuthority(LitePlayer player)
        {
            return true;
        }

        public LitePlayer GetExecutingClient()
        {
            return Networking.GetPlayer(executingClient);
        }
    }

    public class RecordTargetPlayer : System.Attribute
    { }

    public abstract class M_LitePacketInternalMirror
    {
        public int packetId;

        public abstract void Fire(MemoryStream m);
    }

    public class PacketStructDefinition
    {
        public System.Type connectedType;
        public List<PropertyAttribute> properties;

    }


    public class Networking
    {
        public static LitePlayer localPacketPlayer;
        private static Dictionary<System.Type, PacketStructDefinition> structDefinitions;
        public static Dictionary<System.Type, object> dataSerializers;
        public static LitePlayer localPlayer;
        public static Dictionary<int, LitePlayer> players = new Dictionary<int, LitePlayer>();
        public static bool isServer
        {
            get
            {
                return LobbyConnector.isServer;
            }
        }

        static Networking()
        {
            GenerateAllPacketStructDefinitions();
        }

        public static LitePlayer GetPlayer(int id)
        {
            return players[id];
        }

        public static void TransmitPacket(MemoryStream m, int connectionId)
        {
            m.Position = 0;
            SocketSender.SendPacket(m, connectionId);
            // Decode it
            //m.Position = 0;
            //LiteNetworkingGenerated.PacketReader.ReadPacket(m);
        }

        public static void TransmitPacket(LitePacket e)
        {
           /* SyncTransformPacket pkt = e as SyncTransformPacket;

            MemoryStream s = new MemoryStream();
            // Write the packte id
            byte[] b = { 1, 0 };
            s.Write(b, 0, 2);

            // WRite hte packet data
            (new exp_SyncTransformPacket_pkt())._Serialize(pkt, s);

            // Deserialize it
            a12f1449ac.DispersePacket(s);*/

        }

        public static LitePlayer GetLocalPlayer()
        {
            return localPlayer;
        }

        public static LitePlayer GetAuthority(GameObject g)
        {
            return g.GetComponent<NetworkAuthority>().owner;
        }

        public static bool HasLocalAuthority(GameObject e)
        {
            return GetAuthority(e) == GetLocalPlayer();
        }

        public static bool IsBot()
        {
            return GetLocalPlayer().IsBot();
        }

        public static void SetupSerializers()
        {
            /*dataSerializers[typeof(System.Single)] = new M_liteFloatSerializer();
            dataSerializers[typeof(System.Int32)] = new M_liteIntSerializer();
            dataSerializers[typeof(System.Int64)] = new M_liteLongSerializer();
            dataSerializers[typeof(System.String)] = new M_liteStringSerializer();
            dataSerializers[typeof(System.Boolean)] = new M_liteBoolSerializer();
            dataSerializers[typeof(System.Char)] = new M_liteCharSerializer();
            dataSerializers[typeof(Vector2)] = new M_liteVector2Serializer();
            dataSerializers[typeof(Vector3)] = new M_liteVector3Serializer();
            dataSerializers[typeof(GameObject)] = new M_liteEntitySerializer();
            dataSerializers[typeof(Transform)] = new M_liteTransformSerializer();*/
        }

        /*
         * EAR Baking 
         */
        private static void GenerateAllPacketStructDefinitions()
        {
            var subclasses = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(LitePacket)));

            foreach (Type t in subclasses)
            {
                GeneratePacketStructDefinition(t);
            }
        }
        private static void GeneratePacketStructDefinition(Type packetType)
        {
            // Extract all of the properties
            var props = packetType.GetProperties()
                         .Where(propertyInfo => propertyInfo.GetCustomAttributes(false).GetType() != typeof(LiteLocalOnly));

            // Build a manifest out of them
            PacketStructDefinition structDef = new PacketStructDefinition();

            foreach (PropertyInfo prop in props)
            {
                // Check possible byte encryption modes
                System.Type t = prop.GetType();
            }
        }
    }

}

// Byte encryption
public enum ByteEncryptionMode
{
    HIGH_COMPRESSION,
    LOW_COMPRESSION,
    NONE
}

public abstract class LiteByteSerializer<T>
{
    public abstract byte[] Serialize(T t);
    public abstract T Deserialize(MemoryStream b);
}
/*
public class Data_serializers_const
{
    public static M_liteFloatSerializer ser_Single = new M_liteFloatSerializer();
    public static M_liteIntSerializer ser_Int32 = new M_liteIntSerializer();
    public static M_liteLongSerializer ser_Int64 = new M_liteLongSerializer();
    public static M_liteStringSerializer ser_String = new M_liteStringSerializer();
    public static M_liteBoolSerializer ser_Boolean = new M_liteBoolSerializer();
    public static M_liteCharSerializer ser_Char = new M_liteCharSerializer();
    public static M_liteVector2Serializer ser_Vector2 = new M_liteVector2Serializer();
    public static M_liteVector3Serializer ser_Vector3 = new M_liteVector3Serializer();
    public static M_liteEntitySerializer ser_NetworkedEntity = new M_liteEntitySerializer();
    public static M_liteTransformSerializer ser_Transform = new M_liteTransformSerializer();
}*/

/*
public class M_liteStringSerializer : LiteByteSerializer<string>
{
    public override byte[] Serialize(string f)
    {
        return new byte[0];
    }

    public override string  Deserialize(MemoryStream f)
    {
        return "";
    }
}*/

public class M_liteIntSerializer : LiteByteSerializer<int>
{

    public override byte[] Serialize(int f)
    {
        return BitConverter.GetBytes(f);
    }

    public override int Deserialize(MemoryStream f)
    {
        byte[] reader = new byte[4];
        f.Read(reader, 0, 4);

        return BitConverter.ToInt32(reader, 0);
    }
}

public class M_liteDoubleSerializer : LiteByteSerializer<double>
{

    public override byte[] Serialize(double f)
    {
        return BitConverter.GetBytes(f);
    }

    public override double Deserialize(MemoryStream f)
    {
        byte[] reader = new byte[8];
        f.Read(reader, 0, 8);
        return BitConverter.ToDouble(reader, 0);
    }
}


public class M_liteBoolSerializer : LiteByteSerializer<bool>
{

    public override byte[] Serialize(bool f)
    {
        return BitConverter.GetBytes(f);
    }

    public override bool Deserialize(MemoryStream f)
    {
        byte[] reader = new byte[1];
        f.Read(reader, 0, 1);
        return BitConverter.ToBoolean(reader, 0);
    }
}
public class M_liteFloatSerializer : LiteByteSerializer<float>
{

    public override byte[] Serialize(float f)
    {
        return BitConverter.GetBytes(f);
    }

    public override float Deserialize(MemoryStream f)
    {
        byte[] reader = new byte[4];
        f.Read(reader, 0, 4);
        return BitConverter.ToSingle(reader, 0);
    }
}

public class M_liteCharSerializer : LiteByteSerializer<char>
{
    public override byte[] Serialize(char f)
    {
        return new byte[1] { (byte)f };
    }

    public override char Deserialize(MemoryStream f)
    {
        return (char) f.ReadByte();
    }
}

public class M_liteLongSerializer : LiteByteSerializer<long>
{
    public override byte[] Serialize(long f)
    {
        return BitConverter.GetBytes(f);
    }

    public override long Deserialize(MemoryStream f)
    {
        byte[] reader = new byte[8];
        f.Read(reader, 0, 8);
        return BitConverter.ToInt64(reader, 0);
    }
}

// We're doing our own custom string serialization here because the default string serialization doesn't support
// doing it from a split section of a stream, only a whole stream
public class M_liteArraySerializer : LiteByteSerializer<string>
{
    public override byte[] Serialize(string f)
    {
        byte[] b = new byte[f.Length + 1];
        b[0] = (byte) f.Length;
        byte[] ascii = Encoding.ASCII.GetBytes(f);

        for (int i = 0; i < ascii.Length; i++)
        {
            b[i + 1] = ascii[i];
        }

        return b;
    }

    public override string Deserialize(MemoryStream f)
    {
        int length = f.ReadByte();
        byte[] contents = new byte[length];
        f.Read(contents, 0, length);
        return Encoding.ASCII.GetString(contents);
    }
}

public class M_liteEntitySerializer : LiteByteSerializer<NetworkedEntity>
{
    public override byte[] Serialize(NetworkedEntity f)
    {
        uint entId = f.EntityIndex;

        return new byte[] { (byte)(entId & 0xFF), (byte)((entId >> 8) & 0xFF) };
    }

    public override NetworkedEntity Deserialize(MemoryStream f)
    {
        int val = f.ReadByte() + (f.ReadByte() << 8);
        //Debug.Log("Receive data for the entity " + val);
        return EntityManager.GetEntity((uint)val);
    }
}

public class M_liteTransformSerializer : LiteByteSerializer<Transform>
{   
    public override byte[] Serialize(Transform f)
    {
        byte[][][] bytes = new byte[][][]{
            new byte[][]
            {
                BitConverter.GetBytes(f.position.x), BitConverter.GetBytes(f.position.y), BitConverter.GetBytes(f.position.z)
            }, new byte[][]
            {
                BitConverter.GetBytes(f.rotation.x), BitConverter.GetBytes(f.rotation.y), BitConverter.GetBytes(f.rotation.z)
            }, new byte[][]
            {
                BitConverter.GetBytes(f.localScale.x), BitConverter.GetBytes(f.localScale.y), BitConverter.GetBytes(f.localScale.z)
            }
        };

        byte[] ret = new byte[3 * 3 * 4];

        for(int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                for(int k = 0; k < 4; k++)
                {
                    ret[k + j * 4 + i * 12] = bytes[i][j][k];
                }
            }
        }

        return ret;
     
    }

    public override Transform Deserialize(MemoryStream f)
    {
        return MonoBehaviour.FindObjectOfType<GameObject>().transform;
    }
}


public class M_liteVector2Serializer : LiteByteSerializer<Vector2>
{
    public override byte[] Serialize(Vector2 f)
    {
        return new byte[0];
    }

    public override Vector2 Deserialize(MemoryStream f)
    {
        return new Vector2();
    }
}
public class M_liteVector3Serializer : LiteByteSerializer<Vector3>
{
    public override byte[] Serialize(Vector3 f)
    {
        byte[] bx = BitConverter.GetBytes(f.x);
        byte[] by = BitConverter.GetBytes(f.y);
        byte[] bz = BitConverter.GetBytes(f.z);
        byte[] bt = new byte[bx.Length + by.Length + bz.Length];
        for(int i = 0; i < bx.Length; i++)
        {
            bt[i] = bx[i];
        }
        for (int i = 0; i < by.Length; i++)
        {
            bt[i + bx.Length] = by[i];
        }
        for (int i = 0; i < bz.Length; i++)
        {
            bt[i + bx.Length + by.Length] = bz[i];
        }
        return bt;
    }

    public override Vector3 Deserialize(MemoryStream f)
    {
        byte[] bx = new byte[12];

        f.Read(bx, 0, 12);

        return new Vector3(BitConverter.ToSingle(bx, 0), BitConverter.ToSingle(bx, 4), BitConverter.ToSingle(bx, 8));
    }
}
