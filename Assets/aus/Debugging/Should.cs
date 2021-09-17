// https://bitbucket.org/alkee/aus
using System;
using System.Reflection;
using UnityEngine;

namespace aus.Debugging
{
    /// <summary>
    /// shouldjs(https://github.com/shouldjs/should.js) like ...
    /// </summary>
    public class Should
    {
        public Should(MonoBehaviour source)
        {
            if (source == null) throw new ArgumentNullException("source");
            src = source;
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public void NotHaveNullMemberFields(bool declaredOnly = true)
        {
            var bindings = BindingFlags.Public | BindingFlags.Instance;
            if (declaredOnly) bindings |= BindingFlags.DeclaredOnly;
            var fields = src.GetType().GetFields(bindings);
            foreach (var f in fields)
            {
                if (f.FieldType.IsValueType) continue;
                Debug.Assert(!f.GetValue(src).Equals(null), "member " + f.Name + " should not be null in " + src.GetType().Name);
            }
        }

        private MonoBehaviour src;
    }

    public static class ShouldExtension
    {
        public static Should Should(this MonoBehaviour obj)
        {
            return new Should(obj);
        }
    }
}

