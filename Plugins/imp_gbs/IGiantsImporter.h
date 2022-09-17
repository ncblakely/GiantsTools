#pragma once

struct IGiantsImporter
{
	virtual void ImportFile(const MCHAR* name, ImpInterface* ii, Interface* i, BOOL suppressPrompts) = 0;
};
