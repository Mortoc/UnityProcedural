using UnityEngine;
using UnityTest;
using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Procedural.Test
{
    [TestFixture]
    [Category("Procedural")]
    internal class Bezier_Test
    {
        private Bezier _simpleArc = new Bezier(new Bezier.ControlPoint[]{
            new Bezier.ControlPoint(new Vector3(0.0f, 0.0f, 0.0f)),
            new Bezier.ControlPoint(new Vector3(0.5f, 1.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f), new Vector3(1.0f, 1.0f, 0.0f)),
            new Bezier.ControlPoint(new Vector3(1.0f, 0.0f, 0.0f))
        });

        [Test]
        public void BeziersSampleThroughControlPoints()
        {
            var cpTStep = 1.0f / (float)(_simpleArc.ControlPoints.Count() - 1);
            var currentT = 0.0f;

            // Sample at each control point
            foreach (var cp in _simpleArc.ControlPoints)
            {
                UAssert.Near(_simpleArc.PositionSample(currentT), cp.Point, 0.0001f);
                currentT += cpTStep;
            }
        }

        [Test]
        public void BeziersInterpolateSamples()
        {
            var cps = new List<Bezier.ControlPoint>(_simpleArc.ControlPoints);

            // Samples between t=[0.1, 0.499] and t=[0.501, .9] should be y > to the non-interpolated linear position
            for (float t = 0.1f; t < 0.499f; t += 0.01f)
            {
                Vector3 linearPosition = Vector3.Lerp(cps[0].Point, cps[1].Point, t);
                Vector3 interpolatedPosition = _simpleArc.PositionSample(t);
                Assert.LessOrEqual(linearPosition.y, interpolatedPosition.y);
                Assert.LessOrEqual(interpolatedPosition.y, 1.0f);
            }
            for (float t = 0.501f; t < 0.9f; t += 0.01f)
            {
                Vector3 linearPosition = Vector3.Lerp(cps[1].Point, cps[2].Point, t);
                Vector3 interpolatedPosition = _simpleArc.PositionSample(t);
                Assert.LessOrEqual(linearPosition.y, interpolatedPosition.y);
                Assert.LessOrEqual(interpolatedPosition.y, 1.0f);
            }
        }

        [Test]
        public void ConstructSmoothSplineVerification()
        {
            var points = new Vector3[]{
                new Vector3(0.0f, 0.0f, 0.0f),
                new Vector3(1.0f, 0.0f, 0.0f),
                new Vector3(0.0f, 1.0f, 0.0f),
                new Vector3(0.0f, 0.0f, 1.0f),
                new Vector3(1.0f, 1.0f, 0.0f),
                new Vector3(0.0f, 1.0f, 1.0f),
                new Vector3(1.0f, 0.0f, 1.0f),
                new Vector3(1.0f, 1.0f, 1.0f)
            };

            var bezier = Bezier.ConstructSmoothSpline(points);

            // Check that the end points are sane
            UAssert.Near(bezier.PositionSample(0.0f), points[0], 0.0001f);
            UAssert.Near(bezier.PositionSample(1.0f), points[points.Length - 1], 0.0001f);

            // Check that there are no crazy values
            Vector3 pointCenter = MathExt.Average(points);
            for (float t = 0.0f; t < 1.0f; t += 0.01f)
            {
                UAssert.Near(bezier.PositionSample(t), pointCenter, 1.0f);
            }

            // Check that all the spline tangents are locked
            var controlPoints = new List<Bezier.ControlPoint>(bezier.ControlPoints);
            for (int i = 1; i < controlPoints.Count - 1; ++i)
            {
                var controlPoint = controlPoints[i];
                var relativeInTan = controlPoint.InTangent - controlPoint.Point;
                var relativeOutTan = controlPoint.OutTangent - controlPoint.Point;

                UAssert.Near(relativeInTan, relativeOutTan * -1.0f, 0.0001f);
            }
        }


        [Test]
        public void ConstructSmoothClosedSplineVerification()
        {
            var points = new Vector3[]{
                new Vector3(0.0f, 0.0f, 0.0f),
                new Vector3(1.0f, 0.0f, 0.0f),
                new Vector3(0.0f, 1.0f, 0.0f),
                new Vector3(0.0f, 0.0f, 1.0f),
                new Vector3(1.0f, 1.0f, 0.0f),
                new Vector3(0.0f, 1.0f, 1.0f),
                new Vector3(1.0f, 0.0f, 1.0f),
            };

            var bezier = Bezier.ConstructSmoothSpline(points, true);

            // Check that the end points are sane
            UAssert.Near(bezier.PositionSample(0.0f), points[0], 0.0001f);
            UAssert.Near(bezier.PositionSample(1.0f), points[0], 0.0001f);

            // Check that there are no crazy values
            Vector3 pointCenter = MathExt.Average(points);
            for (float t = 0.0f; t < 1.0f; t += 0.01f)
            {
                UAssert.Near(bezier.PositionSample(t), pointCenter, 1.0f);
            }

            // Check that all the spline tangents are locked
            var controlPoints = new List<Bezier.ControlPoint>(bezier.ControlPoints);
            for (int i = 1; i < controlPoints.Count - 1; ++i)
            {
                var controlPoint = controlPoints[i];
                var relativeInTan = controlPoint.InTangent - controlPoint.Point;
                var relativeOutTan = controlPoint.OutTangent - controlPoint.Point;

                UAssert.Near(relativeInTan, relativeOutTan * -1.0f, 0.0001f);
            }
        }

        [Test]
        public void ConstructSmoothStraightLineSpline()
        {
            var points = new Vector3[]{
                new Vector3(-300.0f, 0.0f, 237.0f),
                new Vector3(1.0f, 25.0f, -187.0f)
            };

            var bezier = Bezier.ConstructSmoothSpline(points);

            // Check that all points are linear on the x axis
            for (float t = 0.0f; t < 1.0f; t += 0.05f)
            {
                UAssert.NearLineSegment(points[0], points[1], bezier.PositionSample(t), 0.001f);
            }
        }

        [Test]
        public void ForwardSampleVerificationOnAStraightLine()
        {
            var points = new Vector3[]{
                new Vector3(-300.0f, 100.0f, 37.5f),
                new Vector3(1.2134f, -35.0f, 175.0f)
            };

            var bezier = Bezier.ConstructSmoothSpline(points);
            var pntsDirection = (points[1] - points[0]).normalized;

            for (float t = 0.0f; t < 1.0f; t += 0.05f)
            {
                UAssert.Near(Vector3.Dot(bezier.ForwardSample(t), pntsDirection), 1.0f, 0.001f);
            }
        }


        [Test]
        public void ForwardSampleVerificationOnACircle()
        {
            var points = new Vector3[]{
                new Vector3(1.0f, 0.0f, 0.0f),
                new Vector3(0.0f, 1.0f, 0.0f),
                new Vector3(-1.0f, 0.0f, 0.0f),
                new Vector3(0.0f, -1.0f, 0.0f),
            };

            var bezier = Bezier.ConstructSmoothSpline(points);

            for (float t = 0.0f; t < 1.0f; t += 0.05f)
            {
                var expectedAngle = Mathf.Cos(t);
                UAssert.Near(Vector3.Dot(bezier.ForwardSample(t), Vector3.up), expectedAngle, 0.1f);
            }
        }


        [Test]
        public void ForwardDoesntFlipOnClosedSplines()
        {
            var points = new Vector3[]{
                new Vector3(10.0f, 0.0f, 0.0f),
                new Vector3(0.0f, 10.0f, 0.0f),
                new Vector3(-10.0f, 0.0f, 0.0f),
                new Vector3(0.0f, -10.0f, 0.0f),
            };

            var bezier = Bezier.ConstructSmoothSpline(points, true);

            var dir1 = bezier.ForwardSample(0.0f);
            var dir2 = bezier.ForwardSample(1.0f);

            UAssert.Near(dir1, dir2, 0.0001f);
        }

        [Test]
        public void ClosedBezierVerification()
        {

            var points = new Vector3[]{
                new Vector3(10.0f, 0.0f, 0.0f),
                new Vector3(0.0f, 10.0f, 0.0f),
                new Vector3(-10.0f, 0.0f, 0.0f),
                new Vector3(0.0f, -10.0f, 0.0f),
            };

            var closedBezier = Bezier.ConstructSmoothSpline(points, true);
            var openBezier = Bezier.ConstructSmoothSpline(points, false);

            Assert.IsTrue(closedBezier.Closed);
            Assert.IsFalse(openBezier.Closed);
        }
    }
}