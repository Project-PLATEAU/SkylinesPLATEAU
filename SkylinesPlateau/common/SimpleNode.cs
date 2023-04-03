using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SkylinesPlateau
{
    public class SimpleNode
    {
        const int trimValue = 6;

        public ushort nodeId;
        public float[] nodeCoords;

        public SimpleNode(ushort node, Vector3 pos)
        {
            nodeId = node;
            nodeCoords = new float[2] { pos.x, pos.z };
        }

        /// <summary>
        /// 同じ座標位置の頂点が存在するか確認する
        /// </summary>
        static public bool FindNode(Dictionary<short, List<SimpleNode>> nodeMap, out ushort netNodeId, Vector2 nodePoint)
        {
            short xRound = (short)Math.Round(nodePoint.x);

            if (nodeMap.ContainsKey(xRound))
            {
                foreach (SimpleNode node in nodeMap[xRound])
                {
                    if (node.nodeCoords[0] > nodePoint.x - trimValue &&
                        node.nodeCoords[0] < nodePoint.x + trimValue)
                    {
                        if (node.nodeCoords[1] > nodePoint.y - trimValue &&
                            node.nodeCoords[1] < nodePoint.y + trimValue)
                        {
                            netNodeId = node.nodeId;
                            return true;
                        }
                    }
                }
            }

            netNodeId = 0;
            return false;
        }
    }
}
