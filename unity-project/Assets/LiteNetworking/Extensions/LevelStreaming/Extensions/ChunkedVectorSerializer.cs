using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using LiteNetworking;

[LiteNetworking.CustomSerializer(typeof(Vector3), typeof(LevelStreaming.Position))]
[LiteNetworking.RecordTargetPlayer]
public class ChunkedVectorSerializer : LiteByteSerializer<Vector3>
{
    public M_liteVector3Serializer defaultSerializer = new M_liteVector3Serializer();
    public override Vector3 Deserialize(MemoryStream b)
    {
        if (Networking.isServer)
        {
            int playerChunk = LiteNetworking.Networking.localPacketPlayer?.GetChunkId() ?? 0;
            Vector3 offset = ChunkHandler.i.GetChunkOffset(playerChunk);
            Debug.Log("DS Player offset is " + offset);
            return defaultSerializer.Deserialize(b) + offset;
        }
        else
        {
            return defaultSerializer.Deserialize(b);
        }

    }

    public override byte[] Serialize(Vector3 t)
    {
        if (Networking.isServer)
        {
            int playerChunk = LiteNetworking.Networking.localPacketPlayer?.GetChunkId() ?? 0;
            Vector3 offset = ChunkHandler.i.GetChunkOffset(playerChunk);
            Debug.Log("S Player offset is " + offset);
            return defaultSerializer.Serialize(t - offset);
        }
        else
        {
            return defaultSerializer.Serialize(t);
        }
    }
}
