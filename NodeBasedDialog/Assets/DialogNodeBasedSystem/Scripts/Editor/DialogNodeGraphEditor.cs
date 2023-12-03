using cherrydev;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DialogNodeGraph))]
public class DialogNodeGraphEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        DialogNodeGraph nodeGraph = (DialogNodeGraph)target;

        if (GUILayout.Button("Open Editor Window"))
        {
            NodeEditor.SetCurrentNodeGraph(nodeGraph);
            NodeEditor.OpenWindow();
        }
    }
}