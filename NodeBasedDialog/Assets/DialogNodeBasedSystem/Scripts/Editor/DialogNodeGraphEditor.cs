using UnityEditor;
using UnityEngine;

namespace cherrydev
{
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
                NodeEditor.OnDoubleClickAsset(nodeGraph.GetInstanceID(), -1);
            }
        }
    }
}