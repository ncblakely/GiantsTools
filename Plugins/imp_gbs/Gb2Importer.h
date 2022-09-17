#pragma once

#include "IGiantsImporter.h"
#include "Gb2Data.h"

class Gb2Importer : IGiantsImporter
{
public:
	// Inherited via IGiantsImporter
	virtual void ImportFile(const MCHAR* name, ImpInterface* ii, Interface* i, BOOL suppressPrompts) override;

private:
	void ReadGb2File(const MCHAR* Name);
	void BuildMeshes(ImpInterface* ii);
	Mtl* BuildMaterial(SingleObjectData& obj);

	FILE* m_OpenFile{};
	Gb2Data m_gb2Data;
};
