// https://bitbucket.org/alkee/aus

using System;
using UnityEngine;

namespace aus
{
    // usage example
    //
    //   using (new aus.RollbackTransform(blockManager.BlockContainer.transform))
    //   {
    //      // alignment for convenience
    //      blockManager.BlockContainer.transform.localScale = Vector3.one;
    //      blockManager.BlockContainer.transform.rotation = Quaternion.identity;
    //
    //      // .. do some work here
    //      
    //    } // will be transform recovered automatically
    //
    public class RollbackTransform : IDisposable
    {
        public RollbackTransform(Transform target)
        {
            t = target;
            Save();
        }

        public void Dispose()
        {
            Recover();
        }

        private void Save()
        {
            oPos = t.position;
            oRot = t.rotation;
            oScale = t.localScale;
        }

        private void Recover()
        {
            t.position = oPos;
            t.rotation = oRot;
            t.localScale = oScale;
        }

        private Transform t;
        private Vector3 oPos;
        private Quaternion oRot;
        private Vector3 oScale;
    }

    /// <summary>
    /// Makes transform to
    ///     position = zero
    ///     rotation = identity
    ///     localScale = one
    /// and recovers original transform disposed
    /// </summary>
    public class NormalizeAndRollbackTransform : RollbackTransform
    {
        public NormalizeAndRollbackTransform(Transform target)
            : base(target)
        {
            target.position = Vector3.zero;
            target.rotation = Quaternion.identity;
            target.localScale = Vector3.one;
        }
    }

}
