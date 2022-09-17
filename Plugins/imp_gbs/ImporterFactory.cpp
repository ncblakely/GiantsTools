#include "ImporterFactory.h"
#include "GbsImporter.h"
#include "Gb2Importer.h"

void ImporterFactory::ImportFile(const MCHAR* Name, ImpInterface* EI, Interface* I, BOOL SupressPrompts)
{
	M_STD_STRING strName = Name;
	
	if (strName.rfind(L".gbs") != -1)
	{
		GbsImporter importer;
		importer.ImportFile(Name, EI, I, SupressPrompts);
	}
	else if (strName.rfind(L".gb2") != -1)
	{
		Gb2Importer importer;
		importer.ImportFile(Name, EI, I, SupressPrompts);
	}
}