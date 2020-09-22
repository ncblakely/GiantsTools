#pragma once

#include <IComponent.h>
#include <wrl/client.h>

template<typename T>
using ComPtr = Microsoft::WRL::ComPtr<T>;

// {C942AA9B-C576-4D3F-A54F-B135B500E611}
inline const GUID IID_IComponentContainer = { 0xc942aa9b, 0xc576, 0x4d3f, 0xa5, 0x4f, 0xb1, 0x35, 0xb5, 0x0, 0xe6, 0x11 };

template<typename T>
struct TypedGet
{
	template<typename TGet>
	ComPtr<TGet> Get()
	{
		ComPtr<TGet> temp;
		ComPtr<IComponent> pComponent = ((T*)this)->Get(__uuidof(TGet));
		if (pComponent)
		{
			HRESULT hr = pComponent.As<TGet>(&temp);
			if (FAILED(hr))
			{
				if (hr == E_NOINTERFACE)
				{
					throw std::invalid_argument("The interface is not supported.");
				}

				throw std::invalid_argument(fmt::format("Unknown exception {0:x} querying interface.", hr));
			}

			pComponent.Detach();
			return temp;
		}

		return ComPtr<TGet>(); // Null
	}
};

/// <summary>
/// Container for game components.
/// Facilitates resource acquisition across module boundaries, and interop with .NET code.
/// </summary>
struct IComponentContainer : public TypedGet<IComponentContainer>
{
	virtual ~IComponentContainer() { }

	virtual void Add(const IID& iid, IComponent* component) = 0;
	virtual ComPtr<IComponent> Get(const IID& iid) noexcept = 0;
	virtual void Remove(const IID& iid) noexcept = 0;
	virtual void ReleaseAll() = 0;

	template <typename T>
	ComPtr<T> Get()
	{
		return TypedGet::Get<T>();
	}
};

struct DECLSPEC_UUID("{C942AA9B-C576-4D3F-A54F-B135B500E611}") IComponentContainer;