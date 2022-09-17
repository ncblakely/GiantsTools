#pragma once

#include "GbsData.h"
#include "Gb2Data.h"

#define GBXFlagNormals	0x0001
#define GBXFlagUVs		0x0002
#define GBXFlagRGBs		0x0004

#define GIANTSIMP_CLASSID	Class_ID(0x552cac79, 0x46f2d727)

class GiantsImporter : public SceneImport 
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
	int				DoImport(const MCHAR* name, ImpInterface* ii, Interface* i, BOOL suppressPrompts);	// Import file

	//Constructor/Destructor
	BOOL SupportsOptions(int Ext,DWORD Options);
};

extern HINSTANCE hInstance;