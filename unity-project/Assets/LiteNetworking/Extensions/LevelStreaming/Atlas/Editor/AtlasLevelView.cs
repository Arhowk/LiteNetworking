using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class AtlasLevelView  {
    // Scene data
    private AtlasWorld connectedWorld;

    // Positioning data
    public Vector2 position;
    public float width, height;
    public Rect rect;
    public GUIStyle style;
    public Vector2 localDrag = Vector2.zero;
    public string text;

    public bool isDragged = false, isSelected = false, isResizing = false;

    /* Some Dumb Debug Stuff */
    public string[] pref = new string[] { "Heralding", "Superior", "Mighty", "Agile" };
    public string[] post = new string[] { "Chicken", "Goose", "ScalableBufferManager", "Bow" };

    public AtlasLevelView(Vector2 position, float width, float height, GUIStyle style)
    {
        rect = new Rect(position.x, position.y, width, height);
        this.style = style;
        text = pref[Random.Range(0, 4)] + "\n" + post[Random.Range(0, 4)];
    }

    /* Event Handling */

    // Return true to mark the gui as dirty
    public bool ProcessEvents(Event e, Vector2 offset)
    {
       // Debug.Log("proc ev");
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    Vector2 realMousePosition = e.mousePosition - offset;
                    Debug.Log("Real mouse x is " + realMousePosition.x);
                    //Rect realRect = new Rect(realMousePosition.x, realMousePosition.y, width, height);
                    if (rect.Contains(realMousePosition - localDrag))
                    {
                        isDragged = true;
                        GUI.changed = true;
                        isSelected = true;
                        Debug.Log("Contained!!");
                        //style = selectedNodeStyle;
                    }
                    else
                    {
                        Debug.Log("No : " + rect.xMin);
                        GUI.changed = true;
                        isSelected = false;
                       // style = defaultNodeStyle;
                    }   
                }

                if (e.button == 1 && isSelected && rect.Contains(e.mousePosition))
                {
                    OnContextMenu(e);
                    e.Use();
                }
                break;

            case EventType.MouseUp:
                isDragged = false;
                break;

            case EventType.MouseDrag:
                if (e.button == 0 && isDragged)
                {
                    OnDrag(e.delta);
                    e.Use();
                    return true;
                }
                break;
        }

        return false;
    }

    public void OnContextMenu(Event e)
    {

    }

    public void OnDrag(Vector2 delta)
    {
        Debug.Log("DragNode " + delta);
        localDrag += delta;
        

    }

    public void Draw(Vector2 offset)
    {
        // Draw the background
        Rect drawBox = new Rect(rect.xMin + offset.x + localDrag.x, rect.yMin + offset.y + localDrag.y, rect.width, rect.height);
        GUI.Box(drawBox, "", style);

        Vector2 textPosition = new Vector2(drawBox.xMin, drawBox.yMin);
        GUI.Label(new Rect(textPosition.x+25 , textPosition.y+25  , 100000000000,200), new GUIContent(text));

        // Draw the onmesh links

        // Draw the offmesh links

        // Draw the cursor rects
        float resizeBoxWidth = 13;
        float resizeBoxInlet = 1;   

        Rect rightSide = new Rect(drawBox.xMax - resizeBoxWidth - resizeBoxInlet, drawBox.yMin, resizeBoxWidth - resizeBoxInlet, drawBox.height);
        Rect leftSide = new Rect(drawBox.xMin + resizeBoxInlet, drawBox.yMin, resizeBoxWidth - resizeBoxInlet, drawBox.height);
        Rect topSide = new Rect(drawBox.xMin, drawBox.yMin + resizeBoxInlet, drawBox.width, resizeBoxWidth - resizeBoxInlet);
        Rect bottomSide = new Rect(drawBox.xMin, drawBox.yMax- resizeBoxWidth - resizeBoxInlet, drawBox.width, resizeBoxWidth - resizeBoxInlet);

        EditorGUIUtility.AddCursorRect(rightSide, MouseCursor.ResizeHorizontal);
        EditorGUIUtility.AddCursorRect(topSide, MouseCursor.ResizeVertical);
        EditorGUIUtility.AddCursorRect(leftSide, MouseCursor.ResizeHorizontal);
        EditorGUIUtility.AddCursorRect(bottomSide, MouseCursor.ResizeVertical);
    }
}
