using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SkylinesPlateau
{
    public class InteriorPoints
    {
        // 形状ポリゴン (穴あき)
        public List<Vector3> points = new List<Vector3>();
        // 道路ポリゴンの範囲
        public Vector3 areaMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        public Vector3 areaMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
    }
}
