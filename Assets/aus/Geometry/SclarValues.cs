// https://bitbucket.org/alkee/aus

using aus.Extension;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace aus.Geometry
{
    public class ScalarValues
    {
        public float[] Values { get; private set; }
        public float EffectiveCount { get; private set; } // NaN excluded

        public float Min { get; private set; }
        public float Max { get; private set; }
        public float Mean { get; private set; }
        public float Variance { get; private set; }

        public static ScalarValues Create(PointCloud src, float radius, Func<IEnumerable<Vector3>, float> calculator)
        {
            var count = src.Count;
            var values = new float[count];
            Parallel.For(0, count, (i) =>
            {
                values[i] = calculator(src.GetPoints(src.Points[i], radius));
            });
            return new ScalarValues(values);
        }

        public ScalarValues(float[] src)
        {
            if (src is null) throw new ArgumentNullException(nameof(src));
            if (src.Length == 0) throw new ArgumentException("0 size of array", nameof(src));

            Values = src;

            var stat = Values.MinMaxAvgExcludeNaN();
            Min = stat.Min;
            Max = stat.Max;
            Mean = stat.Avg;
            EffectiveCount = Values.Length - stat.nanCount;

            if (EffectiveCount == 0) throw new ArgumentException($"all NaN array({src.Length})", nameof(src));

            // calculate Variance
            var sum = 0.0f;
            for (var i = 0; i < src.Length; ++i)
            {
                var val = src[i];
                if (float.IsNaN(val)) continue;
                sum += (val - Mean) * (val - Mean);
            }
            Variance = sum / EffectiveCount;
        }

        #region rendering helpers

        public static readonly Color[] DEFAULT_COLOR_SCALE = new Color[] { Color.blue, Color.green, Color.yellow, Color.red };
        public static readonly Color DEFAULT_NAN_COLOR = Color.black;

        public Color[] CreateColors(Color NaNcolor, params Color[] colorscales)
        {
            if (colorscales == null || colorscales.Length == 0) new ArgumentNullException(nameof(colorscales));
            if (colorscales.Length == 1)
            {
                var ret = new Color[Values.Length];
                ret.Fill(colorscales[0], 0, ret.Length);
                return ret;
            }

            var interval = 1.0f / (colorscales.Length - 1);
            var colors = new List<GradientColorKey>();
            var time = 0.0f;
            foreach (var c in colorscales)
            {
                colors.Add(new GradientColorKey { color = c, time = time });
                time += interval;
            }

            return CreateColors(NaNcolor, colors.ToArray());
        }

        public Color[] CreateColors(Color NaNcolor, params GradientColorKey[] colorKeys)
        {
            if (colorKeys == null || colorKeys.Length == 0) new ArgumentException("color key must have one or more", nameof(colorKeys));

            var alphaKeys = new GradientAlphaKey[colorKeys.Length];
            for (var i = 0; i < colorKeys.Length; ++i)
            {
                alphaKeys[i].time = colorKeys[i].time;
                alphaKeys[i].alpha = colorKeys[i].color.a;
            }

            var gradient = new Gradient();
            gradient.SetKeys(colorKeys, alphaKeys);
            return CreateColors(NaNcolor, gradient);
        }

        public Color[] CreateColors(Color NaNcolor, Gradient gradient)
        {
            if (gradient == null) throw new ArgumentNullException(nameof(gradient));

            var t = Max - Min;
            if (t == 0)
            {
                Debug.LogError($"same values of min-max {Min}");
                return null;
            }

            var colors = new Color[Values.Length];
            Parallel.For(0, Values.Length, (i) =>
            {
                var v = Values[i];
                if (float.IsNaN(v)) { colors[i] = NaNcolor; return; }
                colors[i] = gradient.Evaluate((v - Min) / t);
            });
            return colors;
        }

        #endregion rendering helpers
    }
}