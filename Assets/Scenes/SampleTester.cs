using aus.Geometry;
using System.Linq;
using UnityEngine;

public class SampleTester
    : MonoBehaviour
{
    public WavefrontObjMesh objLoader;

    #region Unity message handlers

    private async void Start()
    {
        var objFilePath = Application.streamingAssetsPath + "/mesh/HeadSample1.obj";
        await objLoader.LoadAsync(objFilePath);
        var mesh = objLoader.GetMeshFilter().mesh;

        // make camera see the mesh
        Camera.main.transform.position = mesh.bounds.center + Vector3.up * 0.7f;
        Camera.main.transform.LookAt(mesh.bounds.center);

        // as point cloud
        var pc = new PointCloud(mesh); // using octree by GetPointIndices or GetPoints
        var sample = pc.CreateSample(pc.Count / 5); // 20% points sample

        // visualize
        objLoader.gameObject.SetActive(false); // hide original mesh
        CreateVisualObject(sample, Color.yellow);
        var original = CreateVisualObject(pc, Color.green);
        original.transform.Translate(Vector3.left * 0.4f);
    }

    #endregion Unity message handlers

    private GameObject CreateVisualObject(PointCloud source, Color pointColor)
    {
        var visual = new GameObject("point cloud");
        var meshFilter = visual.AddComponent<MeshFilter>();
        meshFilter.mesh.vertices = source.Points;
        if (source.Normals != null) meshFilter.mesh.normals = source.Normals;
        meshFilter.mesh.SetIndices(Enumerable.Range(0, source.Count).ToArray(), MeshTopology.Points, 0); // as a point
        var meshRenderer = visual.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Particles/Standard Unlit")); // this won't work out of editor
        meshRenderer.material.color = pointColor;
        return visual;
    }
}