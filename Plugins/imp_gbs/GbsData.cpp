#include "Importer.h"

void GbsData::Read(FILE* file)
{
    DWORD versionHeader;
    fread(&versionHeader, 4, 1, file);
    if (versionHeader != GBS_VERSION)
    {
        throw std::exception("File does not appear to be a GBS file.");
    }

    fread(&optionsflags, 4, 1, file);
    fread(&naverts, 4, 1, file);

    averts.resize(naverts);
    for (int i = 0; i < naverts; i++)
    {
        fread(&averts.at(i).x, 4, 1, file);
        fread(&averts.at(i).y, 4, 1, file);
        fread(&averts.at(i).z, 4, 1, file);
    }
    if (FlagIsSet(optionsflags, GBSFlagNormals))
    {
        fread(&nndefs, 4, 1, file);
        fread(&sizendefs, 4, 1, file);
        ndefs.resize(sizendefs);

        for (uint i = 0; i < sizendefs; i++)
            fread(&ndefs.at(i), 2, 1, file);
    }

    fread(&nverts, 4, 1, file);
    iverts.resize(nverts);
    for (int i = 0; i < nverts; i++)
        fread(&iverts.at(i), 2, 1, file);

    if (FlagIsSet(optionsflags, GBSFlagNormals))
    {
        inorms.resize(nverts);
        for (int i = 0; i < nverts; i++)
            fread(&inorms.at(i), 2, 1, file);
    }

    if (FlagIsSet(optionsflags, GBSFlagUVs))
    {
        vertuv.resize(nverts);
        for (int i = 0; i < nverts; i++)
        {
            fread(&vertuv.at(i).u, 4, 1, file);
            fread(&vertuv.at(i).v, 4, 1, file);
            vertuv.at(i).v = vertuv.at(i).v + 1.0f;
        }
    }

    if (FlagIsSet(optionsflags, GBSFlagRGBs))
    {
        vertrgb.resize(nverts);
        for (int i = 0; i < nverts; i++)
        {
            fread(&vertrgb.at(i).r, 1, 1, file);
            fread(&vertrgb.at(i).g, 1, 1, file);
            fread(&vertrgb.at(i).b, 1, 1, file);
        }
    }

    // Get number of max objects
    fread(&nmobjs, 4, 1, file);
    MaxObjs.resize(nmobjs);
    for (auto& maxObj : MaxObjs)
    {
        FileMaxObj fileMaxObj;
        fread(&fileMaxObj, sizeof(FileMaxObj), 1, file);

        maxObj.vstart = fileMaxObj.vstart;
        maxObj.vcount = fileMaxObj.vcount;
        maxObj.nstart = fileMaxObj.nstart;
        maxObj.ncount = fileMaxObj.ncount;
        maxObj.noffset = fileMaxObj.noffset;
        maxObj.fstart = 0;
        maxObj.fcount = 0;
        maxObj.sostart = 0;
        maxObj.socount = 0;
    }

    VerifyMaxObjs();

    // Get number of subobjects
    fread(&nsobjs, 4, 1, file);

    SubObjs.resize(nsobjs);
    for (int ns = 0; ns < nsobjs; ns++)
    {
        SubObject& object = SubObjs.at(ns);
        fread(&object.objname, sizeof(object.objname), 1, file);
        fread(&object.maxobjindex, 4, 1, file);
        fread(&object.totaltris, 4, 1, file);
        fread(&object.ntris, 4, 1, file);

        assert((object.ntris - 1) / 3 == object.totaltris);

        object.tridata.resize(object.ntris + 1);
        fread(object.tridata.data(), sizeof(unsigned short) * object.ntris, 1, file);

        fread(&object.vstart, 4, 1, file);
        fread(&object.vcount, 4, 1, file);
        if (FlagIsSet(optionsflags, GBSFlagUVs))
        {
            fread(&object.texname, 1, 32, file);
            fread(&object.texname2, 1, 32, file);
        }
        fread(&object.falloff, 4, 1, file);

        if (FlagIsSet(optionsflags, GBSFlagRGBs))
            fread(&object.blend, 4, 1, file);

        fread(&object.flags, 4, 1, file);
        fread(&object.emissive, 1, 4, file);
        fread(&object.ambient, 1, 4, file);
        fread(&object.diffuse, 1, 4, file);
        fread(&object.specular, 1, 4, file);
        fread(&object.power, 4, 1, file);

        MaxObj& maxobj = MaxObjs.at(object.maxobjindex);
        maxobj.fcount += object.totaltris;
        if (!maxobj.socount)
        {
            maxobj.sostart = ns;
        }
        maxobj.socount++;
    }
}

bool GbsData::VerifyMaxObjs()
{
    int nmcount = 0;
    for (const auto& maxObj : MaxObjs)
    {
        nmcount += maxObj.vcount;
    }

    assert(nmcount == naverts);
    return nmcount == naverts;
}