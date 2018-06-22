
using LiteNetworking;
using UnityEngine;
public class SpawnEntityPacket : LitePacket {
    public int entityId;
    public long prefabId;
    public int authority;
    public Vector3 position;

    public override void Execute()
    {
        EntityManager.RegenerateEntity(entityId, prefabId, authority, position);
    }
}

public class RemoveEntity : LitePacket
{
    public int entityId;

    public override void Execute()
    {
        
    }
}

public class RegisterPreplacedEntity : LitePacket
{
    long instanceId;
    int newEntityId;
}

public class UpdatePreplacedEntity : LitePacket
{
    long instanceId;
    int newEntityId;
}


