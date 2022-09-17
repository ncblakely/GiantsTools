#pragma once

#include "IGiantsImporter.h"
#include "GbsData.h"

class GbsImporter : IGiantsImporter
{
public:
	// Inherited via IGiantsImporter
	virtual void ImportFile(const MCHAR* name, ImpInterface* ii, Interface* i, BOOL suppressPrompts) override;

private:
	void ReadGbsFile(const MCHAR* Name);
	Mtl* BuildParentMaterial(SubObject& obj, int numSubMaterials);
	Mtl* BuildMaterial(SubObject& obj, Mtl* parentMaterial);
	int GetLocalVertex(Point3* avert, const Mesh& mesh);
	void BuildMeshes(ImpInterface* EI);
	bool EvaluateTriData(unsigned short** pTriData, unsigned short* pTriIdx, unsigned short* acount, int* pV1, int* pV2, int* pV3);

	FILE* m_OpenFile{};
	GbsData m_gbsData;
};