// Importer.cpp : Defines the entry point for the DLL application.
//

#include "Importer.h"
#include "StdUtil.h"

class ImpGbsClassDesc :
    public ClassDesc2
{
public:
    int			IsPublic() { return TRUE; }
    VOID* Create(BOOL Loading) { return new GbsImporter; }
    const MCHAR* ClassName() { return _M("ClassName"); }
    SClass_ID	SuperClassID() { return SCENE_IMPORT_CLASS_ID; }
    Class_ID	ClassID() { return GIANTSIMP_CLASSID; }
    const MCHAR* Category() { return _M(""); }
    const MCHAR* InternalName() { return _M("GiantsImp"); }
    HINSTANCE	HInstance() { return hInstance; }
};

static ImpGbsClassDesc g_ImportCD;

ClassDesc* GetSceneImportDesc()
{
    return &g_ImportCD;
}

void DisplayMessage(const char* msg)
{
    MessageBoxA(GetActiveWindow(), msg, "GBS Import Error", MB_OK);
}

int GbsImporter::ExtCount()
{
    return 1;
}

const MCHAR* GbsImporter::Ext(int n)
{
    switch (n)
    {
        case 0:
            return _M("gbs");
        default:
            return (_M(""));
    }
}

const MCHAR* GbsImporter::LongDesc()
{
    return _M("Long Description");
}

const MCHAR* GbsImporter::ShortDesc()
{
    return _M("Giants Model");
}

const MCHAR* GbsImporter::AuthorName()
{
    return _M("Author");
}

const MCHAR* GbsImporter::CopyrightMessage()
{
    return _M("Copyright");
}

const MCHAR* GbsImporter::OtherMessage1()
{
    return _M("OtherMessage1");
}

const MCHAR* GbsImporter::OtherMessage2()
{
    return _M("OtherMessage2");
}

UINT GbsImporter::Version()
{
    return 100;
}

static BOOL CALLBACK AboutDlgProc(HWND hWnd, UINT Msg, WPARAM wParam, LPARAM lParam)
{
    return FALSE;
}

VOID GbsImporter::ShowAbout(HWND hWnd)
{
}

bool GbsImporter::EvaluateTriData(unsigned short** pTriData, unsigned short* pTriIdx, unsigned short* acount, int* pV1, int* pV2, int* pV3)
{
    unsigned short* triData = *pTriData;
    unsigned short triIdx = *pTriIdx;
    unsigned short count = *acount;

    if (!count)
    {
        if (!(count = *triData))
        {
            return false;
        }

        triIdx = 0;
    }

    *pV1 = triData[triIdx + 1];
    *pV2 = triData[triIdx + 2];
    *pV3 = triData[triIdx + 3];
    triIdx += 3;

    if (!--count)
    {
        triData += *triData * 3 + 1;
    }

    *pTriData = triData;
    *pTriIdx = triIdx;
    *acount = count;

    return true;
}

int GbsImporter::DoImport(const MCHAR* Name, ImpInterface* EI, Interface* I, BOOL SupressPrompts)
{
    try
    {
        m_gbsData = ReadGbsFile(Name);
    }
    catch (const std::exception& e)
    {
        DisplayMessage(e.what());
    }

    BuildMeshes(EI);
    EI->RedrawViews();

    return TRUE;
}

GbsData GbsImporter::ReadGbsFile(const MCHAR* Name)
{
    FILE* file = nullptr;
    try
    {
        errno_t err = _tfopen_s(&file, Name, (_T("rb")));
        if (err != 0)
        {
            throw std::exception("Could not open input file.");
        }

        // Read object into memory
        GbsData gbsFile;
        gbsFile.Read(file);

        fclose(file);
        return gbsFile;
    }
    catch (const std::exception& e)
    {
        if (file)
            fclose(file);

        throw e;
    }
}

int GbsImporter::GetLocalVertex(Point3* avert, const Mesh& mesh)
{
    for (int i = 0; i < mesh.getNumVerts(); i++)
    {
        auto& vert = mesh.verts[i];

        if (vert.x == avert->x && vert.y == avert->y && vert.z == avert->z)
        {
            return i;
        }
    }

    assert(false && "Invalid vertex for object");
    return -1;
}

