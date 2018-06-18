using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Netdsgf {

    [MenuItem("Networking/Generate Protobufs")]
    public static void GenerateProto()
    {
        LiteNetworking.PktDefGenerator.Generate();
    }
}
