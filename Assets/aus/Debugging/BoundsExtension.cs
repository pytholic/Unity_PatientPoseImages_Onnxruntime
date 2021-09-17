using UnityEngine;

namespace aus.Debugging
{
    public static class BoundsExtension
    {
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void DrawDebugBox(this Bounds b, Color color, float duration = 0)
        {
            var p = new Vector3[]
            {
                b.center + new Vector3(-b.extents.x, -b.extents.y, -b.extents.z), // min
                b.center + new Vector3(-b.extents.x, -b.extents.y, b.extents.z),
                b.center + new Vector3(b.extents.x, -b.extents.y, b.extents.z),
                b.center + new Vector3(b.extents.x, -b.extents.y, -b.extents.z),
                b.center + new Vector3(-b.extents.x, b.extents.y, -b.extents.z),
                b.center + new Vector3(-b.extents.x, b.extents.y, b.extents.z),
                b.center + new Vector3(b.extents.x, b.extents.y, b.extents.z), // max
                b.center + new Vector3(b.extents.x, b.extents.y, -b.extents.z),
            };

            Debug.DrawLine(p[0], p[1], color, duration);
            Debug.DrawLine(p[1], p[2], color, duration);
            Debug.DrawLine(p[2], p[3], color, duration);
            Debug.DrawLine(p[3], p[0], color, duration);

            Debug.DrawLine(p[4], p[5], color, duration);
            Debug.DrawLine(p[5], p[6], color, duration);
            Debug.DrawLine(p[6], p[7], color, duration);
            Debug.DrawLine(p[7], p[4], color, duration);

            Debug.DrawLine(p[0], p[4], color, duration);
            Debug.DrawLine(p[1], p[5], color, duration);
            Debug.DrawLine(p[2], p[6], color, duration);
            Debug.DrawLine(p[3], p[7], color, duration);
        }
    }
}
