// https://bitbucket.org/alkee/aus
using System;
using System.Collections.Generic;
using UnityEngine;

namespace aus.Geometry
{
    // ref: https://github.com/Nition/UnityOctree

    public class Octree<T>
    {
        public int Count { get; private set; }
        public Bounds Bounds { get => rootNode.Bounds; }

        private OctreeNode<T> rootNode;
        private readonly float minSize;

        public Octree(float initialWorldSize, Vector3 initialWorldPos, float minNodeSize)
        {
            if (minNodeSize > initialWorldSize)
            {
                throw new ArgumentException($"Minimum node size must be at least as big as the initial world size. {nameof(minNodeSize)}: {minNodeSize}, {nameof(initialWorldSize)}: {initialWorldSize}");
            }
            Count = 0;
            minSize = minNodeSize;
            rootNode = new OctreeNode<T>(initialWorldSize, minSize, initialWorldPos);
        }

        public void Add(T obj, Vector3 objPos)
        {
            // Add object or expand the octree until it can be added
            var safetyCounter = 0; // Safety check against infinite/excessive growth
            while (!rootNode.Add(obj, objPos))
            {
                Grow(objPos - rootNode.Bounds.center);
                if (++safetyCounter > 20)
                {
                    throw new ApplicationException("Aborted Add operation as it seemed to be going on forever attempts at growing the octree.");
                }
            }
            Count++;
        }

        public List<T> GetNearBy(Vector3 pos, float radius)
        {
            var results = new List<T>();
            rootNode.GetNearBy(pos, radius * radius, results);
            return results;
        }

        public List<T> GetNearBy(Ray ray, float distance)
        {
            var results = new List<T>();

            // https://github.com/Nition/UnityOctree/blob/bbc473571c8a0879024077a928ca658a52ecbd99/Scripts/PointOctreeNode.cs#L124
            // ray to bounds
            var rayBounds = new Bounds(ray.origin + (ray.direction * 0.5f)
                , new Vector3(Mathf.Abs(ray.direction.x), Mathf.Abs(ray.direction.y), Mathf.Abs(ray.direction.z)));
            rayBounds.Expand(distance * 2); // effective area by distance

            rootNode.GetNearBy(ref rayBounds, ref ray, distance * distance, results);
            return results;
        }

        private void Grow(Vector3 direction)
        {
            var xDirection = direction.x >= 0 ? 1 : -1;
            var yDirection = direction.y >= 0 ? 1 : -1;
            var zDirection = direction.z >= 0 ? 1 : -1;
            var oldRoot = rootNode;
            var half = rootNode.SideLength / 2;
            var newCenter = rootNode.Bounds.center + new Vector3(xDirection * half, yDirection * half, zDirection * half);

            // Create a new, bigger octree root node
            rootNode = new OctreeNode<T>(oldRoot, minSize, newCenter);
        }
    }

    public class OctreeNode<T>
    {
        public Bounds Bounds { get; private set; }
        public float SideLength { get => Bounds.size.x; } // 정사각형
        public int Count { get; private set; } // 하위 요소들 포함

        private readonly float minSize;

        private class Element
        {
            public T Obj;
            public Vector3 Pos;
        }

        private readonly List<Element> objects = new List<Element>();

        private OctreeNode<T>[] children; // leaf 인 경우 children 이 없을 수 있으므로 초기화는 필요한 경우에(lazy initialize) 한함.
        private bool HasChildren { get { return children != null; } }

        // If there are already NUM_OBJECTS_ALLOWED in a node, we split it into children
        // A generally good number seems to be something around 8-15
        private const int NUM_OBJECTS_ALLOWED = 8;

        public OctreeNode(float baseLength, float minSize, Vector3 center)
        {
            this.minSize = minSize;
            Bounds = new Bounds(center, Vector3.one * baseLength);
        }

        /// <summary>
        ///     constuct a bigger node that contains a child
        /// </summary>
        public OctreeNode(OctreeNode<T> child, float minSize, Vector3 center)
            : this(child.SideLength * 2, minSize, center)
        {
            int childIndex = FindChildIndex(child.Bounds.center);
            CreateChildren();
            children[childIndex] = child;
        }

        public bool Add(T obj, Vector3 objPos)
        {
            if (Bounds.Contains(objPos) == false) return false; // not in charge

            Add(new Element { Obj = obj, Pos = objPos });
            return true;
        }

