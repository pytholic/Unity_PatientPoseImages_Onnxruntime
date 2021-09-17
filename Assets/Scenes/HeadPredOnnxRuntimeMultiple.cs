using System.Collections;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using UnityEngine;
using aus.Extension;
using aus.Geometry;
using System.Linq;

public class HeadPredOnnxRuntimeMultiple : MonoBehaviour
{
    // Start is called before the first frame update
    public WavefrontObjMesh objLoader;
    //string modelPath = @"/home/trojan/Downloads/pointnet_classification.onnx";
    string modelPath = Application.streamingAssetsPath + "/model/pointnet_classification.onnx";
    
    private string[] labels = {"head_left", "head_right"}; 
    private string prediction;

    public string path = Application.streamingAssetsPath + "/mesh";
    public string[] object_list;
    // Start is called before the first frame update
    private async void Start()
    {
        object_list = System.IO.Directory.GetFiles(@path, "*.obj");
        int cnt = object_list.Length;
        Debug.Log($"Total input objects: {cnt}");
        //Debug.Log(object_list[0]);

        for (int i=0; i<cnt; i++)
        {
            # region Loading input mesh
            // Load the mesh object
            var objFilePath = object_list[i];
            await objLoader.LoadAsync(objFilePath);
            var mesh = objLoader.GetMeshFilter().mesh;

            // make camera see the mesh
            Camera.main.transform.position = mesh.bounds.center + Vector3.up * 0.7f;
            Camera.main.transform.LookAt(mesh.bounds.center);

            // Sample as point cloud
            var pc = new PointCloud(mesh); // using octree by GetPointIndices or GetPoints
            var sample = pc.CreateSample(2048); // 20% points sample

            // Convert to tensor
            var input = pcd_to_tensor(sample);
            float[] inputData = input.ToArray();

            # endregion

        //Debug.Log(input_list.Count);
            
            # region Inference session
            // Optional : Create session options and set the graph optimization level for the session
            SessionOptions options = new SessionOptions();
            options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_EXTENDED;
    
            using (var session = new InferenceSession(modelPath, options))
            {
                var inputMeta = session.InputMetadata;
                var (n, w, c) = GetImageShape(inputMeta.First().Value);
                //Debug.Log($"input shape is ({n}, {w}, {c})"); // should return (-1, 2048, 3) for my input
                var container = new List<NamedOnnxValue>();                

                foreach (var name in inputMeta.Keys)
                {
                    var tensor = new DenseTensor<float>(inputData, new int[] {1, 2048, 3});
                    container.Add(NamedOnnxValue.CreateFromTensor<float>(name, tensor));
                }
            
        
                // Run the inference
                using (var results = session.Run(container))  // results is an IDisposableReadOnlyCollection<DisposableNamedOnnxValue> container  
                {
                    // dump the results
                    foreach(var r in results)
                    {
                        //Debug.Log($"Output for {r.Name}");
                        //Debug.Log(r.AsTensor<float>().GetArrayString());

                        // get max index
                        var temp = r.AsTensor<float>().ToList();
                        var maxIndex = (int)temp.IndexOf(temp.Max());
                        //Debug.Log($"Argmax index = {maxIndex}");

                        // get prediction class
                        prediction = labels[maxIndex];
                        Debug.Log($"Network prediction for object {i} is: {prediction}");
                    }
                }
            }
            //Debug.Log(container.Count);
            # endregion 
        }
    }

    public static (int channel, int height, int width) GetImageShape(NodeMetadata sessionMeta)
    {
        Debug.Assert(sessionMeta.Dimensions.Length == 3);
        return (sessionMeta.Dimensions[0], sessionMeta.Dimensions[1], sessionMeta.Dimensions[2]);
    }
    
    public static DenseTensor<float> pcd_to_tensor(PointCloud pcd)
    {
        var tensor = new DenseTensor<float>(new int[] {1, pcd.Count, 1, 3});
        int i = 0;
        foreach(var point in pcd.Points)
        {
            if(i < pcd.Count)
            {
                tensor[0, i, 0, 0] = (float)point[0];
                tensor[0, i, 0, 1] = (float)point[1];
                tensor[0, i, 0, 2] = (float)point[2];
            }
            i++;
        }
        return tensor;

    }
}

