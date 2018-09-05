using UnityEngine;
using UnityEditor;
using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Linq;


namespace Procedural.Test {
  [TestFixture]
  internal class MeshExt_Test {
    private Mesh _mesh;

    [SetUp]
    public void Setup() {
      _mesh = new Mesh();
      _mesh.vertices = new [] {
        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 1.0f)
      };

      _mesh.triangles = new [] {
        2, 1, 0
      };
    }

    [TearDown]
    public void Teardown() {
      Mesh.DestroyImmediate(_mesh);
    }

    //[Test]
    //public void FaceNormVerification() {
    //  UAssert.Near(Vector3.up, _mesh.FaceNorm(0), 0.001f);
    //}

    //[Test]
    //public void FaceCenterVerification() {
    //  UAssert.Near(new Vector3(2.0f / 3.0f, 0.0f, 1.0f / 3.0f), _mesh.FaceCenter(0), 0.001f);
    //}

    [Test]
    public void WhatPowerOfTwoVerification() {
      Assert.AreEqual(0, MathExt.WhatPowerOfTwo(1));
      Assert.AreEqual(1, MathExt.WhatPowerOfTwo(2));
      Assert.AreEqual(2, MathExt.WhatPowerOfTwo(4));
      Assert.AreEqual(3, MathExt.WhatPowerOfTwo(8));
      Assert.AreEqual(4, MathExt.WhatPowerOfTwo(16));
      Assert.AreEqual(5, MathExt.WhatPowerOfTwo(32));
      Assert.AreEqual(6, MathExt.WhatPowerOfTwo(64));
      Assert.AreEqual(7, MathExt.WhatPowerOfTwo(128));
      Assert.AreEqual(8, MathExt.WhatPowerOfTwo(256));
      Assert.AreEqual(9, MathExt.WhatPowerOfTwo(512));
      Assert.AreEqual(10, MathExt.WhatPowerOfTwo(1024));
      Assert.AreEqual(11, MathExt.WhatPowerOfTwo(2048));
      Assert.AreEqual(12, MathExt.WhatPowerOfTwo(4096));
      Assert.AreEqual(13, MathExt.WhatPowerOfTwo(8192));
    }

  }
}