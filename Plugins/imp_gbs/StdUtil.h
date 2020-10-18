#pragma once

#include <codecvt>

namespace util
{

	inline std::wstring to_wstring(const std::string_view& sourceString)
	{
		std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> converter;

		return converter.from_bytes(sourceString.data());
	}

	inline std::string to_string(const std::wstring_view& sourceString)
	{
		using convert_typeX = std::codecvt_utf8<wchar_t>;
		std::wstring_convert<convert_typeX, wchar_t> converterX;

		return converterX.to_bytes(sourceString.data());
	}
}