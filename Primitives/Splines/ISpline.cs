using UnityEngine;

using System;
using System.Collections.Generic;

public interface ISpline
{
    Vector3 PositionSample(float t);
    Vector3 ForwardSample(float t);
    bool Closed { get; }
}

