#pragma once

#define GB2_VERSION 0xaa0100ab

struct SingleObjectData
{
	std::vector<P3D> verts;
	std::vector<unsigned short> tris;
	int ntris{};
	std::vector<VertRGB> vertrgb;
	int nverts{};
	std::vector<UV> vertuv;
	std::string name;
	std::string texname;
};

struct Gb2Data
{
	void Read(FILE* file);

	std::vector<SingleObjectData> objdata;
};
