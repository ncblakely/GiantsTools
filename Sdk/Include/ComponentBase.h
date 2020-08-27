#pragma once

#include <map>
#include <assert.h>
#include <array>
#include <guiddef.h>

#include <IComponent.h>
#include <IComponentContainer.h>

#pragma warning (disable:26487) // Disable LIFETIMES_FUNCTION_POSTCONDITION_VIOLATION: not COM-aware

template<int N, typename... Ts> using NthTypeOf =
typename std::tuple_element<N, std::tuple<Ts...>>::type;

/// <summary>
// Base class for game components, automatically implements IUnknown.
// On construction, registers self with the component container (which is guaranteed to exist
// for the lifetime of the game process, hence the naked pointer stored in this type).
// On final release, the component is removed from the container.
//
// Note: following C#'s example here, we don't allow multiple inheritance. 
// The first interface is the "primary" interface that we inherit from. The additional interfaces
// expanded from the parameter pack are registered with the container, but not inherited.
/// </summary>
/// <typeparam name="...TInterfaces">The interfaces that this component can be located by.</typeparam>
template<typename... TInterfaces>
struct ComponentBase : public NthTypeOf<0, TInterfaces...>
{
    static constexpr std::array<GUID, sizeof...(TInterfaces)> ImplementedIids = { { __uuidof(TInterfaces)... } };
    ComponentBase(IComponentContainer* container)
    {
        m_pContainer = container;

        AddInterfaces();
    }

    virtual ~ComponentBase() 
    { 
        RemoveInterfaces();
    }

    HRESULT STDMETHODCALLTYPE QueryInterface(
        const GUID& riid,
        _COM_Outptr_ void __RPC_FAR* __RPC_FAR* ppvObject) override
    {
        if (!ppvObject)
        {
            return E_INVALIDARG;
        }

        *ppvObject = nullptr;

        if (riid == IID_IUnknown 
            || riid == IID_IComponent
            || IsExplicitlyImplementedInterface(riid))
        {
            *ppvObject = this;
            return S_OK;
        }

        return E_NOINTERFACE;
    }

    unsigned long STDMETHODCALLTYPE AddRef() override
    {
        InterlockedIncrement(&m_refs);
        return m_refs;
    }

    unsigned long STDMETHODCALLTYPE Release() override
    {
        const unsigned long refCount = InterlockedDecrement(&m_refs);
        assert(refCount >= 0);
        if (refCount == 0)
        {
            delete this;
        }

        return refCount;
    }

protected:
    IComponentContainer* m_pContainer = nullptr;

private:
    void AddInterfaces()
    {
        for (const auto& implementedIid : ImplementedIids)
        {
            m_pContainer->Add(implementedIid, this);
        }
    }

    bool IsExplicitlyImplementedInterface(const GUID& iid) noexcept
    {
        for (const auto& implementedIid : ImplementedIids)
        {
            if (IsEqualGUID(implementedIid, iid))
            {
                return true;
            }
        }

        return false;
    }

    void RemoveInterfaces() noexcept
    {
        for (const auto& implementedIid : ImplementedIids)
        {
            m_pContainer->Remove(implementedIid);
        }
    }

    unsigned long m_refs = 0;
};

#pragma warning (default:26487)