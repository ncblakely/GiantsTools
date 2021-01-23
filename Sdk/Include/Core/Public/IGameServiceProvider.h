#pragma once

#include <memory>
#include <guiddef.h>

#include "IGameService.h"

template<typename T>
struct TypedGet
{
	template<typename TGet>
	std::shared_ptr<TGet> Get()
	{
		std::shared_ptr<IGameService> pComponent = ((T*)this)->Get(__uuidof(TGet));
		if (pComponent)
		{
			return std::static_pointer_cast<TGet>(pComponent);
		}

		return nullptr;
	}
};

template<typename T>
struct TypedAdd
{
	template<typename... TInterfaces>
	void Add(std::shared_ptr<IGameService> component)
	{
		static constexpr std::array<GUID, sizeof...(TInterfaces)> InterfaceUuids = { { __uuidof(TInterfaces)... } };

		for (const auto& uuid : InterfaceUuids)
		{
			((T*)this)->Add(uuid, component);
		}
	}
};

/// <summary>
/// Container for game components.
/// Facilitates resource acquisition across module boundaries, and interop with .NET code.
/// </summary>
struct IGameServiceProvider : public TypedGet<IGameServiceProvider>, public TypedAdd<IGameServiceProvider>
{
	virtual ~IGameServiceProvider() { }

	virtual void Add(const IID& iid, std::shared_ptr<IGameService> component) = 0;
	virtual std::shared_ptr<IGameService> Get(const IID& iid) noexcept = 0;
	virtual void Remove(const IID& iid) noexcept = 0;
	virtual void ReleaseAll() = 0;

	template<typename... TInterfaces>
	void Add(std::shared_ptr<IGameService> component)
	{
		return TypedAdd::Add<TInterfaces...>(component);
	}

	template <typename T>
	std::shared_ptr<T> Get()
	{
		return TypedGet::Get<T>();
	}
};