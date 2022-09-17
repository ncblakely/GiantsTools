#pragma once

class ImporterFactory
{
public:
	static void ImportFile(const MCHAR* name, ImpInterface* ii, Interface* i, BOOL suppressPrompts);
};