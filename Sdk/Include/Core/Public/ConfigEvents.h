#pragma once

/// <summary>
/// Event types for <see cref="IConfig">.
/// </summary>
enum class ConfigEventType
{
	None = 0,

	ConfigLoaded,
	ConfigSaving
};

struct ConfigEvent
{
public:
	virtual ~ConfigEvent() = default;
	ConfigEvent(ConfigEventType type) noexcept { this->type = type; }

private:
	ConfigEventType type;
};

struct ConfigLoadedEvent : ConfigEvent
{
	ConfigLoadedEvent(struct IConfig& config) noexcept 
		: ConfigEvent(ConfigEventType::ConfigLoaded),
		config(config)
	{ 
	}

	struct IConfig& config;
};

struct ConfigSavingEvent : ConfigEvent
{
	ConfigSavingEvent(struct IConfig& config) noexcept
		: ConfigEvent(ConfigEventType::ConfigSaving),
		config(config)
	{
	}

	struct IConfig& config;
};
