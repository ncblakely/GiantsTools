#include "pch.h"

#include <codecvt>

#include "Utils.h"

std::wstring to_wstring(const std::string_view& sourceString)
{
	std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> converter;

	return converter.from_bytes(sourceString.data());
}