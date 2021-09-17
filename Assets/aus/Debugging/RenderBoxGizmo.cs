// https://bitbucket.org/alkee/aus

using UnityEngine;

namespace aus.Debugging
{
    [ExecuteInEditMode]
    public class RenderBoxGizmo : MonoBehaviour
    {
        public Color BoxColor = new Color(0.5f, 1.0f, 0.5f, 0.4f);

        // TODO: make this readonly
        public Bounds BoxBounds;

        private Bounds renderBox;
        private Renderer r;

        private void Start()
        {
            r = GetComponent<Renderer>();
            if (r == null) r = GetComponentInChildren<Renderer>();
            if (r == null) enabled = false;
        }

        private void Update()
        {
            renderBox = r.bounds;
            BoxBounds = renderBox;
        }

        private void OnDrawGizmosSelected()
        {
            if (enabled == false) return;

            Gizmos.color = BoxColor;
            Gizmos.DrawCube(renderBox.center, renderBox.size);
        }
    }

}
