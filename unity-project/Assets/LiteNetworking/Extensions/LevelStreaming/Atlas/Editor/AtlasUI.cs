using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Credits to http://gram.gs/gramlog/creating-node-based-editor-unity/
// for some of the original code / tutorial

public class AtlasUI : EditorWindow {

    public AtlasData atlasData;
    public List<AtlasDiscontinuousHook> discHooks;
    public List<AtlasLevelView> levels;

    private Vector2 offset = Vector2.zero;
    private Vector2 drag = Vector2.zero;

    private GUIStyle nodeStyle;
    public static AtlasUI i;

    // Use this for initialization
    private void OnEnable () {
        levels = new List<AtlasLevelView>();

        nodeStyle = new GUIStyle();
        nodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
        nodeStyle.border = new RectOffset(12, 12, 12, 12);
        i = this;
    }

    [MenuItem("Networking/Extensions/Atlas Window")]
    private static void OpenWindow()
    {
        AtlasUI window = GetWindow<AtlasUI>();
        window.titleContent = new GUIContent("World Atlas");
        i = window;
    }

    /* Data Persistence */
    private static void SaveData()
    {

    }

    private static void LoadData()
    {

    }

    // Update is called once per frame
    void OnGUI () {
        // Event Processing
        ProcessNodeEvents(Event.current);
        ProcessEvents(Event.current);

        // Background Draws
        DrawGrids();

        // Node Draws
        DrawWorlds();

        // Connection Draws

        DrawDebug();
        if (GUI.changed) Repaint();
    }
    /* Public API Functions */

    /* Drawing */

    public void DrawDebug()
    {
    }

    public void DrawGrids()
    {
        DrawGrid(20, 0.2f, Color.gray);
        DrawGrid(100, 0.6f, Color.gray);

    }

    private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {

        int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
        int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        offset += drag * 0.5f;
        Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

        for (int i = 0; i < widthDivs; i++)
        {
            Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
        }

        for (int j = 0; j < heightDivs; j++)
        {
            Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
        }

        Handles.color = Color.white;
        Handles.EndGUI();
    }

    public void DrawDiscontinuousHooks()
    {
        foreach(AtlasDiscontinuousHook hook in discHooks)
        {

        }
    }

    public void DrawWorlds()
    {
        foreach(AtlasLevelView level in levels)
        {
            level.Draw(offset);
        }
    }

    public void DrawBaseHooks()
    {

    }

    /* Adder Fns */
    public void AddNode(Vector2 pos, string name = "")
    {
        if (levels == null) levels = new List<AtlasLevelView>();
        AtlasLevelView level = new AtlasLevelView(pos-offset, 100, 100, nodeStyle);
        if(name.Length > 0)
        {
            level.text = name;
        }
        

        levels.Add(level);
        Debug.Log("Add node success");
    }

    /* Event Handling */
    public void ProcessNodeEvents(Event e)
    {
        // Process them in reverse (because the first level is actually the farthest back
        for (int i = levels.Count - 1; i >= 0; i--)
        {
            if (levels[i].ProcessEvents(e, offset)) GUI.changed = true;
        }
    }

    public void ProcessEvents(Event e)
    {
        drag = Vector2.zero;

        switch (e.type)
        {
            case EventType.MouseUp:

                if (e.button == 0)
                {
                    OnClick(e.mousePosition);
                }

                break;
            case EventType.MouseDown:
                if(e.button == 0)
                {
                    OnMouseDown(e.mousePosition);
                }

                if (e.button == 1)
                {
                    OnOpenContextMenu(e.mousePosition);
                }
                break;

            case EventType.MouseDrag:
                if (e.button == 0)
                {
                    OnDrag(e.delta, e.mousePosition);
                }
                break;

            case EventType.DragUpdated:
                DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                break;
            case EventType.DragPerform:
                AddNode(e.mousePosition, DragAndDrop.paths[0]);
                break;
        }

        GUI.changed = true;
    }

    public void OnMouseDown(Vector2 pos)
    {

    }

    public void OnDrag(Vector2 delta, Vector2 pos)
    {
        
        drag = delta;
    }

    public void OnClick(Vector2 position)
    {

    }

    public void OnOpenContextMenu(Vector2 position)
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Add node"), false, () => AddNode(position));
        genericMenu.ShowAsContext();
    }
}
