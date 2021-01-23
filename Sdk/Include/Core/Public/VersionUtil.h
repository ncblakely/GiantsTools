#pragma once

#include "Version.h"

const tstring GetAppName();
const Version& GetAppVersion();
int VersionToInt(const Version& version);
void GameVersionRender();