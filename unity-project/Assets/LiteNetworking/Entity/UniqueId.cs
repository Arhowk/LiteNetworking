using UnityEngine;
using System.Collections;

/* Credits to unity.com's Ash-Blue */

// Placeholder for UniqueIdDrawer script
public class UniqueIdentifierAttribute : PropertyAttribute {
}

public class PrefabIdentifierAttribute : PropertyAttribute {
}

public class UniqueId : MonoBehaviour
{
    [PrefabIdentifier]
    public long prefabId;

    [UniqueIdentifier]
    public long uniqueId;

}