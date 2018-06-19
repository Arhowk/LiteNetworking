using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldLinkData  {
    public string name;
    public List<WorldLinkData> connectedLinks;
    public Vector3 approxLocation;
    public bool isDiscontinuous;

}
