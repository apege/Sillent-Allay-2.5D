using UnityEngine;
using System;
using System.Collections.Generic;

namespace Terrain2D5.Core
{
    /// <summary>
    /// Represents a 2D spline with bezier curve support.
    /// Handles both closed and open splines for terrain shapes.
    /// </summary>
    [System.Serializable]
    public class SplineData
    {
        // Control point with tangent handles for bezier curves
        [System.Serializable]
        public class ControlPoint
        {
            public Vector2 position;
            public Vector2 tangentIn;
            public Vector2 tangentOut;
            public TangentMode tangentMode;

            public ControlPoint(Vector2 pos)
            {
                position = pos;
                tangentIn = Vector2.left * 0.5f;
                tangentOut = Vector2.right * 0.5f;
                tangentMode = TangentMode.Aligned;
            }

            public ControlPoint(Vector2 pos, Vector2 tanIn, Vector2 tanOut, TangentMode mode)
            {
                position = pos;
                tangentIn = tanIn;
                tangentOut = tanOut;
                tangentMode = mode;
            }

            public ControlPoint Clone()
            {
                return new ControlPoint(position, tangentIn, tangentOut, tangentMode);
            }
        }

        public enum TangentMode
        {
            Free,      // Independent tangent handles
            Aligned,   // Tangents aligned but different lengths
            Mirrored   // Tangents mirrored (equal length, opposite direction)
        }

        [SerializeField] private List<ControlPoint> controlPoints = new List<ControlPoint>();
        [SerializeField] private bool isClosed = true;
        [SerializeField] private int resolution = 32; // Points per curve segment

        public List<ControlPoint> ControlPoints => controlPoints;
        public bool IsClosed { get => isClosed; set => isClosed = value; }
        public int Resolution { get => resolution; set => resolution = Mathf.Max(4, value); }

        public int PointCount => controlPoints.Count;

        public SplineData()
        {
            controlPoints = new List<ControlPoint>();
            isClosed = true;
            resolution = 32;
        }

        /// <summary>
        /// Adds a control point to the spline
        /// </summary>
        public void AddPoint(Vector2 position)
        {
            var point = new ControlPoint(position);

            // Auto-calculate tangents based on neighboring points
            if (controlPoints.Count > 0)
            {
                var prevPoint = controlPoints[controlPoints.Count - 1];
                Vector2 direction = (position - prevPoint.position).normalized;
                float distance = Vector2.Distance(position, prevPoint.position) * 0.33f;

                prevPoint.tangentOut = direction * distance;
                point.tangentIn = -direction * distance;
            }

            controlPoints.Add(point);
        }

        /// <summary>
        /// Inserts a point at specified index
        /// </summary>
        public void InsertPoint(int index, Vector2 position)
        {
            index = Mathf.Clamp(index, 0, controlPoints.Count);
            var point = new ControlPoint(position);
            controlPoints.Insert(index, point);
            RecalculateTangents(index);
        }

        /// <summary>
        /// Removes point at index
        /// </summary>
        public void RemovePoint(int index)
        {
            if (index >= 0 && index < controlPoints.Count)
            {
                controlPoints.RemoveAt(index);

                // Recalculate tangents for neighboring points
                if (controlPoints.Count > 0)
                {
                    int prevIndex = (index - 1 + controlPoints.Count) % controlPoints.Count;
                    RecalculateTangents(prevIndex);
                }
            }
        }

        /// <summary>
        /// Updates tangent mode for a point and adjusts tangents accordingly
        /// </summary>
        public void SetTangentMode(int index, TangentMode mode)
        {
            if (index >= 0 && index < controlPoints.Count)
            {
                var point = controlPoints[index];
                point.tangentMode = mode;

                // Adjust tangents based on mode
                switch (mode)
                {
                    case TangentMode.Aligned:
                        AlignTangents(point);
                        break;
                    case TangentMode.Mirrored:
                        MirrorTangents(point);
                        break;
                }
            }
        }

        /// <summary>
        /// Updates a tangent handle and respects tangent mode
        /// </summary>
        public void SetTangent(int index, Vector2 tangent, bool isInTangent)
        {
            if (index < 0 || index >= controlPoints.Count) return;

            var point = controlPoints[index];

            if (isInTangent)
            {
                point.tangentIn = tangent;

                if (point.tangentMode == TangentMode.Aligned)
                {
                    point.tangentOut = -tangent.normalized * point.tangentOut.magnitude;
                }
                else if (point.tangentMode == TangentMode.Mirrored)
                {
                    point.tangentOut = -tangent;
                }
            }
            else
            {
                point.tangentOut = tangent;

                if (point.tangentMode == TangentMode.Aligned)
                {
                    point.tangentIn = -tangent.normalized * point.tangentIn.magnitude;
                }
                else if (point.tangentMode == TangentMode.Mirrored)
                {
                    point.tangentIn = -tangent;
                }
            }
        }

