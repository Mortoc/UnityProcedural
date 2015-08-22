using UnityEngine;
using System;
using System.Collections.Generic;

namespace Procedural
{
    [ExecuteInEditMode]
    public class LoftComponent : GeneratedMeshObject
    {
        public BezierComponent Path;
        public BezierComponent Shape;

        public int pathSegments = 10;
        public int shapeSegments = 16;

        private Loft _loft;


        public override int GetHashCode()
        {
        	if( Path == null || Shape == null )
        		return 0;
        		
            var pathHash = Path.PointsHash() + pathSegments;
            var shapeHash = Shape.PointsHash() + shapeSegments;

            return (int)(pathHash ^ shapeHash);
        }
        
        protected override void GenerateMesh()
        {
            if( _loft == null )
                _loft = new Loft(Path, Shape);            
                
            var meshFilter = GetComponent<MeshFilter>();
            
            if( meshFilter.sharedMesh )
                DestroyImmediate(meshFilter.sharedMesh);

            meshFilter.sharedMesh = _loft.GenerateMesh((uint)pathSegments, (uint)shapeSegments);
        }
    }
}