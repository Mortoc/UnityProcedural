using UnityEngine;
using System;
using System.Collections.Generic;


// From http://wiki.unity3d.com/index.php?title=3d_Math_functions
public static class MathExt
{
	//Find the line of intersection between two planes.	The planes are defined by a normal and a point on that plane.
	//The outputs are a point on the line and a vector which indicates it's direction. If the planes are not parallel, 
	//the function outputs true, otherwise false.
	public static bool PlanePlaneIntersection(out Vector3 linePoint, out Vector3 lineVec, Vector3 plane1Normal, Vector3 plane1Position, Vector3 plane2Normal, Vector3 plane2Position)
	{
		
		linePoint = Vector3.zero;
		lineVec = Vector3.zero;
		
		//We can get the direction of the line of intersection of the two planes by calculating the 
		//cross product of the normals of the two planes. Note that this is just a direction and the line
		//is not fixed in space yet. We need a point for that to go with the line vector.
		lineVec = Vector3.Cross(plane1Normal, plane2Normal);
		
		//Next is to calculate a point on the line to fix it's position in space. This is done by finding a vector from
		//the plane2 location, moving parallel to it's plane, and intersecting plane1. To prevent rounding
		//errors, this vector also has to be perpendicular to lineDirection. To get this vector, calculate
		//the cross product of the normal of plane2 and the lineDirection.		
		Vector3 ldir = Vector3.Cross(plane2Normal, lineVec);		
		
		float denominator = Vector3.Dot(plane1Normal, ldir);
		
		//Prevent divide by zero and rounding errors by requiring about 5 degrees angle between the planes.
		if(Mathf.Abs(denominator) > 0.006f)
		{
			
			Vector3 plane1ToPlane2 = plane1Position - plane2Position;
			float t = Vector3.Dot(plane1Normal, plane1ToPlane2) / denominator;
			linePoint = plane2Position + t * ldir;
			
			return true;
		}
		
		//output not valid
		return false;
	}	
	
	//Get the intersection between a line and a plane. 
	//If the line and plane are not parallel, the function outputs true, otherwise false.
	public static bool LinePlaneIntersection(out Vector3 intersection, Vector3 linePoint, Vector3 lineVec, Vector3 planeNormal, Vector3 planePoint)
	{
		
		float length;
		float dotNumerator;
		float dotDenominator;
		Vector3 vector;
		intersection = Vector3.zero;
		
		//calculate the distance between the linePoint and the line-plane intersection point
		dotNumerator = Vector3.Dot((planePoint - linePoint), planeNormal);
		dotDenominator = Vector3.Dot(lineVec, planeNormal);
		
		//line and plane are not parallel
		if(dotDenominator != 0.0f)
		{
			length =  dotNumerator / dotDenominator;
			
			//create a vector from the linePoint to the intersection point
			vector = SetVectorLength(lineVec, length);
			
			//get the coordinates of the line-plane intersection point
			intersection = linePoint + vector;	
			
			return true;	
		}
		
		//output not valid
		return false;
	}
	
	//Calculate the intersection point of two lines. Returns true if lines intersect, otherwise false.
	//Note that in 3d, two lines do not intersect most of the time. So if the two lines are not in the 
	//same plane, use ClosestPointsOnTwoLines() instead.
	public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
	{
		
		intersection = Vector3.zero;
		
		Vector3 lineVec3 = linePoint2 - linePoint1;
		Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
		Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);
		
