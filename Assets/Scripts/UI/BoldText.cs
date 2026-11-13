using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>
    /// Makes pixel text thicker and easier to read: draws every letter a second time, a hair to the right,
    /// in the text's own colour (so it keeps working when the colour changes).
    /// </summary>
    public class BoldText : BaseMeshEffect
    {
        public float offset = 0.5f;      // in canvas units: 1 screen pixel at the usual 2x scale

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || vh.currentVertCount == 0) return;
            var verts = new List<UIVertex>();
            vh.GetUIVertexStream(verts);
            int count = verts.Count;
            var output = new List<UIVertex>(count * 2);
            for (int i = 0; i < count; i++)
            {
                var v = verts[i];
                v.position += new Vector3(offset, 0, 0);
                output.Add(v);
            }
            output.AddRange(verts);
            vh.Clear();
            vh.AddUIVertexTriangleStream(output);
        }
    }
}
