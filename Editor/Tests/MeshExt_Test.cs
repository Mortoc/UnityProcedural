using UnityEngine;
using UnityEditor;
using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Linq;


namespace Procedural.Test
{
    [TestFixture]
    internal class MeshExt_Test
    {
        private Mesh _mesh;

        [SetUp]
        public void Setup()
        {
            _mesh = new Mesh();
            _mesh.vertices = new Vector3[]{
                new Vector3(0.0f, 0.0f, 0.0f),
                new Vector3(1.0f, 0.0f, 0.0f),
                new Vector3(1.0f, 0.0f, 1.0f)
            };

            _mesh.triangles = new int[]{
                2, 1, 0
            };
        }

        [TearDown]
        public void Teardown()
        {
            Mesh.DestroyImmediate(_mesh);
        }

        [Test]
        public void FaceNormVerification()
        {
            UAssert.Near(Vector3.up, _mesh.FaceNorm(0), 0.001f);
        }

        [Test]
        public void FaceCenterVerification()
        {
            UAssert.Near(new Vector3(2.0f / 3.0f, 0.0f, 1.0f / 3.0f), _mesh.FaceCenter(0), 0.001f);
        }

    }
}