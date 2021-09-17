// https://bitbucket.org/alkee/aus

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;

namespace aus.Extension
{
    public static class UnityExt
    {
        #region serialize / deserialize

        // SerializationException: Type 'UnityEngine.Vector3Int' in Assembly 'UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' is not marked as serializable.
        //  와 같은 문제로 Unity 에서 사용하는 type 들이 serialize 되지 않는 문제를 피하기 위해

        public static void Serialize(this Texture3D texture, IFormatter formatter, Stream stream)
        {
            var t = texture;
            var dim = new Vector3Int(t.width, t.height, t.depth);
            dim.Serialize(formatter, stream);
            formatter.Serialize(stream, t.format);
            formatter.Serialize(stream, t.mipmapCount);
            t.GetPixels().Serialzie(formatter, stream);
        }

        public static Texture3D DeserializeTexture3D(IFormatter formatter, Stream stream)
        {
            var dim = DeserializeVector3Int(formatter, stream);
            var format = (TextureFormat)formatter.Deserialize(stream);
            var mipmapCount = (int)formatter.Deserialize(stream);
            var pixels = DeserialzieColorArray(formatter, stream);

            var t = new Texture3D(dim.x, dim.y, dim.z, format, mipmapCount);
#pragma warning disable UNT0017 // SetPixels invocation is slow
            t.SetPixels(pixels);
#pragma warning restore UNT0017 // SetPixels invocation is slow
            return t;
        }

        //
        public static void Serialize(this Vector3Int target, IFormatter formatter, Stream stream)
        {
            var x = target.x;
            var y = target.y;
            var z = target.z;
            formatter.Serialize(stream, x);
            formatter.Serialize(stream, y);
            formatter.Serialize(stream, z);
        }

        public static Vector3Int DeserializeVector3Int(IFormatter formatter, Stream stream)
        {
            var x = (int)formatter.Deserialize(stream);
            var y = (int)formatter.Deserialize(stream);
            var z = (int)formatter.Deserialize(stream);
            return new Vector3Int(x, y, z);
        }

        public static void Serialize(this Vector3 target, IFormatter formatter, Stream stream)
        {
            var x = target.x;
            var y = target.y;
            var z = target.z;
            formatter.Serialize(stream, x);
            formatter.Serialize(stream, y);
            formatter.Serialize(stream, z);
        }

        public static Vector3 DeserializeVector3(IFormatter formatter, Stream stream)
        {
            var x = (float)formatter.Deserialize(stream);
            var y = (float)formatter.Deserialize(stream);
            var z = (float)formatter.Deserialize(stream);
            return new Vector3(x, y, z);
        }

        public static void Serialzie(this Color[] target, IFormatter formatter, Stream stream)
        {
            var arr = new float[4 * target.Length];
            var arrIndex = 0;
            for (var i = 0; i < target.Length; ++i)
            {
                var c = target[i];
                arr[arrIndex++] = c[0];
                arr[arrIndex++] = c[1];
                arr[arrIndex++] = c[2];
                arr[arrIndex++] = c[3];
            }
            formatter.Serialize(stream, arr);
        }

        public static Color[] DeserialzieColorArray(IFormatter formatter, Stream stream)
        {
            var arr = (float[])formatter.Deserialize(stream);
            var colors = new Color[arr.Length / 4];
            for (var i = 0; i < colors.Length; ++i)
            {
                colors[i] = new Color(arr[i * 4], arr[i * 4 + 1], arr[i * 4 + 2], arr[i * 4 + 3]);
            }
            return colors;
        }

        #endregion serialize / deserialize

        #region Vectors

        public static Vector3 InverseScale(this Vector3 v)
        {
            return new Vector3(1.0f / v.x, 1.0f / v.y, 1.0f / v.z);
        }

        public static Vector3 DivideBy(this Vector3 v, Vector3 src)
        {
            return new Vector3(v.x / src.x, v.y / src.y, v.z / src.z);
        }

        public static Vector3 MultiplyBy(this Vector3 v, Vector3 src)
        {
            return new Vector3(v.x * src.x, v.y * src.y, v.z * src.z);
        }

        public static int[] ToArray(this Vector3Int v)
        {
            return new int[3] { v.x, v.y, v.z };
        }

        public static float[] ToArray(this Vector3 v)
        {
            return new float[3] { v.x, v.y, v.z };
        }

        public static bool Any0(this Vector3 v)
        {
            return v.x == 0 || v.y == 0 || v.z == 0;
        }

        public static bool Any0(this Vector3Int v)
        {
            return v.x == 0 || v.y == 0 || v.z == 0;
        }

