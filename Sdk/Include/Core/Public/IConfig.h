#pragma once

#include "Core/Public/Core.h"

DEFINE_SERVICE_MULTI("{599E6624-694C-41B6-B354-62EEA1132041}", IConfig, IEventSource<ConfigEventType, ConfigEvent>)
{
    virtual ~IConfig() = default;

    virtual void Read() = 0;

    virtual void Save() = 0;

    virtual float GetFloat(const tstring_view& section, const tstring_view& setting) const = 0;

    virtual std::vector<float> GetFloatArray(const tstring_view& section, const tstring_view& setting) const = 0;

    virtual int GetInteger(const tstring_view& section, const tstring_view& setting) const = 0;

    virtual std::vector<int> GetIntegerArray(const tstring_view& section, const tstring_view& setting) const = 0;

    virtual tstring GetString(const tstring_view& section, const tstring_view& setting) const = 0;

    virtual std::vector<tstring> GetStringArray(const tstring_view& section, const tstring_view& setting) const = 0;

    virtual void SetFloat(const tstring_view& section, const tstring_view& setting, float value) = 0;

    virtual void SetFloatArray(const tstring_view& section, const tstring_view& setting, std::vector<float>&& values) = 0;

    virtual void SetInteger(const tstring_view& section, const tstring_view& setting, int value) = 0;

    virtual void SetIntegerArray(const tstring_view& section, const tstring_view& setting, std::vector<int>&& values) = 0;

    virtual void SetString(const tstring_view& section, const tstring_view& setting, tstring_view value) = 0;

    virtual void SetStringArray(const tstring_view& section, const tstring_view& setting, std::vector<tstring>&& values) = 0;
};