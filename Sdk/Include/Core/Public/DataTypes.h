#pragma once

#include <float.h>
#include <math.h>
#include <memory>
#include <string>
#include <string_view>

//////////////////////////////////////////////////////////////////////////////////////
// Basic game data types and macros

#define ADJUSTABLE 1

#define TRUE 1
#define FALSE 0

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
#define FlagIsClearE(b, f) (!FlagIsSetE(b, f))
#define FlagFlip(b, f) ((b) ^= (f))
#define FlagIsSet(b, f) (((b) & (f)) != 0)
#define FlagIsSetE(b, f) (((b) & (f)) == f)

#define PI (3.14159265358979f)

//////////////////////////////////////////////////////////////////////////////////////
// Vectors

struct P4D
{
	P4D() noexcept { }
	explicit P4D(float x, float y, float z, float w) noexcept : x(x), y(y), z(z), w(w) { }

	float x = 0;
	float y = 0;
	float z = 0;
	float w = 0;
};

struct P3D
{
	float x;
	float y;
	float z;

	inline const P3D& operator -= (const P3D& other)
	{
		x -= other.x;
		y -= other.y;
		z -= other.z;

		return *this;
	}

	inline const P3D& operator += (const P3D& other)
	{
		x += other.x;
		y += other.y;
		z += other.z;

		return *this;
	}

	inline const P3D& operator *= (float scale)
	{
		x *= scale;
		y *= scale;
		z *= scale;

		return *this;
	}

	inline P3D operator * (float scale) const
	{
		return Scale(scale);
	}

	inline P3D operator - (const P3D& other) const
	{
		P3D result;
		result.x = x - other.x;
		result.y = y - other.y;
		result.z = z - other.z;

		return result;
	}

	inline P3D operator + (const P3D& other)
	{
		P3D result;
		result.x = x + other.x;
		result.y = y + other.y;
		result.z = z + other.z;

		return result;
	}

	inline P3D Cross(const P3D& other) const
	{
		P3D result;

		result.x = y * other.z - z * other.y;
		result.y = z * other.x - x * other.z;
		result.z = x * other.y - y * other.x;

		return result;
	}

	inline float Dot(const P3D& v2) const
	{
		return x * v2.x + y * v2.y + z * v2.z;
	}

	inline float Length() const
	{
		return (float)(sqrt(x * x + y * y + z * z));
	}

	inline P3D Scale(float scale) const
	{
		P3D scaled;

		scaled.x = x * scale;
		scaled.y = y * scale;
		scaled.z = z * scale;

		return scaled;
	}

	inline P3D Normalize() const
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

	inline bool IsNaN() const
	{
		return (_isnan(x) || _isnan(y) || _isnan(z));
	}

	inline bool Finite() const
	{
		return (_finite(x) && _finite(y) && _finite(z));
	}

	inline float Distance2D(const P3D& other) const
	{
		return ((float)sqrt((other.x - this->x) * (other.x - this->x) + (other.y - this->y) * (other.y - this->y)));
	}

	inline float DistanceSquared2D(const P3D& other) const
	{
		return ((float)((other.x - this->x) * (other.x - this->x) + (other.y - this->y) * (other.y - this->y)));
	}

	inline float Distance3D(const P3D& other) const
	{
		return ((float)sqrt((other.x - this->x) * (other.x - this->x) + (other.y - this->y) * (other.y - this->y) + (other.z - this->z) * (other.z - this->z)));
	}

	inline float DistanceSquared3D(const P3D& other) const
	{
		return ((float)((other.x - this->x) * (other.x - this->x) + (other.y - this->y) * (other.y - this->y) + (other.z - this->z) * (other.z - this->z)));
	}

	inline bool Empty() const 
	{
		return this->x == 0.0f && this->y == 0.0f && this->z == 0.0f;
	}

	inline P4D AsP4D() const
	{
		P4D temp;
		temp.x = this->x;
		temp.y = this->y;
		temp.z = this->z;
		temp.w = 0.0f;

		return temp;
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

// Matrix

struct M4X4
{
	union
	{
		struct
		{
			float _11, _12, _13, _14;
			float _21, _22, _23, _24;
			float _31, _32, _33, _34;
			float _41, _42, _43, _44;
		};
		float m[4][4];
	};

	inline M4X4 operator*(const M4X4& pm2)
	{
		M4X4 out;
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				out.m[i][j] = m[i][0] * pm2.m[0][j] + m[i][1] * pm2.m[1][j] + m[i][2] * pm2.m[2][j] + m[i][3] * pm2.m[3][j];
			}
		}

		return out;
	}

	bool operator==(const M4X4& other) const
	{
		return memcmp(this, &other, sizeof(*this)) == 0;
	}

	bool operator!=(const M4X4& other) const
	{
		return !(*this == other);
	}
};

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