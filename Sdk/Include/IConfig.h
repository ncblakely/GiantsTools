#pragma once

#include <IComponent.h>
#include <IEventSource.h>
#include <ConfigEvents.h>
#include <ConfigConstants.h>

// {599E6624-694C-41B6-B354-62EEA1132041}
inline const GUID IID_IConfig = { 0x599e6624, 0x694c, 0x41b6, 0xb3, 0x54, 0x62, 0xee, 0xa1, 0x13, 0x20, 0x41 };

struct IConfig : IComponent, IEventSource<ConfigEventType, ConfigEvent>
{
    virtual ~IConfig() = default;

    virtual void STDMETHODCALLTYPE Read() = 0;

    virtual void STDMETHODCALLTYPE Save() = 0;

    virtual float STDMETHODCALLTYPE GetFloat(const tstring_view& section, const tstring_view& setting) const = 0;

    virtual std::vector<float> STDMETHODCALLTYPE GetFloatArray(const tstring_view& section, const tstring_view& setting) const = 0;

    virtual int STDMETHODCALLTYPE GetInteger(const tstring_view& section, const tstring_view& setting) const = 0;

    virtual std::vector<int> STDMETHODCALLTYPE GetIntegerArray(const tstring_view& section, const tstring_view& setting) const = 0;

    virtual tstring STDMETHODCALLTYPE GetString(const tstring_view& section, const tstring_view& setting) const = 0;

    virtual std::vector<tstring> STDMETHODCALLTYPE GetStringArray(const tstring_view& section, const tstring_view& setting) const = 0;

    virtual void STDMETHODCALLTYPE SetFloat(const tstring_view& section, const tstring_view& setting, float value) = 0;

    virtual void STDMETHODCALLTYPE SetFloatArray(const tstring_view& section, const tstring_view& setting, std::vector<float>&& values) = 0;

    virtual void STDMETHODCALLTYPE SetInteger(const tstring_view& section, const tstring_view& setting, int value) = 0;

    virtual void STDMETHODCALLTYPE SetIntegerArray(const tstring_view& section, const tstring_view& setting, std::vector<int>&& values) = 0;

    virtual void STDMETHODCALLTYPE SetString(const tstring_view& section, const tstring_view& setting, tstring_view value) = 0;

    virtual void STDMETHODCALLTYPE SetStringArray(const tstring_view& section, const tstring_view& setting, std::vector<tstring>&& values) = 0;
};

struct DECLSPEC_UUID("{599E6624-694C-41B6-B354-62EEA1132041}") IConfig;