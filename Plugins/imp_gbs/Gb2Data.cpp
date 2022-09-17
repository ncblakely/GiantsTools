#include "Importer.h"

#define MemReadInt(m,v) { (v) = *(int*)(m); (m)+=sizeof(int); }
#define MemReadFloat(m,v) { (v) = *(float*)(m); (m)+=sizeof(float); }
#define MemReadData(m,v,l) { memcpy( (v), (m), (l) ); (m)+=(l); }
#define MemSkipData(m,l) { (m)+=(l); }

void Gb2Data::Read(FILE* file)
{
	char	name[16], texname[16];
	int		numobj, offset, flags, matflags;
	float	falloff, blend;

	fseek(file, 0, SEEK_END);
	int zlen = ftell(file);
	fseek(file, 0, SEEK_SET);

	auto zmemArray = std::vector<BYTE>(zlen);
	UBYTE* zmem = zmemArray.data();
	fread(zmem, zlen, 1, file);

	UBYTE* memptr = zmem;
	fclose(file);

	DWORD versionHeader;
	MemReadInt(memptr, versionHeader);
    if (versionHeader != GB2_VERSION)
    {
        throw std::exception("File does not appear to be a GB2 file.");
    }

	// Read # of objects
	MemReadInt(memptr, numobj);

	this->objdata.resize(numobj);

	// Process each object
	UBYTE* tablemem = memptr;
	for (int ns = 0; ns < numobj; ns++)
	{
		SingleObjectData& obj = this->objdata.at(ns);

		memptr = tablemem + ns * sizeof(int);

		MemReadInt(memptr, offset);
		memptr = zmem + offset;
		MemReadData(memptr, name, 16);

		obj.name = name;

		MemReadInt(memptr, flags);
		MemReadFloat(memptr, falloff);
		if (FlagIsSet(flags, GBXFlagRGBs))
		{
			MemReadFloat(memptr, blend);
		}
		else
			blend = 0;

		MemReadInt(memptr, matflags);

		if (FlagIsSet(flags, GBXFlagUVs))
		{
			MemReadData(memptr, texname, sizeof(texname));
			obj.texname = texname;
		}

		MemReadInt(memptr, obj.nverts);
		MemReadInt(memptr, obj.ntris);

		// Allocate vertex & tri lists
		obj.verts.resize(obj.nverts);
		if (FlagIsSet(flags, GBXFlagUVs))
		{
			obj.vertuv.resize(obj.nverts);
		}

		if (FlagIsSet(flags, GBXFlagRGBs))
		{
			obj.vertrgb.resize(obj.nverts);
		}

		// Read vertices
		for (int i = 0; i < obj.nverts; i++)
		{
			MemReadData(memptr, &obj.verts.at(i), sizeof(P3D));
		}

		if (FlagIsSet(flags, GBXFlagNormals))
		{
			MemSkipData(memptr, sizeof(P3D) * obj.nverts); // Normals are auto-generated by the game
		}

		if (FlagIsSet(flags, GBXFlagUVs))
		{
			// read uvs
			for (int i = 0; i < obj.nverts; i++)
			{
				MemReadData(memptr, &obj.vertuv.at(i), sizeof(UV));
				obj.vertuv.at(i).v = obj.vertuv.at(i).v + 1.0f;
			}
		}

		if (FlagIsSet(flags, GBXFlagRGBs))
		{
			for (int i = 0; i < obj.nverts; i++)
			{
				// read rgbs
				MemReadData(memptr, &obj.vertrgb.at(i), sizeof(VertRGB));
			}
		}

		obj.tris.resize(obj.ntris * 3);
		// Read tris
		for (int i = 0; i < obj.ntris * 3; i++)
		{
			MemReadData(memptr, &obj.tris.at(i), sizeof(int));
		}
	}
}