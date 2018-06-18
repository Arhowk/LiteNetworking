using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//[Networking.CustomSerializer(typeof(Vector3))]
public class ChunkedVectorSerializer : LiteByteSerializer<Vector3>
{
    public override Vector3 Deserialize(MemoryStream b)
    {
        //int playerChunk = Networking.localPacketPlayer.chunk;

        return Vector3.zero;
    }

    public override byte[] Serialize(Vector3 t)
    {

        return new byte[0];
    }
}
