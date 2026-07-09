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
#if UNITY_6000_3_OR_NEWER
                NodeEditor.OnDoubleClickAsset(nodeGraph.GetEntityId(), -1);
#else
                NodeEditor.OnDoubleClickAsset(nodeGraph.GetInstanceID(), -1);
#endif
            }
        }
    }
}