#pragma once

namespace ECS
{
#ifdef ENABLE_DEBUG
	template<typename TComponent>
	void EntityComponentEditor(TComponent& component)
	{
		// No specialization has been defined for the component
	}

	template <typename T>
	inline T EntityValueParser(const std::string& inputValue)
	{
		// No parser is defined for the property type
	}

	template<>
	inline float EntityValueParser(const std::string& inputValue)
	{
		return std::stof(inputValue);
	}

	template<>
	inline int EntityValueParser(const std::string& inputValue)
	{
		return std::stoi(inputValue);
	}

	template<>
	inline SBYTE EntityValueParser(const std::string& inputValue)
	{
		return std::stoi(inputValue);
	}

	template <typename T>
	inline void EntityPropertyEditor(const char* propertyName, T& value)
	{
		std::string valueAsString = fmt::format("{0}", value);
		if (ImGui::InputText(propertyName, &valueAsString))
		{
			if (!valueAsString.empty())
			{
				try
				{
					value = EntityValueParser<T>(valueAsString);
				}
				catch (const std::exception&)
				{
					// Ignore
				}
			}
		}
	}

#endif
}