using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Procedural
{
    public class Loft
    {
        public ISpline Path { get; private set; }
        public ISpline Shape { get; private set; }

        public bool StartCap { get; set; }
        public bool EndCap { get; set; }

        public float Banking { get; set; }

        public static Mesh GenerateMesh(ISpline path, ISpline shape, uint pathSegments, uint shapeSegments)
        {
            return new Loft(path, shape).GenerateMesh(pathSegments, shapeSegments);
        }


        public Loft(ISpline path, ISpline shape)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            if (shape == null)
                throw new ArgumentNullException("shape");
            
            Path = path;
            Shape = shape;
        }

        public Mesh GenerateMesh(uint pathSegments, uint shapeSegments)
        {
            if( pathSegments < 1 )
                throw new ArgumentException("pathSegments must be at least 1");
            if( shapeSegments < 2 )
                throw new ArgumentException("shapeSegments must be at least 2");

            Mesh mesh = new Mesh();

            var vertCount = (pathSegments+1) * (shapeSegments+1);

            Vector3[] verts = new Vector3[vertCount];
            Vector3[] norms = new Vector3[vertCount];
            Vector2[] uvs = new Vector2[vertCount];

            Func<uint, uint, int> uvToVertIdx = (shapeSegment, pathSegment) => {
                return (int)((pathSegment * shapeSegments) + shapeSegment);
            };

            var triangleCount = pathSegments * shapeSegments * 6;
            int[] tris = new int[triangleCount];

            float pathStep = 1.0f / (float)pathSegments;
            float shapeStep = 1.0f / (float)(shapeSegments-1);

            for(uint pathSeg = 0; pathSeg < pathSegments+1; ++pathSeg)
            {
                var pathT = pathStep * (float)pathSeg;
                var pathPnt = Path.PositionSample(pathT);
                var pathDir = Path.ForwardSample(pathT);
                var pathRot = Quaternion.FromToRotation(Vector3.up, pathDir);

                if( pathDir == -Vector3.up )
                {
                    // Compensate for the gimbal lock when 
                    // pathDir is polar opposite to up
                    pathRot *= Quaternion.AngleAxis(180.0f, Vector3.up);
                }
                
                for(uint shapeSeg = 0; shapeSeg < shapeSegments+1; ++shapeSeg)
                {
                    var shapeT = shapeStep * (float)shapeSeg;
                    var shapePnt = Shape.PositionSample(shapeT);
                    var vertIdx = uvToVertIdx(shapeSeg, pathSeg);
                    var shapePntRotated = pathRot * shapePnt;
                    verts[vertIdx] = pathPnt + shapePntRotated;
                    uvs[vertIdx].x = shapeT;
                    uvs[vertIdx].y = pathT;
                }
            }

            var triIdx = 0;
            for(uint pathSeg = 0; pathSeg < pathSegments; ++pathSeg)
            {
                for(uint shapeSeg = 0; shapeSeg < shapeSegments; ++shapeSeg)
                {
                    var nextShapeSeg = (shapeSeg + 1) % shapeSegments;
                    var vert1 = uvToVertIdx(shapeSeg, pathSeg);
                    var vert2 = uvToVertIdx(shapeSeg, pathSeg + 1);
                    var vert3 = uvToVertIdx(nextShapeSeg, pathSeg);
                    var vert4 = vert3;
                    var vert5 = vert2;
                    var vert6 = uvToVertIdx(nextShapeSeg, pathSeg + 1);

                    tris[triIdx++] = vert1;
                    tris[triIdx++] = vert2;
                    tris[triIdx++] = vert3;
                    tris[triIdx++] = vert4;
                    tris[triIdx++] = vert5;
                    tris[triIdx++] = vert6;

                    Vector3 faceNormal1 = Vector3.Cross(verts[vert1] - verts[vert2], verts[vert1] - verts[vert3]).normalized; 
                    Vector3 faceNormal2 = Vector3.Cross(verts[vert4] - verts[vert5], verts[vert4] - verts[vert6]).normalized; 

                    norms[vert1] += faceNormal1;
                    norms[vert2] += faceNormal1;
                    norms[vert3] += faceNormal1;
                    norms[vert4] += faceNormal2;
                    norms[vert5] += faceNormal2;
                    norms[vert6] += faceNormal2;
                }
            }


            // If the path is closed, make sure there is no normal crease at the loop
            if (Path.Closed)
            {
                var lastPathShapeBaseIdx = shapeSegments * pathSegments;
                var firstSegmentNorms = new Vector3[shapeSegments];
                Array.Copy(norms, firstSegmentNorms, shapeSegments);

                for (var shapeSeg = 0u; shapeSeg < shapeSegments; ++shapeSeg)
                {
                    norms[shapeSeg] += norms[lastPathShapeBaseIdx + shapeSeg];
                    norms[lastPathShapeBaseIdx + shapeSeg] += firstSegmentNorms[shapeSeg];
                }
            }

            // If the shape is closed, make sure there is no normal crease at the loop
            if(Shape.Closed)
            {
                var firstSegmentNorms = new Vector3[pathSegments];
                Array.Copy(norms, firstSegmentNorms, pathSegments);

                for (var pathSeg = 0u; pathSeg < pathSegments+1; ++pathSeg)
                {
                    var shapeVertsStart = pathSeg * shapeSegments;
                    var shapeVertsEnd = shapeVertsStart + shapeSegments - 1;
                    var startNorm = norms[shapeVertsStart];
                    norms[shapeVertsStart] += norms[shapeVertsEnd];
                    norms[shapeVertsEnd] += startNorm;
                }
            }

            for(int n = 0; n < norms.Length; ++n)
            {
                norms[n] = norms[n].normalized;
            }

            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.uv2 = uvs;
            mesh.uv2 = uvs;
            mesh.normals = norms;
            mesh.triangles = tris;

            return mesh;
        }
    }
}