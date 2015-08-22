using UnityEngine;

using System;
using System.Collections.Generic;

namespace Procedural
{
	public abstract class GeneratedMeshObject : MonoBehaviour 
	{
		private int _lastHash = 0;

        protected virtual void Start()
        {
            _lastHash = 0;

        	if( !GetComponent<Renderer>() )
        		gameObject.AddComponent<MeshRenderer>();

        	if( !GetComponent<MeshFilter>() )
        		gameObject.AddComponent<MeshFilter>();

        	CheckForDirtyMesh();
        }

        protected virtual void Update()
        {
        	CheckForDirtyMesh();
        }

        private void CheckForDirtyMesh()
        {
        	var hash = GetHashCode();
        	if( hash != _lastHash )
        	{
		    	_lastHash = hash;
		        GenerateMesh();
		    }
        }

        protected abstract void GenerateMesh();

        public override int GetHashCode()
        {
        	throw new NotImplementedException("GeneratedMeshObjects require a custom GetHashCode");
        }
	}
}