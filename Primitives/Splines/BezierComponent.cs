using UnityEngine;

using System;
using System.Collections.Generic;

namespace Procedural
{
    public class BezierComponent : MonoBehaviour, ISpline
    {
        public bool _showCurve = false;
        public Color _splineColor = Color.red;
        public Color _tangentColor = Color.Lerp(Color.red, Color.black, 0.75f);
        public Color _handleColor = Color.Lerp(Color.red, Color.black, 0.85f);
        public bool _showForwardVectors = false;
        public bool _triangulate = false;
        public int _triangulateSegments = 16;
        public bool _reverseCPs = false;

        public List<Vector3> points = new List<Vector3>(new Vector3[]{
            new Vector3(0.0f, 1.0f, 0.0f),
            new Vector3(1.0f, 0.0f, 0.0f)
        });
        public bool _continuous = false;

        private uint _pointsHash = 0;
        private Bezier _bezier = null;
        private bool _overridden = false;

        public void OverrideBezier(Bezier b)
        {
            _bezier = b;
            _overridden = true;
        }

        public Vector3 PositionSample(float t)
        {
            UpdateBezier();
            return _bezier.PositionSample(t);
        }

        public Vector3 ForwardSample(float t)
        {
            UpdateBezier();
            return _bezier.ForwardSample(t);
        }

        public bool Closed
        {
            get
            {
                UpdateBezier();
                return _bezier.Closed;
            }
        }

        public void UpdateBezier()
        {
            if (_overridden)
                return;

            var pointsHash = PointsHash();
            if (pointsHash != _pointsHash)
            {
                if (_reverseCPs)
                {
                    points.Reverse();
                    _reverseCPs = false;
                }

                _bezier = Bezier.ConstructSmoothSpline(points, _continuous);

                var meshFilter = GetComponent<MeshFilter>();

                if (_triangulate)
                {
                    if (!meshFilter)
                        meshFilter = gameObject.AddComponent<MeshFilter>();

                    if (meshFilter.sharedMesh)
                        DestroyImmediate(meshFilter.sharedMesh);

                    if (!GetComponent<Renderer>())
                        gameObject.AddComponent<MeshRenderer>();

                    meshFilter.sharedMesh = _bezier.Triangulate((uint)_triangulateSegments);
                }
                else if (meshFilter)
                {
                    if (meshFilter.sharedMesh)
                        DestroyImmediate(meshFilter.sharedMesh);

                    if (GetComponent<Renderer>())
                        DestroyImmediate(GetComponent<Renderer>());

                    DestroyImmediate(meshFilter);
                }


                _pointsHash = pointsHash;
            }
        }

        public uint PointsHash()
        {
            var result = (uint)points.GetHashCode();
            result += (uint)_triangulateSegments;
            foreach (var pnt in points)
            {
                result ^= BitConverter.ToUInt32(BitConverter.GetBytes(pnt.x), 0);
                result ^= BitConverter.ToUInt32(BitConverter.GetBytes(pnt.y), 0);
                result ^= BitConverter.ToUInt32(BitConverter.GetBytes(pnt.z), 0);
            }

            if (_continuous)
                result ^= 1;

            if (_triangulate)
                result ^= 2;

            if (_reverseCPs)
                result ^= 4;

            return result;
        }

        void OnDrawGizmos()
        {
            if (_showCurve)
            {
                UpdateBezier();

                Gizmos.color = _splineColor;
                var last = transform.TransformPoint(_bezier.PositionSample(0.0f));
                for (float t = 0.01f; t < 1.0f; t += 0.01f)
                {
                    var current = transform.TransformPoint(_bezier.PositionSample(t));
                    Gizmos.DrawLine(last, current);

                    if (_showForwardVectors)
                    {
                        Gizmos.color = Color.Lerp(_splineColor, Color.blue, 0.25f);
                        Gizmos.DrawLine(current, current + _bezier.ForwardSample(t));
                        Gizmos.color = _splineColor;
                    }

                    last = current;
                }
                if (_continuous)
                {
                    var current = transform.TransformPoint(_bezier.PositionSample(0.0f));
                    Gizmos.DrawLine(last, current);
                }

                Gizmos.color = _tangentColor;
                foreach (var cp in _bezier.ControlPoints)
                {
                    Gizmos.DrawLine(transform.TransformPoint(cp.Point), transform.TransformPoint(cp.InTangent));
                    Gizmos.DrawLine(transform.TransformPoint(cp.Point), transform.TransformPoint(cp.OutTangent));
                }

                Gizmos.color = _handleColor;
                float pntRadius = 0.0f;
                float pntCount = 0.0f;
                foreach (var cp in _bezier.ControlPoints)
                {
                    pntRadius += (transform.TransformPoint(cp.InTangent) - transform.TransformPoint(cp.OutTangent)).magnitude;
                    pntCount += 1.0f;
                }
                pntRadius /= pntCount;
                pntRadius *= 0.02f;

                foreach (var cp in _bezier.ControlPoints)
                {
                    Gizmos.DrawSphere(transform.TransformPoint(cp.Point), pntRadius);
                    Gizmos.DrawSphere(transform.TransformPoint(cp.InTangent), pntRadius * 0.75f);
                    Gizmos.DrawSphere(transform.TransformPoint(cp.OutTangent), pntRadius * 0.75f);
                }
            }
        }
    }
}