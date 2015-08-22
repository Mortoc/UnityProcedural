using UnityEngine;
using System.Collections;


namespace Procedural
{
	public class FaceNormVisualizer : MonoBehaviour 
	{
		public Color _color = Color.red;

		void OnDrawGizmos()
		{
			var meshFilter = GetComponent<MeshFilter>();

			if( meshFilter && meshFilter.sharedMesh )
			{
				Gizmos.color = _color;
				var mesh = meshFilter.sharedMesh;

				for(int tri = 0; tri < mesh.triangles.Length; tri += 3)
				{
					var center = transform.TransformPoint(mesh.FaceCenter(tri));
					var norm = mesh.FaceNorm(tri);

					Gizmos.DrawLine(center, center + norm);
				}
			}
		}
	}
}