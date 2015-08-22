using UnityEngine;

using System;
using System.Collections.Generic;

namespace Procedural
{
	public static class MeshExt
	{
		public static Vector3 FaceNorm(this Mesh mesh, int triStartIdx)
		{
			var v1 = mesh.vertices[mesh.triangles[triStartIdx]];
			var v2 = mesh.vertices[mesh.triangles[triStartIdx + 1]];
			var v3 = mesh.vertices[mesh.triangles[triStartIdx + 2]];

			return Vector3.Cross(
				v2 - v1,
				v3 - v1
			).normalized;
		}

		public static Vector3 FaceCenter(this Mesh mesh, int triStartIdx)
		{
			var v1 = mesh.vertices[mesh.triangles[triStartIdx]];
			var v2 = mesh.vertices[mesh.triangles[triStartIdx + 1]];
			var v3 = mesh.vertices[mesh.triangles[triStartIdx + 2]];

			return (v1 + v2 + v3) * 1.0f/3.0f;
		}
	}
}