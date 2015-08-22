using UnityEngine;
using UnityEditor;
using NUnit.Framework;

using System;
using System.Collections.Generic;

namespace Procedural.Test
{
    public static class UAssert
    {
        public static void Near(float val1, float val2, float dist)
        {
            var valDiff = val1 - val2;
            Assert.LessOrEqual(valDiff, dist,
                String.Format("The values {0} and {1} were more than {2} apart.", val1, val2, dist));
        }

        public static void Near(Vector3 pnt1, Vector3 pnt2, float dist)
        {
            var distSqr = dist * dist;
            var pntDistSqr = (pnt1 - pnt2).sqrMagnitude;

            Assert.LessOrEqual(pntDistSqr, distSqr,
                String.Format("The points ({0}, {1}, {2}) and ({3}, {4}, {5}) were more than {6} apart.",
                                pnt1.x.ToString("f3"), pnt1.y.ToString("f3"), pnt1.z.ToString("f3"),
                                pnt2.x.ToString("f3"), pnt2.y.ToString("f3"), pnt2.z.ToString("f3"),
                             dist.ToString("f3")));
        }

        public static void NotNear(Vector3 pnt1, Vector3 pnt2, float dist)
        {
            var distSqr = dist * dist;
            var pntDistSqr = (pnt1 - pnt2).sqrMagnitude;
            Assert.Greater(pntDistSqr, distSqr,
                            String.Format("The points ({0}, {1}, {2}) and ({3}, {4}, {5}) were less than {6} apart.",
                            pnt1.x.ToString("f3"), pnt1.y.ToString("f3"), pnt1.z.ToString("f3"),
                            pnt2.x.ToString("f3"), pnt2.y.ToString("f3"), pnt2.z.ToString("f3"),
                            dist));
        }

        public static void NearLineSegment(Vector3 lineEndA, Vector3 lineEndB, Vector3 pnt, float dist)
        {
            var pntProj = MathExt.ProjectPointOnLineSegment(lineEndA, lineEndB, pnt);
            Assert.LessOrEqual((pntProj - pnt).sqrMagnitude, dist * dist,
                String.Format("The point {0} was more than {1} from the line segment [{2} - {3}].",
                        pnt, dist, lineEndA, lineEndB));
        }
    }
}