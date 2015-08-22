using UnityEngine;

using System;
using System.Collections.Generic;


namespace Procedural
{
	[ExecuteInEditMode]
	public class LayeredPlateau : GeneratedMeshObject 
	{
		public int randomSeed = 0;
		public int layerCount = 3;
		public int radiusSegments = 24;
		public int heightSegments = 2;

		public float baseRadiusMin = 10.0f;
		public float baseRadiusMax = 12.0f;

		public float scaleDownMin = 0.8f;
		public float scaleDownMax = 0.8f;

		public float layerHeightMin = 2.0f;
		public float layerHeightMax = 5.0f;

		public float centerJitterAmount = 0.1f;
		public float waviness = 0.25f;

		private Bezier GenerateBaseBezier(float size, MersenneTwister rand)
		{
			var centerJitterX = Mathf.Lerp(-1.0f, 1.0f, size * centerJitterAmount * rand.NextSinglePositive());
			var centerJitterZ = Mathf.Lerp(-1.0f, 1.0f, size * centerJitterAmount * rand.NextSinglePositive());
			var centerJitter = new Vector3(centerJitterX, 0.0f, centerJitterZ);
			var cpCount = 32;
			var step = Mathf.PI * -2.0f / (float)cpCount;
			var cps = new Vector3[cpCount];

			for(int i = 0; i < cpCount; ++i)
			{
				var t = (float)i * step;
				var x = Mathf.Sin(t) * size;
				var z = Mathf.Cos(t) * size;
				cps[i].x = Mathf.Lerp(0.0f, x, 1.0f - (waviness * rand.NextSinglePositive()));
				cps[i].z = Mathf.Lerp(0.0f, z, 1.0f - (waviness * rand.NextSinglePositive()));
				cps[i] += centerJitter;
			}

			return Bezier.ConstructSmoothSpline(cps, true);
		}

		protected override void GenerateMesh()
		{
			foreach(Transform child in transform)
				DestroyImmediate(child.gameObject);

	        var rand = new MersenneTwister(randomSeed);

	        var totalHeight = 0.0f;
		    var scaleDown = 1.0f;
		    var combineMeshInstances = new List<CombineInstance>();
	        for(int i = 0; i < layerCount; ++i)
	        {
	        	var baseRadius = Mathf.Lerp(baseRadiusMin, baseRadiusMax, rand.NextSinglePositive()) * scaleDown;
		        var baseBezier = GenerateBaseBezier(baseRadius * scaleDown, rand);

		        var previousTotalHeight = totalHeight;
		        totalHeight += Mathf.Lerp(layerHeightMin, layerHeightMax, rand.NextSinglePositive()) * scaleDown;
		        var heightBezier = Bezier.ConstructSmoothSpline(
		        	new Vector3[]{
		        		Vector3.up * previousTotalHeight, 
		        		Vector3.up * totalHeight
		        	}
		        );	

		        var heightSegs = (uint)heightSegments;
		        var pathSegs = (uint)(radiusSegments * scaleDown);

		        if( heightSegs > 0 && pathSegs > 2 )
		        {
		        	var combineMeshInstance = new CombineInstance();
		        	combineMeshInstance.mesh = Loft.GenerateMesh(
			        	heightBezier, 
			        	baseBezier, 
			        	heightSegs,
			        	pathSegs
			        );
			        combineMeshInstances.Add(combineMeshInstance);

                    var topCap = new CombineInstance();
			        topCap.mesh = baseBezier.Triangulate(pathSegs, Vector3.up * totalHeight);
			        combineMeshInstances.Add(topCap);
			    }

		        scaleDown *= Mathf.Lerp(scaleDownMin, scaleDownMax, rand.NextSinglePositive());
	        }

	        var meshFilter = GetComponent<MeshFilter>();
	        if( meshFilter.sharedMesh )
	        	DestroyImmediate(meshFilter.sharedMesh);

	        meshFilter.sharedMesh = new Mesh();
	        meshFilter.sharedMesh.CombineMeshes(combineMeshInstances.ToArray(), true, false);
	        
	        foreach(var combineInstance in combineMeshInstances)
	        	DestroyImmediate(combineInstance.mesh);
		}

		public override int GetHashCode()
		{
			var hash = layerCount + randomSeed + radiusSegments + heightSegments;
			hash ^= BitConverter.ToInt32(BitConverter.GetBytes(centerJitterAmount), 0);
			hash ^= BitConverter.ToInt32(BitConverter.GetBytes(waviness), 0);
			hash ^= BitConverter.ToInt32(BitConverter.GetBytes(scaleDownMin), 0);
			hash ^= BitConverter.ToInt32(BitConverter.GetBytes(scaleDownMax), 0);
			hash ^= BitConverter.ToInt32(BitConverter.GetBytes(baseRadiusMin), 0);
			hash ^= BitConverter.ToInt32(BitConverter.GetBytes(baseRadiusMax), 0);
			hash ^= BitConverter.ToInt32(BitConverter.GetBytes(layerHeightMin), 0);
			hash ^= BitConverter.ToInt32(BitConverter.GetBytes(layerHeightMax), 0);
			return hash;
		}
	}
}