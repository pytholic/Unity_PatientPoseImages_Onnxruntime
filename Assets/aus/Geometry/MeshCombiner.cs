// https://bitbucket.org/alkee/aus
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace aus.Geometry
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    [DisallowMultipleComponent]
    public class MeshCombiner : MonoBehaviour
    { // ref: https://youtu.be/wYAlky1aZn4

        public bool MergeOnlyActvieChildren = false;

        public void CombineMeshes()
        {
            bool includeInactive = !MergeOnlyActvieChildren;
            var children = GetComponentsInChildren<MeshRenderer>(includeInactive);
            if (children.Length < 2) return; // nothing to do

            Debug.Log(name + " is combining " + (children.Length - 1).ToString() + " meshes");

            // preparing materials
            var materials = new List<Material>();
            foreach (var r in GetComponentsInChildren<MeshRenderer>(includeInactive))
            {
                if (r.transform == transform) continue; // GetComponentsInChildren includes itself
                foreach (var m in r.sharedMaterials)
                {
                    if (materials.Contains(m)) continue;
                    materials.Add(m);
                }
            }

            Debug.Log("  and " + materials.Count.ToString() + " materials");

            // creating submeshes by each materials
            using (new NormalizeAndRollbackTransform(transform))
            {
                var submeshes = new List<Mesh>();
                foreach (var m in materials)
                {
                    var subcombiners = new List<CombineInstance>();
                    foreach (var f in GetComponentsInChildren<MeshFilter>(includeInactive))
                    {
                        if (f.transform == transform) continue; // GetComponentsInChildren includes itself
                        var r = f.GetComponent<MeshRenderer>();
                        if (r == null)
                        {
                            Debug.LogWarning(f.name + " has no MeshRenderer");
                            continue;
                        }
                        else if (r.enabled == false)
                        {
                            Debug.LogWarning(f.name + " has disabled renderer. skipping");
                            continue;
                        }

                        for (var subMeshIndex = 0; subMeshIndex < r.sharedMaterials.Length; ++subMeshIndex)
                        {
                            if (m != r.sharedMaterials[subMeshIndex]) continue; // not the target
                            var c = new CombineInstance();
                            c.mesh = f.sharedMesh;
                            c.subMeshIndex = subMeshIndex;
                            c.transform = f.transform.localToWorldMatrix;
                            subcombiners.Add(c);
                        }
                    }
                    // flatten into a single mesh
                    if (subcombiners.Count == 0) continue; // not referenced material?
                    var submesh = new Mesh();
                    submesh.CombineMeshes(subcombiners.ToArray(), true);
                    submeshes.Add(submesh);
                }

                Debug.Log("  and " + submeshes.Count.ToString() + " submeshes");

                // preparing final mesh
                var finalCombiner = new List<CombineInstance>();
                foreach (var m in submeshes)
                {
                    var c = new CombineInstance();
                    c.mesh = m;
                    c.subMeshIndex = 0;
                    c.transform = Matrix4x4.identity;
                    finalCombiner.Add(c);
                }

                var finalMesh = new Mesh();
                finalMesh.CombineMeshes(finalCombiner.ToArray(), false);
                GetComponent<MeshFilter>().sharedMesh = finalMesh;
                GetComponent<MeshRenderer>().sharedMaterials = materials.ToArray();
            } // transform rollback
        }

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MeshCombiner))]
    public class MeshCombinerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var combiner = (target as MeshCombiner);

            if (GUILayout.Button("Combine/Refresh"))
            {
                combiner.CombineMeshes();
            }
        }
    }
#endif
}