        public void GetNearBy(Vector3 pos, float sqrDistance, List<T> outResult)
        {
            if (Count == 0) return; // no points in here
            if ((Bounds.ClosestPoint(pos) - pos).sqrMagnitude > sqrDistance) return; // not interested(out of range)

            if (children != null) // this is not a leaf
            {
                foreach (var child in children)
                    child.GetNearBy(pos, sqrDistance, outResult); // recursive call to children
                return;
            }

            // this is a leaf node
            foreach (var elem in objects)
            {
                if ((pos - elem.Pos).sqrMagnitude <= sqrDistance)
                {
                    outResult.Add(elem.Obj);
                }
            }
        }

        public void GetNearBy(ref Bounds effectiveArea, ref Ray ray, float sqrMagnitude, List<T> outResult)
        {
            if (Count == 0) return; // no points in here
            if (effectiveArea.Intersects(Bounds) == false) return; // no interaction with the ray bounds

            if (children != null) // this is not a leaf
            {
                foreach (var child in children)
                    child.GetNearBy(ref effectiveArea, ref ray, sqrMagnitude, outResult); // recursive call to children
                return;
            }

            // this is a leaf node
            foreach (var elem in objects)
            {
                var sqrDistanceFromRay = Vector3.Cross(ray.direction, elem.Pos - ray.origin).sqrMagnitude;

                if (sqrDistanceFromRay <= sqrMagnitude)
                {
                    outResult.Add(elem.Obj);
                }
            }
        }

        /// <summary>
        ///     Draws the bounds of all objects in the tree visually for debugging.
        ///     Must be called from OnDrawGizmos externally. See also: <see cref="DrawAllBounds(int)"/> DrawAllBounds.
        /// </summary>
        public void DrawAllObjects()
        {
            if (children != null)
            {
                foreach (var child in children) child.DrawAllObjects();
                return;
            }

            // leaf node
            foreach (var elem in objects)
            {
                Gizmos.DrawIcon(elem.Pos, "animationkeyframe", false);
            }
        }

        /// <summary>
        ///     add a element in given position
        /// </summary>
        /// <param name="e">elemt to be added</param>
        /// <returns>a leaf node which contains the point</returns>
        private OctreeNode<T> Add(Element e)
        {
            // We know it fits at this level if we've got this far
            ++Count; // never fails

            // We always put things in the deepest possible child
            // So we can skip checks and simply move down if there are children aleady
            if (HasChildren == false)
            {
                // Just add if few objects are here, or children would be below min size
                if (objects.Count < NUM_OBJECTS_ALLOWED || (SideLength / 2) < minSize)
                {
                    objects.Add(e);
                    return this; // We're done. No children yet
                }

                // Enough objects in this node already: Create the 8 children
                CreateChildren();
                // Now that we have the new children, move this node's existing objects into them
                foreach (var elem in objects)
                {
                    FindChild(elem.Pos).Add(elem);
                }
                objects.Clear(); // Remove from here. This node is not a leaf anymore.
            }

            // Handle the new object we're adding now
            return FindChild(e.Pos).Add(e);
        }

        private void CreateChildren()
        {
            var quarter = SideLength / 4f;
            var newLength = SideLength / 2;
            var center = Bounds.center;
            children = new OctreeNode<T>[8];
            children[0] = new OctreeNode<T>(newLength, minSize, center + new Vector3(-quarter, quarter, -quarter));
            children[1] = new OctreeNode<T>(newLength, minSize, center + new Vector3(quarter, quarter, -quarter));
            children[2] = new OctreeNode<T>(newLength, minSize, center + new Vector3(-quarter, quarter, quarter));
            children[3] = new OctreeNode<T>(newLength, minSize, center + new Vector3(quarter, quarter, quarter));
            children[4] = new OctreeNode<T>(newLength, minSize, center + new Vector3(-quarter, -quarter, -quarter));
            children[5] = new OctreeNode<T>(newLength, minSize, center + new Vector3(quarter, -quarter, -quarter));
            children[6] = new OctreeNode<T>(newLength, minSize, center + new Vector3(-quarter, -quarter, quarter));
            children[7] = new OctreeNode<T>(newLength, minSize, center + new Vector3(quarter, -quarter, quarter));
        }

        private OctreeNode<T> FindChild(Vector3 objPos)
        {
            var index = FindChildIndex(objPos);
            return children[index];
        }

        private int FindChildIndex(Vector3 objPos)
        {
            // Find which child the object is closest to based on where the
            // object's center is located in relation to the octree's center
            var center = Bounds.center;
            return (objPos.x <= center.x ? 0 : 1) + (objPos.y >= center.y ? 0 : 4) + (objPos.z <= center.z ? 0 : 2);
        }
    }
}