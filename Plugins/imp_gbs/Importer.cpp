// Importer.cpp : Defines the entry point for the DLL application.
//

#include "Importer.h"
#include "ImporterFactory.h"

class ImpGbsClassDesc :
    public ClassDesc2
{
public:
    int			IsPublic() { return TRUE; }
    VOID* Create(BOOL Loading) { return new GiantsImporter; }
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

int GiantsImporter::ExtCount()
{
    return 2;
}

const MCHAR* GiantsImporter::Ext(int n)
{
    switch (n)
    {
        case 0:
            return _M("gbs");
        case 1:
            return _M("gb2");
        default:
            return (_M(""));
    }
}

const MCHAR* GiantsImporter::LongDesc()
{
    return _M("Long Description");
}

const MCHAR* GiantsImporter::ShortDesc()
{
    return _M("Giants Model");
}

const MCHAR* GiantsImporter::AuthorName()
{
    return _M("Author");
}

const MCHAR* GiantsImporter::CopyrightMessage()
{
    return _M("Copyright");
}

const MCHAR* GiantsImporter::OtherMessage1()
{
    return _M("OtherMessage1");
}

const MCHAR* GiantsImporter::OtherMessage2()
{
    return _M("OtherMessage2");
}

UINT GiantsImporter::Version()
{
    return 100;
}

static BOOL CALLBACK AboutDlgProc(HWND hWnd, UINT Msg, WPARAM wParam, LPARAM lParam)
{
    return FALSE;
}

VOID GiantsImporter::ShowAbout(HWND hWnd)
{
}

int GiantsImporter::DoImport(const MCHAR* Name, ImpInterface* EI, Interface* I, BOOL suppressPrompts)
{
    try
    {
        ImporterFactory::ImportFile(Name, EI, I, suppressPrompts);
    }
    catch (const std::exception& e)
    {
        DisplayMessage(e.what());
    }

    return TRUE;
}

BOOL GiantsImporter::SupportsOptions(int Ext, DWORD Options)
{
    return FALSE;
}