void GbsImporter::BuildMeshes(ImpInterface* EI)
{
    for (const auto& mobj : m_gbsData.MaxObjs)
    {
        assert(mobj.fcount > 0);
        assert(mobj.vcount > 0);

        Mesh mesh;
        mesh.setNumVerts(mobj.vcount);
        mesh.setNumTVerts(mobj.vcount);
        mesh.setNumVertCol(mobj.vcount);
        mesh.setNumFaces(mobj.fcount);
        mesh.setNumTVFaces(mobj.fcount);
        mesh.setNumVCFaces(mobj.fcount);

        for (int i = 0; i < mobj.vcount; i++)
        {
            int vstart = mobj.vstart;
            int index = i + vstart;
            assert(index < m_gbsData.naverts);

            UV* uvptr = (UV*)(m_gbsData.vertuv.data() + (mobj.vstart + i));

            Point3 point(m_gbsData.averts[index].x, m_gbsData.averts[index].y, m_gbsData.averts[index].z);

            mesh.setVert(i, point);
            mesh.setTVert(i, UVVert(uvptr->u, uvptr->v, 0.0f));
            //mesh.setTVert(i, UVVert(uvptr->u, -uvptr->v, 0.0f));
        }

        Mtl* topLevelMaterial = nullptr;
        const char* objName = nullptr;
        int materialLevel = 0;

        int faceIndex = 0;
        for (int subObjIndex = mobj.sostart; subObjIndex < mobj.sostart + mobj.socount; subObjIndex++)
        {
            SubObject& obj = m_gbsData.SubObjs.at(subObjIndex);
            if (mobj.socount > 1)
            {
                // Max object consists of multiple sub objects
                if (materialLevel == 0)
                {
                    // Build parent material (either MixMat or multi-mtl):
                    topLevelMaterial = BuildParentMaterial(obj, mobj.socount);
                    objName = obj.objname;
                }

                // Add sub-material:
                Mtl* subMtl = BuildMaterial(obj, topLevelMaterial);
                topLevelMaterial->SetSubMtl(materialLevel, subMtl);
            }
            else
            {
                // Just one subobj, build the single material and be done with it:
                topLevelMaterial = BuildMaterial(obj, nullptr);
                objName = obj.objname;
            }

            int v1, v2, v3;
            unsigned short tidx = -1;
            unsigned short* tptr = obj.tridata.data();

            unsigned short count = 0;
            while (EvaluateTriData(&tptr, &tidx, &count, &v1, &v2, &v3))
            {
                assert(faceIndex < mesh.getNumFaces());
                int vstart = mobj.vstart;
                Face& face = mesh.faces[faceIndex];

                // Map from display to animation vertices, then to a "local" index for this sub object, starting from zero:
                int remappedV0 = GetLocalVertex(&m_gbsData.averts[m_gbsData.iverts.at(v1)], mesh);
                int remappedV1 = GetLocalVertex(&m_gbsData.averts[m_gbsData.iverts.at(v2)], mesh);
                int remappedV2 = GetLocalVertex(&m_gbsData.averts[m_gbsData.iverts.at(v3)], mesh);

                VertColor v0Col = VertColor(m_gbsData.vertrgb.at(v1).r / 255.0f, m_gbsData.vertrgb.at(v1).g / 255.0f, m_gbsData.vertrgb.at(v1).b / 255.0f);
                VertColor v1Col = VertColor(m_gbsData.vertrgb.at(v2).r / 255.0f, m_gbsData.vertrgb.at(v2).g / 255.0f, m_gbsData.vertrgb.at(v2).b / 255.0f);
                VertColor v2Col = VertColor(m_gbsData.vertrgb.at(v3).r / 255.0f, m_gbsData.vertrgb.at(v3).g / 255.0f, m_gbsData.vertrgb.at(v3).b / 255.0f);

                mesh.vertCol[remappedV0] = v0Col;
                mesh.vertCol[remappedV1] = v1Col;
                mesh.vertCol[remappedV2] = v2Col;

                assert(remappedV0 >= 0 && remappedV1 >= 0 && remappedV2 >= 0);
                assert(remappedV0 < mobj.vcount && remappedV1 < mobj.vcount && remappedV2 < mobj.vcount);
                assert(remappedV0 < m_gbsData.naverts && remappedV1 < m_gbsData.naverts && remappedV2 < m_gbsData.naverts);

                face.setVerts(remappedV0, remappedV1, remappedV2);
                face.setEdgeVisFlags(EDGE_VIS, EDGE_VIS, EDGE_VIS);
                face.setMatID(materialLevel);

                TVFace& tvFace = mesh.tvFace[faceIndex];
                tvFace.setTVerts(remappedV0, remappedV1, remappedV2);

                TVFace& vcFace = mesh.vcFace[faceIndex];
                vcFace.setTVerts(remappedV0, remappedV1, remappedV2);

                faceIndex++;
            }

            materialLevel++;
        }

        mesh.buildNormals();
        mesh.buildBoundingBox();
        mesh.InvalidateEdgeList();

        TriObject* tri = CreateNewTriObject();
        tri->mesh = mesh;
        Matrix3 tm;
        tm.IdentityMatrix();

        ImpNode* Node = EI->CreateNode();
        if (!Node)
        {
            throw std::exception("Could not create Node");
        }

        Node->Reference(tri);
        Node->SetTransform(0, tm);
        EI->AddNodeToScene(Node);
        INode* iNode = Node->GetINode();
        iNode->SetMtl(topLevelMaterial);
        Node->SetName(util::to_wstring(objName).c_str());
    }
}