		float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);
		
		//Lines are not coplanar. Take into account rounding errors.
		if((planarFactor >= 0.00001f) || (planarFactor <= -0.00001f))
		{
			
			return false;
		}
		
		//Note: sqrMagnitude does x*x+y*y+z*z on the input vector.
		float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
		
		if((s >= 0.0f) && (s <= 1.0f))
		{
			intersection = linePoint1 + (lineVec1 * s);
			return true;
		}

		return false;       
	}
	
	//Two non-parallel lines which may or may not touch each other have a point on each line which are closest
	//to each other. This function finds those two points. If the lines are not parallel, the function 
	//outputs true, otherwise false.
	public static bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
	{
		
		closestPointLine1 = Vector3.zero;
		closestPointLine2 = Vector3.zero;
		
		float a = Vector3.Dot(lineVec1, lineVec1);
		float b = Vector3.Dot(lineVec1, lineVec2);
		float e = Vector3.Dot(lineVec2, lineVec2);
		
		float d = a*e - b*b;
		
		//lines are not parallel
		if(d != 0.0f)
		{
			float recpD = 1.0f / d;
			Vector3 r = linePoint1 - linePoint2;
			float c = Vector3.Dot(lineVec1, r);
			float f = Vector3.Dot(lineVec2, r);
			
			float s = (b*f - c*e) * recpD;
			float t = (a*f - c*b) * recpD;
			
			closestPointLine1 = linePoint1 + lineVec1 * s;
			closestPointLine2 = linePoint2 + lineVec2 * t;
			
			return true;
		}

		return false;
	}	
	
	//This function returns a point which is a projection from a point to a line.
	//The line is regarded infinite. If the line is finite, use ProjectPointOnLineSegment() instead.
	public static Vector3 ProjectPointOnLine(Vector3 linePoint, Vector3 lineVec, Vector3 point)
	{	
		//get vector from point on line to point in space
		Vector3 linePointToPoint = point - linePoint;
		float t = Vector3.Dot(linePointToPoint, lineVec);
		return linePoint + lineVec * t;
	}
	
	//This function returns a point which is a projection from a point to a line segment.
	//If the projected point lies outside of the line segment, the projected point will 
	//be clamped to the appropriate line edge.
	//If the line is infinite instead of a segment, use ProjectPointOnLine() instead.
	public static Vector3 ProjectPointOnLineSegment(Vector3 linePoint1, Vector3 linePoint2, Vector3 point)
	{
		Vector3 vector = linePoint2 - linePoint1;
		Vector3 projectedPoint = ProjectPointOnLine(linePoint1, vector.normalized, point);
		int side = PointOnWhichSideOfLineSegment(linePoint1, linePoint2, projectedPoint);
		
		//The projected point is on the line segment
		if(side == 0)
			return projectedPoint;

		if(side == 1)	
			return linePoint1;
		
		if(side == 2)
			return linePoint2;
		
		//output is invalid
		return Vector3.zero;
	}	
	
	//This function returns a point which is a projection from a point to a plane.
	public static Vector3 ProjectPointOnPlane(Vector3 planeNormal, Vector3 planePoint, Vector3 point)
	{	
		float distance;
		Vector3 translationVector;
		
		//First calculate the distance from the point to the plane:
		distance = SignedDistancePlanePoint(planeNormal, planePoint, point);
		
		//Reverse the sign of the distance
		distance *= -1;
		
		//Get a translation vector
		translationVector = SetVectorLength(planeNormal, distance);
		
		//Translate the point to form a projection
		return point + translationVector;
	}	
	
	//Projects a vector onto a plane. The output is not normalized.
	public static Vector3 ProjectVectorOnPlane(Vector3 planeNormal, Vector3 vector)
	{	
		return vector - (Vector3.Dot(vector, planeNormal) * planeNormal);
	}
	
	//Get the shortest distance between a point and a plane. The output is signed so it holds information
	//as to which side of the plane normal the point is.
	public static float SignedDistancePlanePoint(Vector3 planeNormal, Vector3 planePoint, Vector3 point)
	{	
		return Vector3.Dot(planeNormal, (point - planePoint));
	}	

	//Convert a plane defined by 3 points to a plane defined by a vector and a point. 
	//The plane point is the middle of the triangle defined by the 3 points.
	public static void PlaneFrom3Points(out Vector3 planeNormal, out Vector3 planePoint, Vector3 pointA, Vector3 pointB, Vector3 pointC)
	{
		
		planeNormal = Vector3.zero;
		planePoint = Vector3.zero;
		
		//Make two vectors from the 3 input points, originating from point A
		Vector3 AB = pointB - pointA;
		Vector3 AC = pointC - pointA;
		
		//Calculate the normal
		planeNormal = Vector3.Normalize(Vector3.Cross(AB, AC));
		
		//Get the points in the middle AB and AC
		Vector3 middleAB = pointA + (AB / 2.0f);
		Vector3 middleAC = pointA + (AC / 2.0f);
		
		//Get vectors from the middle of AB and AC to the point which is not on that line.
		Vector3 middleABtoC = pointC - middleAB;
		Vector3 middleACtoB = pointB - middleAC;
		
		//Calculate the intersection between the two lines. This will be the center 
		//of the triangle defined by the 3 points.
		//We could use LineLineIntersection instead of ClosestPointsOnTwoLines but due to rounding errors 
		//this sometimes doesn't work.
		Vector3 temp;
		ClosestPointsOnTwoLines(out planePoint, out temp, middleAB, middleABtoC, middleAC, middleACtoB);
	}

	//This function finds out on which side of a line segment the point is located.
	//The point is assumed to be on a line created by linePoint1 and linePoint2. If the point is not on
	//the line segment, project it on the line using ProjectPointOnLine() first.
	//Returns 0 if point is on the line segment.
	//Returns 1 if point is outside of the line segment and located on the side of linePoint1.
	//Returns 2 if point is outside of the line segment and located on the side of linePoint2.
	public static int PointOnWhichSideOfLineSegment(Vector3 linePoint1, Vector3 linePoint2, Vector3 point)
	{
		Vector3 lineVec = linePoint2 - linePoint1;
		Vector3 pointVec = point - linePoint1;
		
		float dot = Vector3.Dot(pointVec, lineVec);
		
		//point is on side of linePoint2, compared to linePoint1
		if(dot > 0)
		{
			//point is on the line segment
			if(pointVec.magnitude <= lineVec.magnitude)
				return 0;
			
			//point is not on the line segment and it is on the side of linePoint2
			else
				return 2;
		}
		
		//Point is not on side of linePoint2, compared to linePoint1.
		//Point is not on the line segment and it is on the side of linePoint1.
		return 1;
	}

	//create a vector of direction "vector" with length "size"
	public static Vector3 SetVectorLength(Vector3 vector, float size)
	{
		//normalize the vector
		Vector3 vectorNormalized = Vector3.Normalize(vector);
		
		//scale the vector
		return vectorNormalized *= size;
	}


    public static float Hermite(float start, float end, float value)
    {
        return Mathf.Lerp(start, end, value * value * (3.0f - 2.0f * value));
    }

    public static float Sinerp(float start, float end, float value)
    {
        return Mathf.Lerp(start, end, Mathf.Sin(value * Mathf.PI * 0.5f));
    }

    public static float Coserp(float start, float end, float value)
    {
        return Mathf.Lerp(start, end, 1.0f - Mathf.Cos(value * Mathf.PI * 0.5f));
    }

    public static float Berp(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        value = (Mathf.Sin(value * Mathf.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) * (1f + (1.2f * (1f - value)));
        return start + (end - start) * value;
    }

    public static float SmoothStep(float x, float min, float max)
    {
        x = Mathf.Clamp(x, min, max);
        float v1 = (x - min) / (max - min);
        float v2 = (x - min) / (max - min);
        return -2 * v1 * v1 * v1 + 3 * v2 * v2;
    }

    public static float Lerp(float start, float end, float value)
    {
        return ((1.0f - value) * start) + (value * end);
    }

    public static Vector3 NearestPoint(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
    {
        Vector3 lineDirection = Vector3.Normalize(lineEnd - lineStart);
        float closestPoint = Vector3.Dot((point - lineStart), lineDirection) / Vector3.Dot(lineDirection, lineDirection);
        return lineStart + (closestPoint * lineDirection);
    }

    public static Vector3 NearestPointStrict(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
    {
        Vector3 fullDirection = lineEnd - lineStart;
        Vector3 lineDirection = Vector3.Normalize(fullDirection);
        float closestPoint = Vector3.Dot((point - lineStart), lineDirection) / Vector3.Dot(lineDirection, lineDirection);
        return lineStart + (Mathf.Clamp(closestPoint, 0.0f, Vector3.Magnitude(fullDirection)) * lineDirection);
    }
    public static float Bounce(float x)
    {
        return Mathf.Abs(Mathf.Sin(6.28f * (x + 1f) * (x + 1f)) * (1f - x));
    }

    // test for value that is near specified float (due to floating point inprecision)
    // all thanks to Opless for this!
    public static bool Approx(float val, float about, float range)
    {
        return ((Mathf.Abs(val - about) < range));
    }

    // test if a Vector3 is close to another Vector3 (due to floating point inprecision)
    // compares the square of the distance to the square of the range as this 
    // avoids calculating a square root which is much slower than squaring the range
    public static bool Approx(Vector3 val, Vector3 about, float range)
    {
        return ((val - about).sqrMagnitude < range * range);
    }

    /*
      * CLerp - Circular Lerp - is like lerp but handles the wraparound from 0 to 360.
      * This is useful when interpolating eulerAngles and the object
      * crosses the 0/360 boundary.  The standard Lerp function causes the object
      * to rotate in the wrong direction and looks stupid. Clerp fixes that.
      */
    public static float Clerp(float start, float end, float value)
    {
        float min = 0.0f;
        float max = 360.0f;
        float half = Mathf.Abs((max - min) / 2.0f);//half the distance between min and max
        float retval = 0.0f;
        float diff = 0.0f;

        if ((end - start) < -half)
        {
            diff = ((max - start) + end) * value;
            retval = start + diff;
        }
        else if ((end - start) > half)
        {
            diff = -((max - end) + start) * value;
            retval = start + diff;
        }
        else retval = start + (end - start) * value;

        // Debug.Log("Start: "  + start + "   End: " + end + "  Value: " + value + "  Half: " + half + "  Diff: " + diff + "  Retval: " + retval);
        return retval;
    }


	public static Vector4 Average(IEnumerable<Vector4> vectors)
	{
		var accum = Vector4.zero;
		var count = 0.0f;
		
		foreach (var vector in vectors) 
		{
			accum += vector;
			count += 1.0f;
		}
		
		return accum / count;
	}

	public static Vector3 Average(IEnumerable<Vector3> vectors)
	{
		var accum = Vector3.zero;
		var count = 0.0f;

		foreach (var vector in vectors) 
		{
			accum += vector;
			count += 1.0f;
		}

		return accum / count;
	}
	
	public static Vector2 Average(IEnumerable<Vector2> vectors)
	{
		var accum = Vector2.zero;
		var count = 0.0f;
		
		foreach (var vector in vectors) 
		{
			accum += vector;
			count += 1.0f;
		}
		
		return accum / count;
	}
	
	
	public static float Average(IEnumerable<float> values)
	{
		var accum = 0.0f;
		var count = 0.0f;
		
		foreach (var val in values) 
		{
			accum += val;
			count += 1.0f;
		}
		
		return accum / count;
	}
}
