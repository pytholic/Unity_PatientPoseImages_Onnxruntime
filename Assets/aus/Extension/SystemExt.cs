// https://bitbucket.org/alkee/aus

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace aus.Extension
{
    public class Defer
        : IDisposable
    {
        private readonly System.Action work;

        public Defer(System.Action workAtDispose)
        {
            work = workAtDispose;
        }

        public void Dispose()
        {
            work();
        }
    }

    public static class SystemExt
    {
        public static string ToHumanTimeString(this TimeSpan span, int significantDigits = 3)
        { // https://www.extensionmethod.net/csharp/timespan/timespan-tohumantimestring
            var format = "G" + significantDigits;
            return span.TotalMilliseconds < 1000 ? span.TotalMilliseconds.ToString(format) + " milliseconds"
                : (span.TotalSeconds < 60 ? span.TotalSeconds.ToString(format) + " seconds"
                    : (span.TotalMinutes < 60 ? span.TotalMinutes.ToString(format) + " minutes"
                        : (span.TotalHours < 24 ? span.TotalHours.ToString(format) + " hours"
                                                : span.TotalDays.ToString(format) + " days")));
        }

        public static void Fill<T>(this T[] array, T value, int startIndex, int count)
        {
            if (array.Length < startIndex + count) throw new IndexOutOfRangeException();
            while (count-- > 0)
            {
                array[startIndex + count] = value;
            }
        }

        public static void Ellipsoid(this short[] src, Vector3Int dim, Vector3Int pos, Vector3Int radius, short val = 1)
        { // see UnityExt.FillEllipsoid
            Debug.Assert(src.Length == dim.x * dim.y * dim.z);

            var rxSqr = radius.x * radius.x;
            var rySqr = radius.y * radius.y;
            var rzSqr = radius.z * radius.z;
            var rSqr = rxSqr * rySqr * rzSqr;

            for (var x = pos.x - radius.x; x < pos.x + radius.x + 1; ++x)
            {
                if (x < 0 || x >= dim.x) continue;
                var xSqr = (pos.x - x) * (pos.x - x);
                for (var y = pos.y - radius.y; y < pos.y + radius.y + 1; ++y)
                {
                    if (y < 0 || y >= dim.y) continue;
                    var ySqr = (pos.y - y) * (pos.y - y);
                    for (var z = pos.z - radius.z; z < pos.z + radius.z + 1; ++z)
                    {
                        if (z < 0 || z >= dim.z) continue;
                        var zSqr = (pos.z - z) * (pos.z - z);
                        if (xSqr * rySqr * rzSqr + ySqr * rxSqr * rzSqr + zSqr * rxSqr * rySqr < rSqr)
                        {
                            src[x + y * dim.x + z * dim.x * dim.y] = val;
                        }
                    }
                }
            }
        }

        public static void FillEllipsoid(this short[] src, Vector3Int dim, short val, Vector3Int pos, Vector3Int radius)
        {
            Ellipsoid(src, dim, pos, radius, val);
        }

        public static void Ellipse(this short[] src, Vector3Int dim, Vector3Int pos, Vector3Int radius, short val = 1)
        {
            Debug.Assert(src.Length == dim.x * dim.y * dim.z);

            var rxSqr = radius.x == 0 ? 1 : radius.x * radius.x;
            var rySqr = radius.y == 0 ? 1 : radius.y * radius.y;
            var rzSqr = radius.z == 0 ? 1 : radius.z * radius.z;
            var rSqr = rxSqr * rySqr * rzSqr;

            for (var x = pos.x - radius.x; x < pos.x + radius.x + 1; ++x)
            {
                if (x < 0 || x >= dim.x) continue;
                var xSqr = (pos.x - x) * (pos.x - x);
                for (var y = pos.y - radius.y; y < pos.y + radius.y + 1; ++y)
                {
                    if (y < 0 || y >= dim.y) continue;
                    var ySqr = (pos.y - y) * (pos.y - y);
                    for (var z = pos.z - radius.z; z < pos.z + radius.z + 1; ++z)
                    {
                        if (z < 0 || z >= dim.z) continue;
                        var zSqr = (pos.z - z) * (pos.z - z);
                        if (xSqr * rySqr * rzSqr + ySqr * rxSqr * rzSqr + zSqr * rxSqr * rySqr < rSqr)
                        {
                            src[x + y * dim.x + z * dim.x * dim.y] = val;
                        }
                    }
                }
            }
        }

        public static void FillEllipse(this short[] src, Vector3Int dim, short val, Vector3Int pos, Vector3Int radius)
        {
            Ellipse(src, dim, pos, radius, val);
        }

        public static short Clamp(this short v, short min, short max)
        {
            if (v > max) v = max;
            if (v < min) v = min;
            return v;
        }

        public static (short min, short max) MinMax(this IEnumerable<short> v)
        {
            (short min, short max) = (short.MaxValue, short.MinValue);
            foreach (var i in v)
            {
                if (min > i) min = i;
                if (max < i) max = i;
            }
            return (min, max);
        }

        public static DateTime? ParseToDateTime(this string text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            return DateTime.Parse(text);
        }

        public static async Task Start(this System.Threading.Thread thread, System.Threading.CancellationToken ct, System.Action onProgress = null)
        {
            thread.Start();
            while (thread.IsAlive)
            {
                onProgress?.Invoke();
                if (ct.IsCancellationRequested)
                {
                    thread.Abort();
                    ct.ThrowIfCancellationRequested();
                }
                await Task.Delay(100);
            }
        }

        public static T[,,] To3D<T>(this T[] source, int w, int h)
        {
            int d = source.Length / w * h;
            Debug.Assert(source.Length == w * h * d);
            T[,,] buff3D = new T[h, w, d];
            Buffer.BlockCopy(source, 0, buff3D, 0, h * w);
            return buff3D;
        }

        public static (float Min, float Max, float Avg, int nanCount) MinMaxAvgExcludeNaN(this IEnumerable<float> source)
        { // linq 의 Min, Max 는 IComparable 처리방식으로 NaN 비교시 문제(https://en.wikipedia.org/wiki/IEEE_754-2008_revision#Min_and_max) 를 피하기 위함
            if (source.Count() == 0) throw new ArgumentException("should not be empty", nameof(source));

            var min = float.MaxValue;
            var max = float.MinValue;
            var sum = 0.0f;
            var cnt = 0;
            foreach (var x in source)
            {
                if (float.IsNaN(x)) continue;
                ++cnt;
                sum += x;
                if (min > x) min = x;
                if (max < x) max = x;
            }
            return (min, max, cnt > 0 ? (sum / cnt) : float.NaN, source.Count() - cnt);
        }

        public static string Md5(this string soruce)
        { // http://wiki.unity3d.com/index.php?title=MD5
            var bytes = System.Text.Encoding.UTF8.GetBytes(soruce);
            var hashed = md5.ComputeHash(bytes);

            var hash = "";
            for (var i = 0; i < hashed.Length; ++i)
                hash += Convert.ToString(hashed[i], 16).PadLeft(2, '0');
            return hash.PadLeft(32, '0');
        }
        private static System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();

        public static string Sha1(this string soruce)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(soruce);
            var hashed = sha1.ComputeHash(bytes);

            var hash = "";
            for (var i = 0; i < hashed.Length; ++i)
                hash += Convert.ToString(hashed[i], 16).PadLeft(2, '0');
            return hash.PadLeft(32, '0');
        }
        private static System.Security.Cryptography.SHA1 sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();

        public static void Shuffle<T>(this IList<T> list, int? seed = null)
        { // ref: https://stackoverflow.com/questions/273313
            var rnd = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
            var n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}