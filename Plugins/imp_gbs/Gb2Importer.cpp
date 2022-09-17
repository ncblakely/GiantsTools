#include "Gb2Importer.h"
#include "StdUtil.h"

void Gb2Importer::ImportFile(const MCHAR* name, ImpInterface* ii, Interface* i, BOOL suppressPrompts)
{
    ReadGb2File(name);

    BuildMeshes(ii);
    ii->RedrawViews();
}


void Gb2Importer::ReadGb2File(const MCHAR* Name)
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
        m_gb2Data.Read(file);

        fclose(file);
    }
    catch (const std::exception& e)
    {
        if (file)
            fclose(file);

        throw e;
    }
}

void Gb2Importer::BuildMeshes(ImpInterface* ii)
{
    for (auto& obj : this->m_gb2Data.objdata)
    {
        assert(obj.ntris > 0);
        assert(obj.nverts > 0);

        Mesh mesh;
        mesh.setNumVerts(obj.nverts);
        mesh.setNumTVerts(obj.nverts);
        mesh.setNumVertCol(obj.nverts);
        mesh.setNumFaces(obj.ntris);
        mesh.setNumTVFaces(obj.ntris);
        mesh.setNumVCFaces(obj.ntris);

        for (int i = 0; i < obj.nverts; i++)
        {
            Point3 point(obj.verts[i].x, obj.verts[i].y, obj.verts[i].z);
            mesh.setVert(i, point);

            const UV& uv = obj.vertuv.at(i);
            mesh.setTVert(i, UVVert(uv.u, uv.v, 0.0f));
        }

        int faceIndex = 0;
        for (int i = 0; i < obj.tris.size(); i += 3)
        {
            int v0 = obj.tris.at(i);
            int v1 = obj.tris.at(i + 1);
            int v2 = obj.tris.at(i + 2);

            // Create faces
            Face& face = mesh.faces[faceIndex];
            face.setVerts(v0, v1, v2);
            face.setEdgeVisFlags(EDGE_VIS, EDGE_VIS, EDGE_VIS);

            // Set per-vertex coloring (appears to be unused by game)
            TVFace& vcFace = mesh.vcFace[faceIndex];
            vcFace.setTVerts(v0, v1, v2);

            // Set textured (UV) vertices
            TVFace& tvFace = mesh.tvFace[faceIndex];
            tvFace.setTVerts(v0, v1, v2);

            faceIndex++;
        }

        mesh.buildNormals();
        mesh.buildBoundingBox();
        mesh.InvalidateEdgeList();

        TriObject* tri = CreateNewTriObject();
        tri->mesh = mesh;
        Matrix3 tm;
        tm.IdentityMatrix();

        ImpNode* Node = ii->CreateNode();
        if (!Node)
        {
            throw std::exception("Could not create Node");
        }

        Mtl* material = BuildMaterial(obj);

        Node->Reference(tri);
        Node->SetTransform(0, tm);
        ii->AddNodeToScene(Node);
        INode* iNode = Node->GetINode();
        iNode->SetMtl(material);
        Node->SetName(util::to_wstring(obj.name).c_str());
    }
}

Mtl* Gb2Importer::BuildMaterial(SingleObjectData& obj)
{
    auto bitmapTex = NewDefaultBitmapTex();
    bitmapTex->SetName(util::to_wstring(obj.texname).c_str());
    bitmapTex->SetMapName(util::to_wstring(obj.texname).c_str());

    StdMat2* stdMat = NewDefaultStdMat();
    stdMat->EnableMap(ID_DI, true);
    if (bitmapTex)
    {
        stdMat->SetSubTexmap(ID_DI, bitmapTex);
    }

    stdMat->SetName(util::to_wstring(obj.texname).c_str());
    return stdMat;
}