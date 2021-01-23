#pragma once

// Return a new point in the coordinate system typically used by OpenGL applications.
inline P3D GameToGLPoint(const P3D& gamePoint)
{
	P3D newPoint;
	newPoint.x = gamePoint.x * -1.0f;
	newPoint.y = gamePoint.z;
	newPoint.z = gamePoint.y;
	return newPoint;
}

// Return a new point in the game's coordinate system from an OpenGL point.
inline static P3D GLToGamePoint(const P3D& glPoint)
{
	P3D newPoint;
	newPoint.x = glPoint.x * -1.0f;
	newPoint.z = glPoint.y;
	newPoint.y = glPoint.z;

	return newPoint;
}
