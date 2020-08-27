#pragma once

#include <CodeAnalysis/warnings.h>

// AFX includes:
#include "framework.h"
#include "afxdialogex.h"

// STL includes:
#include <memory>
#include <string>
#include <vector>
#include <codecvt>
#include <functional>
#include <assert.h>
#include <array>
#include <guiddef.h>

// Third party:
#pragma warning(push, 1)
#pragma warning (disable : ALL_CODE_ANALYSIS_WARNINGS)
#include <fmt/format.h>
#pragma warning(pop)