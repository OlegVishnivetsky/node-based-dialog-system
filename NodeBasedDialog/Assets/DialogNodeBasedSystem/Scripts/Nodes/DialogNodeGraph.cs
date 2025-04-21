using System.Collections.Generic;
using UnityEngine;

namespace cherrydev
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Nodes/Node Graph", fileName = "New Node Graph")]
    public class DialogNodeGraph : ScriptableObject
    {
        public List<Node> NodesList = new();

#if UNITY_EDITOR

        [HideInInspector] public Node NodeToDrawLineFrom = null;
        [HideInInspector] public Vector2 LinePosition = Vector2.zero;

        public static bool ShowLocalizationKeys { get; set; }
        
        /// <summary>
        /// Assigning values to nodeToDrawLineFrom and linePosition fields
        /// </summary>
        /// <param name="nodeToDrawLineFrom"></param>
        /// <param name="linePosition"></param>
        public void SetNodeToDrawLineFromAndLinePosition(Node nodeToDrawLineFrom, Vector2 linePosition)
        {
            NodeToDrawLineFrom = nodeToDrawLineFrom;
            LinePosition = linePosition;
        }

        /// <summary>
        /// Draging all selected nodes
        /// </summary>
        /// <param name="delta"></param>
        public void DragAllSelectedNodes(Vector2 delta)
        {
            foreach (Node node in NodesList)
            {
                if (node.IsSelected)
                    node.DragNode(delta);
            }
        }

        /// <summary>
        /// Returning amount of selected nodes
        /// </summary>
        /// <returns></returns>
        public int GetAmountOfSelectedNodes()
        {
            int amount = 0;

            foreach (Node node in NodesList)
            {
                if (node.IsSelected)
                    amount++;
            }

            return amount;
        }

#endif
    }
}