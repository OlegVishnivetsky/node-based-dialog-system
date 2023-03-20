using System.Collections.Generic;
using UnityEngine;

namespace cherrydev
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Nodes/Node Graph", fileName = "New Node Graph")]
    public class DialogNodeGraph : ScriptableObject
    {
        public List<Node> nodesList = new List<Node>();

#if UNITY_EDITOR

        [HideInInspector] public Node nodeToDrawLineFrom = null;
        [HideInInspector] public Vector2 linePosition = Vector2.zero;

        /// <summary>
        /// Assigning values to nodeToDrawLineFrom and linePosition fields
        /// </summary>
        /// <param name="nodeToDrawLineFrom"></param>
        /// <param name="linePosition"></param>
        public void SetNodeToDrawLineFromAndLinePosition(Node nodeToDrawLineFrom, Vector2 linePosition)
        {
            this.nodeToDrawLineFrom = nodeToDrawLineFrom;
            this.linePosition = linePosition;
        }

        /// <summary>
        /// Draging all selected nodes
        /// </summary>
        /// <param name="delta"></param>
        public void DragAllSelectedNodes(Vector2 delta)
        {
            foreach (var node in nodesList)
            {
                if (node.isSelected)
                {
                    node.DragNode(delta);
                }
            }
        }

        /// <summary>
        /// Returning amount of selected nodes
        /// </summary>
        /// <returns></returns>
        public int GetAmountOfSelectedNodes()
        {
            int amount = 0;

            foreach (Node node in nodesList)
            {
                if (node.isSelected)
                {
                    amount++;
                }
            }

            return amount;
        }

#endif
    }
}