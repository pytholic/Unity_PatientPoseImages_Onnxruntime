// https://bitbucket.org/alkee/aus

using System;
using System.Collections;
using UnityEngine;

namespace aus
{
    public static class Coroutines
    {
        public static IEnumerator Lerp(Color from, Color to, float duration, Action<Color> frame, System.Action end = null)
        {
            yield return null;
            float elapsed = 0.0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                frame(Color.Lerp(from, to, elapsed / duration));
                yield return null;
            }
            frame(to);
            if (end != null) end.Invoke();
        }

        public static IEnumerator Lerp(float from, float to, float duration, Action<float> frame, System.Action end = null)
        {
            yield return null;
            float elapsed = 0.0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                frame(Mathf.Lerp(from, to, elapsed / duration));
                yield return null;
            }
            frame(to);
            if (end != null) end.Invoke();
        }

        public static IEnumerator Lerp(Vector3 from, Vector3 to, float duration, Action<Vector3> frame, System.Action end = null)
        {
            yield return null;
            float elapsed = 0.0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                frame(Vector3.Lerp(from, to, elapsed / duration));
                yield return null;
            }
            frame(to);
            if (end != null) end.Invoke();
        }

        public static IEnumerator Lerp(Quaternion from, Quaternion to, float duration, Action<Quaternion> frame, System.Action end = null)
        {
            yield return null;
            float elapsed = 0.0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                frame(Quaternion.Lerp(from, to, elapsed / duration));
                yield return null;
            }
            frame(to);
            if (end != null) end.Invoke();
        }

        public static IEnumerator WaitAndRun(float duration, Func<IEnumerator> frame)
        {
            yield return new WaitForSeconds(duration);
            yield return frame();
        }

        // yield is not allowed in C# lambda(CS1621)
        public static IEnumerator WaitAndRun(float duration, System.Action frame)
        {
            yield return new WaitForSeconds(duration);
            frame();
        }

    }
}