        /// <summary>
        /// Generates interpolated points along the spline for mesh generation
        /// </summary>
        public List<Vector2> GeneratePoints()
        {
            var points = new List<Vector2>();

            if (controlPoints.Count < 2)
            {
                foreach (var cp in controlPoints)
                {
                    points.Add(cp.position);
                }
                return points;
            }

            int segmentCount = isClosed ? controlPoints.Count : controlPoints.Count - 1;

            for (int i = 0; i < segmentCount; i++)
            {
                var p0 = controlPoints[i];
                var p1 = controlPoints[(i + 1) % controlPoints.Count];

                // Cubic bezier curve evaluation
                for (int j = 0; j < resolution; j++)
                {
                    float t = j / (float)resolution;
                    Vector2 point = EvaluateBezier(
                        p0.position,
                        p0.position + p0.tangentOut,
                        p1.position + p1.tangentIn,
                        p1.position,
                        t
                    );
                    points.Add(point);
                }
            }

            // Add final point if not closed
            if (!isClosed && controlPoints.Count > 0)
            {
                points.Add(controlPoints[controlPoints.Count - 1].position);
            }

            return points;
        }

        /// <summary>
        /// Evaluates a cubic bezier curve at parameter t
        /// </summary>
        private Vector2 EvaluateBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector2 point = uuu * p0;
            point += 3 * uu * t * p1;
            point += 3 * u * tt * p2;
            point += ttt * p3;

            return point;
        }

        /// <summary>
        /// Recalculates auto-tangents for smooth curves
        /// </summary>
        private void RecalculateTangents(int index)
        {
            if (controlPoints.Count < 2) return;

            var point = controlPoints[index];

            int prevIndex = (index - 1 + controlPoints.Count) % controlPoints.Count;
            int nextIndex = (index + 1) % controlPoints.Count;

            if (!isClosed && (index == 0 || index == controlPoints.Count - 1))
            {
                // End points in open splines
                if (index == 0 && controlPoints.Count > 1)
                {
                    Vector2 dir = (controlPoints[1].position - point.position).normalized;
                    float dist = Vector2.Distance(point.position, controlPoints[1].position) * 0.33f;
                    point.tangentOut = dir * dist;
                    point.tangentIn = -dir * dist;
                }
                else if (index == controlPoints.Count - 1)
                {
                    Vector2 dir = (point.position - controlPoints[index - 1].position).normalized;
                    float dist = Vector2.Distance(point.position, controlPoints[index - 1].position) * 0.33f;
                    point.tangentIn = -dir * dist;
                    point.tangentOut = dir * dist;
                }
            }
            else
            {
                // Middle points or closed spline
                Vector2 prev = controlPoints[prevIndex].position;
                Vector2 next = controlPoints[nextIndex].position;
                Vector2 tangent = (next - prev).normalized;

                float distPrev = Vector2.Distance(point.position, prev) * 0.33f;
                float distNext = Vector2.Distance(point.position, next) * 0.33f;

                point.tangentIn = -tangent * distPrev;
                point.tangentOut = tangent * distNext;
            }
        }

        private void AlignTangents(ControlPoint point)
        {
            if (point.tangentIn.magnitude > 0.001f)
            {
                point.tangentOut = -point.tangentIn.normalized * point.tangentOut.magnitude;
            }
        }

        private void MirrorTangents(ControlPoint point)
        {
            point.tangentOut = -point.tangentIn;
        }

        /// <summary>
        /// Calculates bounds of the spline
        /// </summary>
        public Bounds GetBounds()
        {
            if (controlPoints.Count == 0)
                return new Bounds(Vector3.zero, Vector3.zero);

            Vector2 min = controlPoints[0].position;
            Vector2 max = controlPoints[0].position;

            foreach (var point in controlPoints)
            {
                min = Vector2.Min(min, point.position);
                max = Vector2.Max(max, point.position);
            }

            Vector2 center = (min + max) * 0.5f;
            Vector2 size = max - min;

            return new Bounds(center, size);
        }

        /// <summary>
        /// Creates a deep copy of this spline
        /// </summary>
        public SplineData Clone()
        {
            var clone = new SplineData();
            clone.isClosed = this.isClosed;
            clone.resolution = this.resolution;

            foreach (var point in controlPoints)
            {
                clone.controlPoints.Add(point.Clone());
            }

            return clone;
        }

        /// <summary>
        /// Clears all control points
        /// </summary>
        public void Clear()
        {
            controlPoints.Clear();
        }
    }
}