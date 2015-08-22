using UnityEngine;
using UnityEditor;

using System.Collections;

using Procedural;

[CustomEditor(typeof(BezierComponent), true)]
public class BezierComponentEditor : Editor 
{
	public override void OnInspectorGUI() 
	{
		//DrawDefaultInspector();

		if( GUILayout.Button("Update") )
		{
			((BezierComponent)target).UpdateBezier();
		}

		base.OnInspectorGUI();
	}
}
