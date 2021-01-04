#pragma once

#include <float.h>
#include <math.h>
#include <memory>
#include <string>
#include <string_view>

//////////////////////////////////////////////////////////////////////////////////////
// Basic game data types and macros

typedef unsigned int uint;
typedef unsigned char UBYTE;
typedef signed char SBYTE;
typedef unsigned short UWORD;
typedef int BOOL;
typedef unsigned long ULONG;
typedef unsigned long DWORD;
typedef std::int64_t int64;
typedef std::uint64_t uint64;
#ifdef UNICODE
typedef std::wstring tstring;
typedef std::wstring_view tstring_view;
#else
typedef std::string tstring;
typedef std::string_view tstring_view;
#endif

#define countof(array) (sizeof((array)) / sizeof((array)[0]))

#define FlagSet(b, f) ((b) |= (f))
#define FlagClear(b, f) ((b) &= ~(f))
#define FlagIsClear(b, f) (!FlagIsSet(b, f))
#define FlagFlip(b, f) ((b) ^= (f))
#define FlagIsSet(b, f) (((b) & (f)) != 0)

//////////////////////////////////////////////////////////////////////////////////////
// Vectors

struct P3D
{
	float x;
	float y;
	float z;

	inline const P3D& operator -= (const P3D& rhs)
	{
		x -= rhs.x;
		y -= rhs.y;
		z -= rhs.z;

		return *this;
	}

	inline const P3D& operator += (const P3D& rhs)
	{
		x += rhs.x;
		y += rhs.y;
		z += rhs.z;

		return *this;
	}

	inline P3D operator - (const P3D& rhs)
	{
		P3D result;

		result.x = x - rhs.x;
		result.y = y - rhs.y;
		result.z = z - rhs.z;

		return result;
	}

	inline P3D Cross(const P3D& v2)
	{
		P3D result;

		result.x = y * v2.z - z * v2.y;
		result.y = z * v2.x - x * v2.z;
		result.z = x * v2.y - y * v2.x;

		return result;
	}

	inline float Dot(const P3D& v2)
	{
		return x * v2.x + y * v2.y + z * v2.z;
	}

	inline float Length()
	{
		return (float)(sqrt(x * x + y * y + z * z));
	}

	inline P3D Scale(float scale)
	{
		x *= scale;
		y *= scale;
		z *= scale;

		return *this;
	}

	inline P3D Normalize()
	{
		const float length = Length();
		P3D normalized = *this;

		float factor = 0.0f;
		if (length)
		{
			factor = 1 / length;
		}
		else
		{
			factor = 1.0f;
		}

		normalized.x *= factor;
		normalized.y *= factor;
		normalized.z *= factor;

		return normalized;
	}

	bool IsNaN() const
	{
		return (_isnan(x) || _isnan(y) || _isnan(z));
	}

	bool Finite() const
	{
		return (_finite(x) && _finite(y) && _finite(z));
	}
};

#pragma pack (push, 1)
// Optimized 3D vector for network packets.
struct NetP3D
{
	short x, y, z;
};
#pragma pack (pop)

//////////////////////////////////////////////////////////////////////////////////////

struct RGBFloat
{
	float r{};
	float g{};
	float b{};
};

struct VertRGB
{
	unsigned char r;
	unsigned char g;
	unsigned char b;
};

struct UV
{
	float u, v;
};