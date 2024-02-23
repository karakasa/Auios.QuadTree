// Copyright 2020 Connor Andrew Ngo
// Licensed under the MIT License

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Auios.QuadTree
{
    /// <summary>
    /// A tree data structure in which each node has exactly four children. 
    /// Used to partition a two-dimensional space by recursively subdividing it into four quadrants.
    /// Allows to efficiently find objects spatially relative to each other.
    /// </summary>
    /// <typeparam name="T">The type of elements in the QuadTree.</typeparam>
    public sealed class QuadTreeNonOverlap<T, TBoundProvider>
        where TBoundProvider : struct, IQuadTreeObjectBounds<T>
    {
        /// <summary>The area of this quadrant.</summary>
        public QuadTreeRect Area;
        /// <summary>Objects in this quadrant.</summary>
        private readonly List<T> _objects;
        /// <summary>If this quadrant has sub quadrants. Objects only exist on quadrants without children.</summary>
        private bool _hasChildren;

        /// <summary>Top left quadrant.</summary>
        private QuadTreeNonOverlap<T, TBoundProvider> quad_TL;
        /// <summary>Top right quadrant.</summary>
        private QuadTreeNonOverlap<T, TBoundProvider> quad_TR;
        /// <summary>Bottom left quadrant.</summary>
        private QuadTreeNonOverlap<T, TBoundProvider> quad_BL;
        /// <summary>Bottom right quadrant.</summary>
        private QuadTreeNonOverlap<T, TBoundProvider> quad_BR;

        /// <summary>Gets the current depth level of this quadrant.</summary>
        public int CurrentLevel { get; }
        /// <summary>Gets the max depth level.</summary>
        public int MaxLevel { get; }
        /// <summary>Gets the max number of objects in this quadrant.</summary>
        public int MaxObjects { get; }

        /// <summary>Initializes a new instance of the <see cref="T:Auios.QuadTree.QuadTree`1"></see> class.</summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the boundary rectangle.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the boundary rectangle.</param>
        /// <param name="width">The width of the boundary rectangle.</param>
        /// <param name="height">The height of the boundary rectangle.</param>
        /// <param name="maxObjects">The max number of elements in one rectangle.</param>
        /// <param name="maxLevel">The max depth level.</param>
        /// <param name="currentLevel">The current depth level. Leave default if this is the root QuadTree.</param>
        public QuadTreeNonOverlap(double x, double y, double width, double height,
            int maxObjects = 10, int maxLevel = 5, int currentLevel = 0)
        {
            Area = new QuadTreeRect(x, y, width, height);
            _objects = [];

            CurrentLevel = currentLevel;
            MaxLevel = maxLevel;
            MaxObjects = maxObjects;

            _hasChildren = false;
        }

        /// <summary>Initializes a new instance of the <see cref="T:Auios.QuadTree.QuadTree`1"></see> class.</summary>
        /// <param name="width">The width of the boundary rectangle.</param>
        /// <param name="height">The height of the boundary rectangle.</param>
        /// <param name="maxObjects">The max number of elements in one rectangle.</param>
        /// <param name="maxLevel">The max depth level.</param>
        /// <param name="currentLevel">The current depth level. Leave default if this is the root QuadTree.</param>
        public QuadTreeNonOverlap(double width, double height, int maxObjects = 10, int maxLevel = 5, int currentLevel = 0)
            : this(0, 0, width, height, maxObjects, maxLevel, currentLevel) { }

        private bool IsObjectInside(T obj)
        {
            if (default(TBoundProvider).GetTop(obj) < Area.Bottom) return false;
            if (default(TBoundProvider).GetBottom(obj) > Area.Top) return false;
            if (default(TBoundProvider).GetLeft(obj) > Area.Right) return false;
            if (default(TBoundProvider).GetRight(obj) < Area.Left) return false;
            return true;
        }

        /// <summary>Checks if the current quadrant is overlapping with a <see cref="T:Auios.QuadTree.QuadTreeRect"></see></summary>
        private bool IsOverlapping(QuadTreeRect rect)
        {
            if (rect.Right < Area.Left || rect.Left > Area.Right) return false;
            if (rect.Top > Area.Bottom || rect.Bottom < Area.Top) return false;
            Area.isOverlapped = true;
            return true;
        }

        /// <summary>Splits the current quadrant into four new quadrants and drops all objects to the lower quadrants.</summary>
        private void Quarter()
        {
            if (CurrentLevel >= MaxLevel) return;

            int nextLevel = CurrentLevel + 1;
            _hasChildren = true;
            quad_TL = new(Area.X, Area.Y, Area.HalfWidth, Area.HalfHeight, MaxObjects, MaxLevel, nextLevel);
            quad_TR = new(Area.CenterX, Area.Y, Area.HalfWidth, Area.HalfHeight, MaxObjects, MaxLevel, nextLevel);
            quad_BL = new(Area.X, Area.CenterY, Area.HalfWidth, Area.HalfHeight, MaxObjects, MaxLevel, nextLevel);
            quad_BR = new(Area.CenterX, Area.CenterY, Area.HalfWidth, Area.HalfHeight, MaxObjects, MaxLevel, nextLevel);

            foreach (T obj in _objects)
            {
                Add(obj);
            }

            _objects.Clear();
        }

        /// <summary> Removes all elements from the <see cref="T:Auios.QuadTree.QuadTree`1"></see>.</summary>
        public void Clear()
        {
            if (_hasChildren)
            {
                quad_TL.Clear();
                quad_TL = null;
                quad_TR.Clear();
                quad_TR = null;
                quad_BL.Clear();
                quad_BL = null;
                quad_BR.Clear();
                quad_BR = null;
            }

            _objects.Clear();
            _hasChildren = false;
            Area.isOverlapped = false;
        }

        /// <summary> Adds an object into the <see cref="T:Auios.QuadTree.QuadTree`1"></see>.</summary>
        /// <param name="obj">The object to add.</param>
        /// <returns>true if the object is successfully added to the <see cref="T:Auios.QuadTree.QuadTree`1"></see>; false if object is not added to the <see cref="T:Auios.QuadTree.QuadTree`1"></see>.</returns>
        public bool Add(T obj)
        {
            // CHANGED: Allow zerod value type
            // if (obj == null) throw new ArgumentNullException(nameof(obj));

            if (!IsObjectInside(obj)) return false;

            if (_hasChildren)
            {
                if (quad_TL.Add(obj)) return true;
                if (quad_TR.Add(obj)) return true;
                if (quad_BL.Add(obj)) return true;
                if (quad_BR.Add(obj)) return true;
            }
            else
            {
                _objects.Add(obj);
                if (_objects.Count > MaxObjects)
                {
                    Quarter();
                }
            }

            return true;
        }

        /// <summary> Adds a collection of objects into the <see cref="T:Auios.QuadTree.QuadTree`1"></see>.</summary>
        /// <param name="objects">The collection of objects to add.</param>
        public void AddRange(IEnumerable<T> objects)
        {
            foreach (T obj in objects)
            {
                Add(obj);
            }
        }

        /// <summary>Returns the total number of objects in the <see cref="T:Auios.QuadTree.QuadTree`1"></see> and its children.</summary>
        /// <returns>the total number of objects in this instance.</returns>
        public int Count()
        {
            int count = 0;
            if (_hasChildren)
            {
                count += quad_TL.Count();
                count += quad_TR.Count();
                count += quad_BL.Count();
                count += quad_BR.Count();
            }
            else
            {
                count = _objects.Count;
            }

            return count;
        }

        /// <summary> Returns every <see cref="T:Auios.QuadTree.QuadTreeRect"></see> from the <see cref="T:Auios.QuadTree.QuadTree`1"></see>.</summary>
        /// <returns> an array of <see cref="T:Auios.QuadTree.QuadTreeRect"></see> from the <see cref="T:Auios.QuadTree.QuadTree`1"></see>.</returns>
        public List<QuadTreeRect> GetGrid()
        {
            List<QuadTreeRect> grid = new List<QuadTreeRect> { Area };
            if (_hasChildren)
            {
                grid.AddRange(quad_TL.GetGrid());
                grid.AddRange(quad_TR.GetGrid());
                grid.AddRange(quad_BL.GetGrid());
                grid.AddRange(quad_BR.GetGrid());
            }
            return grid;
        }

        /// <summary>Searches for objects in any quadrants which the passed region overlaps, but not specifically within that region.</summary>
        /// <param name="rect">The search region.</param>
        /// <returns>an array of objects.</returns>
        public List<T> FindObjects(QuadTreeRect rect)
        {
            var foundObjects = new List<T>();
            FindObjectsInternal(rect, foundObjects);
            return foundObjects;
        }
        public List<T> FindObjects(T bounds)
        {
            return FindObjects(new QuadTreeRect(
                default(TBoundProvider).GetLeft(bounds),
                default(TBoundProvider).GetTop(bounds),
                default(TBoundProvider).GetRight(bounds),
                default(TBoundProvider).GetBottom(bounds)
                ));
        }
        private void FindObjectsInternal(QuadTreeRect rect, List<T> list)
        {
            if (_hasChildren)
            {
                quad_TL.FindObjectsInternal(rect, list);
                quad_TR.FindObjectsInternal(rect, list);
                quad_BL.FindObjectsInternal(rect, list);
                quad_BR.FindObjectsInternal(rect, list);
            }
            else
            {
                if (IsOverlapping(rect))
                {
                    list.AddRange(_objects);
                }
            }
        }

        public ObjectCollection FindObjectsForEnumeration(QuadTreeRect rect)
        {
            var foundObjects = new List<QuadTreeNonOverlap<T, TBoundProvider>>();
            FindObjectsForEnumerationInternal(rect, foundObjects);
            return new(foundObjects);
        }
        private void FindObjectsForEnumerationInternal(QuadTreeRect rect, List<QuadTreeNonOverlap<T, TBoundProvider>> list)
        {
            if (_hasChildren)
            {
                quad_TL.FindObjectsForEnumerationInternal(rect, list);
                quad_TR.FindObjectsForEnumerationInternal(rect, list);
                quad_BL.FindObjectsForEnumerationInternal(rect, list);
                quad_BR.FindObjectsForEnumerationInternal(rect, list);
            }
            else if (IsOverlapping(rect))
            {
                list.Add(this);
            }
        }
        public readonly struct ObjectCollection(List<QuadTreeNonOverlap<T, TBoundProvider>> list)
            : IEnumerable<T>
        {
            public ObjectCollectionEnumerator GetEnumerator() => new(list);
            IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
        public struct ObjectCollectionEnumerator(List<QuadTreeNonOverlap<T, TBoundProvider>> list)
            : IEnumerator<T>
        {
            private int ListIndex = 0;
            private int InListIndex = -1;

            private T current = default;

            private readonly List<QuadTreeNonOverlap<T, TBoundProvider>> _list = list;
            readonly object IEnumerator.Current => Current;
            public readonly T Current => current;

            public readonly void Dispose()
            {
            }

            public void Reset()
            {
                ListIndex = 0;
                InListIndex = -1;
            }
            public bool MoveNext()
            {
                for (; ; )
                {
                    if (ListIndex >= _list.Count)
                    {
                        return false;
                    }

                    var l = _list[ListIndex]._objects;
                    ++InListIndex;
                    if (InListIndex >= l.Count)
                    {
                        InListIndex = -1;
                        ++ListIndex;
                        continue;
                    }

                    current = l[InListIndex];
                    return true;
                }
            }
        }
    }
}