Mtl* GbsImporter::BuildParentMaterial(SubObject& obj, int numSubMaterials)
{
    // Check if we need to create a MixMat blend material
    // (this check is iffy, not fully certain how to identify MixMat exports but this seems close enough)
    if (obj.blend > 0.50 && (obj.flags & 0x10 || obj.flags & 0x20))
    {
        //if (bitmapTex)
        //{
        //    //bitmapTex->GetUVGen()->SetUVWSource(2);
        //}

        // Create custom MixMat material (from official 3dsmax plugin)
        Mtl* mixMatMaterial = (Mtl*)CreateInstance(SClass_ID(MATERIAL_CLASS_ID), Class_ID(MIXMAT_CLASS_ID, 0));
        mixMatMaterial->SetName(util::to_wstring(obj.objname).c_str());

        int ns = mixMatMaterial->NumSubMtls();

        // Set the blend value
        IParamBlock2* parameterBlock = mixMatMaterial->GetParamBlock(0);
        parameterBlock->SetValue(0, 0, obj.blend);

        return mixMatMaterial;
    }
    else
    {
        // Standard multi-material
        MultiMtl* multiMtl = NewDefaultMultiMtl();
        multiMtl->SetName(util::to_wstring(obj.objname).c_str());
        multiMtl->SetNumSubMtls(numSubMaterials);

        return multiMtl;
    }
}

Mtl* GbsImporter::BuildMaterial(SubObject& obj, Mtl* parentMaterial)
{
    BitmapTex* bitmapTex = nullptr;

    if (obj.texname && obj.texname[0])
    {
        std::wstring mapName = util::to_wstring(obj.texname).c_str();
        mapName += L".tga"; // DDS partially supported but not in use, okay to hardcode for now

        bitmapTex = NewDefaultBitmapTex();
        bitmapTex->SetName(util::to_wstring(obj.texname).c_str());
        bitmapTex->SetMapName(mapName.c_str());
    }

    StdMat2* stdMat = NewDefaultStdMat();
    if (FlagIsSet(obj.flags, GBSFlagMaxLit) || parentMaterial)
    {
        // This isn't quite right, not all models with blend materials have this flag set. See ripper.gbs.
        // Behavior is correct enough for now though.
        stdMat->SetWireUnits(TRUE);
    }

    stdMat->EnableMap(ID_DI, true);

    if (obj.flags & 0x10)
    {
        stdMat->SetTransparencyType(TRANSP_ADDITIVE);
    }
    else if (obj.flags & 0x20)
    {
        stdMat->SetTransparencyType(TRANSP_SUBTRACTIVE);
    }
    stdMat->SetShinStr(1.0f, 0);
    stdMat->SetOpacFalloff(obj.falloff, 0);

    if (obj.emissive != 0)
    {
        Color emissive(GetRValue(obj.emissive), GetGValue(obj.emissive), GetBValue(obj.emissive));
        stdMat->SetSelfIllumColor(emissive, 0);
    }

    if (bitmapTex)
    {
        stdMat->SetSubTexmap(ID_DI, bitmapTex);
    }

    Color diffuse(GetRValue(obj.diffuse), GetGValue(obj.diffuse), GetBValue(obj.diffuse));
    stdMat->SetDiffuse(diffuse, 0);
    Color ambient(GetRValue(obj.ambient), GetGValue(obj.ambient), GetBValue(obj.ambient));
    stdMat->SetAmbient(ambient, 0);
    Color specular(GetRValue(obj.specular), GetGValue(obj.specular), GetBValue(obj.specular));
    stdMat->SetSpecular(specular, 0);
    stdMat->SetShininess(obj.power / 100.0f, 0);

    stdMat->SetName(util::to_wstring(obj.objname).c_str());

    return stdMat;
}

BOOL GbsImporter::SupportsOptions(int Ext, DWORD Options)
{
    return FALSE;
}