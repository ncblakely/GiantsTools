#pragma once

#include "GbsData.h"

#define GIANTSIMP_CLASSID	Class_ID(0x552cac79, 0x46f2d727)

class GbsImporter : public SceneImport 
{
public:
	static HWND hParams;

	int				ExtCount();					// Number of extensions supported
	const MCHAR*	Ext(int n);					// Extension #n (i.e. "3DS")
	const MCHAR*	LongDesc();					// Long ASCII description (i.e. "Autodesk 3D Studio File")
	const MCHAR*	ShortDesc();				// Short ASCII description (i.e. "3D Studio")
	const MCHAR*	AuthorName();				// ASCII Author name
	const MCHAR*	CopyrightMessage();			// ASCII Copyright message
	const MCHAR*	OtherMessage1();			// Other message #1
	const MCHAR*	OtherMessage2();			// Other message #2
	unsigned int	Version();					// Version number * 100 (i.e. v3.01 = 301)
	void			ShowAbout(HWND hWnd);		// Show DLL's "About..." box
	int				DoImport(const MCHAR *name,ImpInterface *i,Interface *gi, BOOL suppressPrompts=FALSE);	// Import file

	//Constructor/Destructor
	BOOL SupportsOptions(int Ext,DWORD Options);

private:
	GbsData ReadGbsFile(const MCHAR* Name);
	Mtl* BuildParentMaterial(SubObject& obj, int numSubMaterials);
	Mtl* BuildMaterial(SubObject& obj, Mtl* parentMaterial);
	int GetLocalVertex(Point3* avert, const Mesh& mesh);
	void BuildMeshes(ImpInterface* EI);
	bool EvaluateTriData(unsigned short** pTriData, unsigned short* pTriIdx, unsigned short* acount, int* pV1, int* pV2, int* pV3);

	FILE* m_OpenFile{};
	GbsData m_gbsData;
};

extern HINSTANCE hInstance;