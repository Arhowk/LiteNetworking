using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace LiteNetworking
{
    public partial class NetworkedEntity
    {
        private int loadedChunkId;

        public int GetChunkId()
        {
            return loadedChunkId;
        }

        public bool IsVisibleToPlayer(LitePlayer p)
        {
            return p.GetChunkId() == GetChunkId();
        }
    }
}

