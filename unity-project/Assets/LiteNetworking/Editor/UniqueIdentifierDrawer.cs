using UnityEditor;
using UnityEngine;
using System;
/* Credits to unity.com's Ash-Blue */

[CustomPropertyDrawer(typeof(UniqueIdentifierAttribute))]
public class UniqueIdDrawer : PropertyDrawer
{
    public bool hasVerifiedId = false;
    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
    {
        // Generate a unique ID, defaults to an empty string if nothing has been serialized yet
        if (prop.longValue < 1)
        {
            //  Guid guid = Guid.NewGuid();
            long guid = UnityEngine.Random.Range(0, System.Int32.MaxValue) + (((long)UnityEngine.Random.Range(0, System.Int32.MaxValue)) << 32);
            prop.longValue = guid;
        }
        if (hasVerifiedId == false)
        {
            if (!StaticIDManager.RegisterID(prop.longValue, prop.serializedObject.targetObject as UniqueId))
            {
                long guid = UnityEngine.Random.Range(0, System.Int32.MaxValue) + (((long)UnityEngine.Random.Range(0, System.Int32.MaxValue)) << 32);
                prop.longValue = guid;
            }
            hasVerifiedId = true;
        }

        // Place a label so it can't be edited by accident
        Rect textFieldPosition = position;
        textFieldPosition.height = 16;
        DrawLabelField(textFieldPosition, prop, label);
    }

    void DrawLabelField(Rect position, SerializedProperty prop, GUIContent label)
    {
        EditorGUI.LabelField(position, label, new GUIContent(prop.longValue.ToString()));
    }
}
[CustomPropertyDrawer(typeof(PrefabIdentifierAttribute))]
public class PrefabIdDrawer : PropertyDrawer
{
    public bool hasVerifiedId = false;
    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
    {
        UniqueId thisObject = (UniqueId) prop.serializedObject.targetObject;
        GameObject prefabRoot = PrefabUtility.GetPrefabParent(thisObject.gameObject) as GameObject;

        if(thisObject.gameObject == prefabRoot || prefabRoot == null)
        {
            // Generate a unique ID, defaults to an empty string if nothing has been serialized yet 
            if (prop.longValue < 1)
            {
                //  Guid guid = Guid.NewGuid();
                long guid = UnityEngine.Random.Range(0, System.Int32.MaxValue) + (((long)UnityEngine.Random.Range(0, System.Int32.MaxValue)) << 32);
                prop.longValue = guid;
            }
            if (hasVerifiedId == false)
            {
                if (!StaticIDManager.RegisterID(prop.longValue, prop.serializedObject.targetObject as UniqueId))
                {
                    long guid = UnityEngine.Random.Range(0, System.Int32.MaxValue) + (((long)UnityEngine.Random.Range(0, System.Int32.MaxValue)) << 32);
                    prop.longValue = guid;
                }
                hasVerifiedId = true;
            }
        }
        else
        {
            prop.longValue = prefabRoot.GetComponent<UniqueId>().prefabId;
        }



        // Place a label so it can't be edited by accident
        Rect textFieldPosition = position;
        textFieldPosition.height = 16;
        DrawLabelField(textFieldPosition, prop, label);
    }

    void DrawLabelField(Rect position, SerializedProperty prop, GUIContent label)
    {
        EditorGUI.LabelField(position, label, new GUIContent(prop.longValue.ToString()));
    }
}