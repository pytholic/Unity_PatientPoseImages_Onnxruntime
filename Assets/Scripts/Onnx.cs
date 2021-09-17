using aus.Extension;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class Onnx
{
    public static (int channel, int height, int width) GetImageShape(NodeMetadata sessionMeta)
    {
        Debug.Assert(sessionMeta.Dimensions.Length == 4);
        return (sessionMeta.Dimensions[1], sessionMeta.Dimensions[2], sessionMeta.Dimensions[3]);
    }

    public static DenseTensor<float> CreateTextureTensor(Texture2D input, int channel = 3)
    {
        Debug.Assert(channel == 3 || channel == 1); // bgr or graysacle channel only
        var tensor = new DenseTensor<float>(new int[] { 1, channel, input.height, input.width });
        for (var x = 0; x < input.width; ++x)
        {
            for (var y = 0; y < input.height; ++y)
            {
                var p = input.GetPixel(x, input.height - y); // y flipped coordination for unity texture
                if (channel == 1)
                { // making grayscale
                    tensor[0, 0, y, x] = (p.b + p.g + p.r) / 3;
                }
                else
                {
                    tensor[0, 0, y, x] = p.b;
                    tensor[0, 1, y, x] = p.g;
                    tensor[0, 2, y, x] = p.r;
                }
            }
        }
        return tensor;
    }

    public static List<Vector2Int> Predict2DPositions(InferenceSession session, Texture2D input)
    {
        var inputMeta = session.InputMetadata;
        Debug.Assert(inputMeta.Count == 1); // only 1 input supported
        var (c, h, w) = GetImageShape(inputMeta.First().Value);

        // preparing input texture
        var tex = input.Rescale(w, h);

        // setup input
        var inputs = new List<NamedOnnxValue>();
        inputs.Add(NamedOnnxValue.CreateFromTensor("input", CreateTextureTensor(tex, c)));

        // run and get results
        using var outputs = session.Run(inputs);
        var output = outputs.First().AsTensor<float>();
        Debug.Assert(output.Rank == 2); // only supports rank 2 results

        var results = new List<Vector2Int>();
        var arr = output.ToArray();
        for (var i = 0; i < arr.Length; i += 2)
        {
            var x = (int)(input.width * arr[i]);
            var y = (int)(input.height * (1 - arr[i + 1])); // flipped texture coordination
            results.Add(new Vector2Int(x, y));
        }
        return results;
    }

    public static List<Vector2Int> Predict2DPositions(string onnxFilePath, Texture2D input)
    {
        using var session = new InferenceSession(onnxFilePath);
        
        Debug.Log($"loaded model {onnxFilePath}");
        var inputMeta = session.InputMetadata;
        Debug.Assert(inputMeta.Count == 1); // only 1 input supported
        var (c, h, w) = GetImageShape(inputMeta.First().Value);
        Debug.Log($"input shape is ({c}, {h}, {w})");

        return Predict2DPositions(session, input);
    }

    public static void SaveTensor<T>(Tensor<T> t, string filePath)
    {
        var txt = t.GetArrayString();
        File.WriteAllText(filePath, txt);
    }
}