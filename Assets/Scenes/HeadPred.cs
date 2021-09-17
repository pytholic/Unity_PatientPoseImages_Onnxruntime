using aus.Geometry;
using System.Linq;
using UnityEngine;
using Unity.Barracuda;

public class HeadPred: MonoBehaviour
{
    public WavefrontObjMesh objLoader;
    public NNModel modelSource;

    private string[] labels = {"head_right", "head_left"}; 
    private string prediction;

    private IWorker worker;

    #region Unity message handlers

    private async void Start()
    {
        var objFilePath = Application.streamingAssetsPath + "/mesh/head_right_5.obj";
        await objLoader.LoadAsync(objFilePath);
        var mesh = objLoader.GetMeshFilter().mesh;

        // make camera see the mesh
        Camera.main.transform.position = mesh.bounds.center + Vector3.up * 0.7f;
        Camera.main.transform.LookAt(mesh.bounds.center);

        // as point cloud
        var pc = new PointCloud(mesh); // using octree by GetPointIndices or GetPoints
        var sample = pc.CreateSample(2048); // 20% points sample

        // Debug.Log($"Test sample shape: {input.Count}"); 
        
        foreach(var point in sample.Points){
            Debug.Log($"Point location: {point[0]}, {point[1]}, {point[2]}");
            break;
        }

        //Debug.Log($"Test sample shape: {input.Count}");    
        
        var model = ModelLoader.Load(modelSource);
        var worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, model);

        var input = pcd_to_tensor(sample);
        Debug.Log(input.shape);
        // Debug.Log(input.length);
        // Debug.Log(input[0]);
        // float[] temp = input.ToReadOnlyArray();

        //  foreach(var item in temp){
        //     Debug.Log(item);
        //     break;
        // }
        var output = worker.Execute(input).PeekOutput();

        float[] temp = output.ToReadOnlyArray();
        print("Network raw predictions:" + temp[0] + ", " + temp[1]);

        var index = output.ArgMax()[0];
        Debug.Log($"Image was recognised as class number: {index}");

        prediction = labels[index];
        print("Prediction is: " + prediction);

        input.Dispose();
        output.Dispose();
        worker.Dispose();


        // visualize
        //objLoader.gameObject.SetActive(false); // hide original mesh
        //CreateVisualObject(sample, Color.yellow);
        //var original = CreateVisualObject(pc, Color.green);
        //original.transform.Translate(Vector3.left);
    }

    #endregion Unity message handlers

    public static Tensor pcd_to_tensor(PointCloud pcd)
    {
        float[] floatValues = new float[1 * pcd.Count * 3];
        int i = 0;
        foreach(var point in pcd.Points)
        {
            if(i < pcd.Count * 3)
            {
                floatValues[i * 3 + 0] = (float)point[0];
                floatValues[i * 3 + 1] = (float)point[1];
                floatValues[i * 3 + 2] = (float)point[2];
            }
            i++;
        }
        //Debug.Log(floatValues.Min() + ", " + floatValues.Max());
        return new Tensor(1, 1, pcd.Count, 3, floatValues);
    }
    
    
    // public static Tensor pcd_to_tensor(PointCloud pcd)
    // {
    //     float[] floatValues = new float[1 * pcd.Count * 3];

    //     for (int i = 0; i < pcd.Count; ++i)
    //     {
    //         var point = pcd.Points[i];

    //         floatValues[i * 3 + 0] = (float)point[0];
    //         floatValues[i * 3 + 1] = (float)point[1];
    //         floatValues[i * 3 + 2] = (float)point[2];
    //     }
    //     //Debug.Log(floatValues.Min() + ", " + floatValues.Max());
    //     return new Tensor(1, 1, pcd.Count, 3, floatValues);
    // }


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