        public static bool IsInCube(this Vector3 v, Vector3 cubeCenter, Vector3 cubeSize)
        {
            var half = cubeSize * 0.5f;
            if (v.x < cubeCenter.x - half.x || v.x > cubeCenter.x + half.x) return false;
            if (v.y < cubeCenter.y - half.y || v.y > cubeCenter.y + half.y) return false;
            if (v.z < cubeCenter.z - half.z || v.z > cubeCenter.z + half.z) return false;
            return true;
        }

        public static Vector2 To2D(this Vector3 v, Vector3 angle)
        {
            return new Vector2(v.x * Mathf.Cos(angle.z * Mathf.Deg2Rad) - v.y * Mathf.Sin(angle.z * Mathf.Deg2Rad),
                v.z * Mathf.Cos(angle.x * Mathf.Deg2Rad) - v.y * Mathf.Sin(angle.x * Mathf.Deg2Rad));
        }

        public static Vector2Int To2D(this Vector3Int v, Vector3 angle)
        {
            return new Vector2Int(Mathf.RoundToInt(v.x * Mathf.Cos(angle.z * Mathf.Deg2Rad) - v.y * Mathf.Sin(angle.z * Mathf.Deg2Rad)),
                Mathf.RoundToInt(v.z * Mathf.Cos(angle.x * Mathf.Deg2Rad) - v.y * Mathf.Sin(angle.x * Mathf.Deg2Rad)));
        }

        public static Vector2Int Abs(this Vector2Int v)
        {
            return new Vector2Int(Math.Abs(v.x), Math.Abs(v.y));
        }

