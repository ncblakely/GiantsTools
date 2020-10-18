#pragma once

#define GBS_VERSION 0xaa0100be
#define GBSFlagNormals	0x0001
#define GBSFlagUVs		0x0002
#define GBSFlagRGBs		0x0004
#define GBSFlagMaxLit	(1 << 31)

struct FileMaxObj
{
	int		vstart;
	int		vcount;
	int		nstart;
	int		ncount;
	int		noffset;
};

struct MaxObj : FileMaxObj
{
	int		fstart;
	int		fcount;
	int		sostart;
	int		socount;
};

struct SubObject
{
	char objname[32];
	DWORD maxobjindex;
	int ntris; // count of tridata (including preceding 'count' short)
	int totaltris;
	std::vector<unsigned short> tridata; // indexed tridata, value is pointer to iverts
	int vstart;
	int vcount;
	char texname[32];
	char texname2[32];
	float falloff;
	float blend;
	DWORD flags;
	DWORD emissive;
	DWORD ambient;
	DWORD diffuse;
	DWORD specular;
	float power;
};

enum TransType
{
	TransNone,
	TransAdd,
	TransSub,
	TransBlend
};

struct GbsData
{
public:
	void Read(FILE* file);

	DWORD optionsflags{};
	DWORD nndefs{};
	DWORD sizendefs{};
	std::vector<WORD> ndefs;
	std::vector<USHORT> inorms;
	int naverts{};
	std::vector<Point3> averts;
	int nsobjs{};
	int nmobjs{};
	std::vector<USHORT> iverts;
	std::vector<VertRGB> vertrgb;
	int nverts{};
	std::vector<UV> vertuv;
	std::vector<MaxObj> MaxObjs;
	std::vector<SubObject> SubObjs;
private:
	bool VerifyMaxObjs();
};