        public static Vector2 Abs(this Vector2 v)
        {
            return new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));
        }

        public static Vector3 GetMeanVector(this IEnumerable<Vector3> points)
        {
            var count = points.Count();
            if (count == 0) throw new ArgumentException($"one or more points needed");
            var sum = Vector3.zero;
            foreach (var p in points) sum += p;
            return sum / count;
        }

        public static Vector2 GetMeanVector(this IEnumerable<Vector2> points)
        {
            var count = points.Count();
            if (count == 0) throw new ArgumentException($"one or more points needed");
            var sum = Vector2.zero;
            foreach (var p in points) sum += p;
            return sum / count;
        }

        public static Vector3Int GetMeanVector(this IEnumerable<Vector3Int> points)
        {
            var count = points.Count();
            if (count == 0) throw new ArgumentException($"one or more points needed");
            var sum = Vector3Int.zero;
            foreach (var p in points) sum += p;
            return sum / count;
        }

        public static Vector2Int GetMeanVector(this IEnumerable<Vector2Int> points)
        {
            var count = points.Count();
            if (count == 0) throw new ArgumentException($"one or more points needed");
            var sum = Vector2Int.zero;
            foreach (var p in points) sum += p;
            return sum / count;
        }

        #endregion Vectors

        #region Textures

        public static void FillEllipsoid(this Texture3D tex, Color color, Vector3Int pos, Vector3Int radius)
        { // x*x/a*a + y*y/b*b + z*z/c*c = 1 --> b*b*c*c*x*x + a*a*c*c*y*y + a*a*b*b*z*z = a*a*b*b*c*c
            var rxSqr = radius.x * radius.x;
            var rySqr = radius.y * radius.y;
            var rzSqr = radius.z * radius.z;
            var rSqr = rxSqr * rySqr * rzSqr;

            for (var x = pos.x - radius.x; x < pos.x + radius.x + 1; ++x)
            {
                if (x < 0 || x >= tex.width) continue;
                var xSqr = (pos.x - x) * (pos.x - x);
                for (var y = pos.y - radius.y; y < pos.y + radius.y + 1; ++y)
                {
                    if (y < 0 || y >= tex.height) continue;
                    var ySqr = (pos.y - y) * (pos.y - y);
                    for (var z = pos.z - radius.z; z < pos.z + radius.z + 1; ++z)
                    {
                        if (z < 0 || z >= tex.depth) continue;
                        var zSqr = (pos.z - z) * (pos.z - z);
                        if (xSqr * rySqr * rzSqr + ySqr * rxSqr * rzSqr + zSqr * rxSqr * rySqr < rSqr)
                            tex.SetPixel(x, y, z, color);
                    }
                }
            }
        }

        public static void FillEllipse(this Texture2D tex, Color color, Vector2Int pos, Vector2Int radius)
        { // x*x/a*a + y*y/b*b = 1   -->  b*b*x*x + a*a*y*y = a*a*b*b
            var rxSqr = radius.x * radius.x;
            var rySqr = radius.y * radius.y;
            var rSqr = rxSqr * rySqr;

            for (var x = pos.x - radius.x; x < pos.x + radius.x + 1; x++)
            {
                if (x < 0 || x >= tex.width) continue;
                var xSqr = (pos.x - x) * (pos.x - x);
                for (var y = pos.y - radius.y; y < pos.y + radius.y + 1; y++)
                {
                    if (y < 0 || y >= tex.height) continue;
                    var ySqr = (pos.y - y) * (pos.y - y);
                    if (xSqr * rySqr + ySqr * rxSqr < rSqr)
                        tex.SetPixel(x, y, color);
                }
            }
        }

        public static void FillColor(this Texture2D tex, Color color)
        {
            Color32 c = color;
            var pixels = Enumerable.Repeat(c, tex.width * tex.height).ToArray();
            tex.SetPixels32(pixels);
        }

        public static Ray PointToRay(this Camera cam, int textureWidth, int textureHeight, Vector2Int pos)
        {
            var normalizedPoint = new Vector2(pos.x / (float)textureWidth, pos.y / (float)textureHeight);
            var backup = cam.targetTexture;
            var tmp = RenderTexture.GetTemporary(textureWidth, textureHeight);
            using (new Defer(() => { cam.targetTexture = backup; RenderTexture.ReleaseTemporary(tmp); }))
            {
                // targetTexture 를 texture 에 맞게 지정하지 않으면,
                //   screen ratio 에 따라 살짝 다른 위치(x좌표)의 ray 를 반환한다(#398 ; https://bitbucket.org/alkee_skia/mars-processor/issues/398/nose-detection#comment-60924133)
                cam.targetTexture = tmp;
                return cam.ViewportPointToRay(normalizedPoint);
            }
        }

        public static Ray PointToRay(this Texture2D tex, Camera source, Vector2Int pos)
        {
            return PointToRay(source, tex.width, tex.height, pos);
        }

        public static Texture2D Rescale(this Texture2D tex, int width, int height)
        { // ref: http://jon-martin.com/?p=114
            var result = new Texture2D(width, height, tex.format, false);
            var pixels = result.GetPixels(0);
            var incX = 1.0f / width;
            var incY = 1.0f / height;
            for (var px = 0; px < pixels.Length; px++)
            {
                pixels[px] = tex.GetPixelBilinear(incX * ((float)px % width), incY * ((float)Mathf.Floor(px / width)));
            }
#pragma warning disable UNT0017 // SetPixels invocation is slow
            result.SetPixels(pixels, 0);
#pragma warning restore UNT0017 // SetPixels invocation is slow
            result.Apply();
            return result;
        }

        #endregion Textures

        #region Mesh

        public static Mesh RecoordinateAndCombine(this IEnumerable<Mesh> meshes, Transform sourcetransform)
        {
            var count = meshes.Count();
            CombineInstance[] combine = new CombineInstance[count];

            var i = 0;
            foreach (var m in meshes)
            {
                combine[i].mesh = m;
                combine[i].transform = sourcetransform.worldToLocalMatrix;
                ++i;
            }

            var mesh = new Mesh();
            mesh.CombineMeshes(combine);
            return mesh;
        }

        #endregion Mesh

        #region GameObject

        public static int DestoryAllChidren(this GameObject g)
        {
            var count = 0;
            foreach (Transform t in g.transform)
            {
                UnityEngine.Object.Destroy(t.gameObject);
                ++count;
            }
            return count;
        }

        #endregion GameObject

        #region Monobehaviour

        public static void ActionInNextFrame(this MonoBehaviour m, System.Action action)
        {
            m.StartCoroutine(coroutine());

            IEnumerator coroutine()
            {
                yield return null;
                yield return new WaitForEndOfFrame();
                action();
            }
        }

        public static T CopyTo<T>(this T src, GameObject dest) where T : UnityEngine.Component
        { // ref: https://gist.github.com/mattatz/1c039dce5ef251dcef7b628c878bea32
            var type = src.GetType();

            var dst = dest.GetComponent(type) as T;
            if (!dst) dst = dest.AddComponent(type) as T;

            var fields = GetAllFields(type);
            foreach (var field in fields)
            {
                if (field.IsStatic) continue;
                field.SetValue(dst, field.GetValue(src));
            }

            var props = type.GetProperties();
            foreach (var prop in props)
            {
                if (!prop.CanWrite || !prop.CanWrite || prop.Name == "name") continue;
                prop.SetValue(dst, prop.GetValue(src, null), null);
            }

            return dst;
        }

        private static IEnumerable<FieldInfo> GetAllFields(Type t)
        {
            if (t == null) return Enumerable.Empty<FieldInfo>();
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                                 BindingFlags.Static | BindingFlags.Instance |
                                 BindingFlags.DeclaredOnly;
            return t.GetFields(flags).Concat(GetAllFields(t.BaseType));
        }

        #endregion Monobehaviour

        #region Camera

        public static Texture2D ScreenShot(this Camera source, int textureSize = 512)
        {
            var backupTarget = source.targetTexture;
            var screenShot = RenderTexture.GetTemporary(textureSize, textureSize);
            using (new Defer(() =>
            {
                source.targetTexture = backupTarget;
                RenderTexture.ReleaseTemporary(screenShot);
            }))
            {
                // render to RenderTexture
                source.targetTexture = screenShot;

                source.ResetAspect();
                source.Render();

                // RenderTexture to Texture2D
                var saveTarget = new Texture2D(screenShot.width, screenShot.height, TextureFormat.RGBA32, false);
                var backupActive = RenderTexture.active;
                RenderTexture.active = screenShot;
                saveTarget.ReadPixels(new Rect(0, 0, screenShot.width, screenShot.height), 0, 0); // read from RenderTexture.active
                saveTarget.Apply();
                RenderTexture.active = backupActive;
                return saveTarget;
            }
        }

        #endregion Camera

        #region ETC

        // https://gist.github.com/mattyellen/d63f1f557d08f7254345bff77bfdc8b3
        public static System.Runtime.CompilerServices.TaskAwaiter GetAwaiter(this AsyncOperation asyncOp)
        {
            // TODO: cancel 가능하도록. 참고: UniTask ; https://github.com/Cysharp/UniTask#cancellation-and-exception-handling
            var tcs = new System.Threading.Tasks.TaskCompletionSource<object>();
            asyncOp.completed += obj => { tcs.SetResult(null); }; // event 에서 제거하는 코드가 없는데 괜찮을까???
            return ((System.Threading.Tasks.Task)tcs.Task).GetAwaiter();
            /* Example:
            var getRequest = UnityWebRequest.Get("http://www.google.com");
            await getRequest.SendWebRequest();
            var result = getRequest.downloadHandler.text;
            */
        }

        public static BoundsInt Encapsulate(BoundsInt a, BoundsInt b)
        {
            // https://docs.unity3d.com/ScriptReference/Bounds.Encapsulate.html
            // nullable에도 사용할 수 있도록 this를 사용하지 않음.
            a.xMin = Math.Min(a.xMin, b.xMin);
            a.yMin = Math.Min(a.yMin, b.yMin);
            a.zMin = Math.Min(a.zMin, b.zMin);
            a.xMax = Math.Max(a.xMax, b.xMax);
            a.yMax = Math.Max(a.yMax, b.yMax);
            a.zMax = Math.Max(a.zMax, b.zMax);
            return a;
        }

        public static BoundsInt Encapsulate(BoundsInt bounds, Vector3Int point)
        // bounds를 this로 선언했지만 struct라 함수내부에서 바뀐점들이 반영되지 않는 것을 확인.
        // bounds를 this에다가 ref로 선언했지만 Property는 ref로 전달할 수 없었음.
        // 비직관적이지만 반환 값에 복사하는 방식을 사용.
        {
            var minVector = bounds.min;
            var maxVector = bounds.max;
            bounds.min =
                new Vector3Int(Math.Min(minVector.x, point.x), Math.Min(minVector.y, point.y), Math.Min(minVector.z, point.z));
            bounds.max = // Bounds를 min 이상, max 미만 개념으로 사용하기 위해 max를 1을 더한 값과 비교.
                new Vector3Int(Math.Max(maxVector.x, point.x + 1), Math.Max(maxVector.y, point.y + 1), Math.Max(maxVector.z, point.z + 1));
            return bounds;
        }

        public static float DiagonalLength(this Bounds bounds)
        {
            return (bounds.max - bounds.min).magnitude;
        }

        public static void ForEach(this BoundsInt bounds, Action<int, int, int> pred)
        {
            var minX = bounds.min.x;
            var minY = bounds.min.y;
            var minZ = bounds.min.z;
            var maxX = bounds.max.x;
            var maxY = bounds.max.y;
            var maxZ = bounds.max.z;
            for (var z = minZ; z < maxZ; ++z)
                for (var y = minY; y < maxY; ++y)
                    for (var x = minX; x < maxX; ++x)
                        pred(x, y, z);
        }

        #endregion ETC
    }
}