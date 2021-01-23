#pragma once

#include "Core/Public/Core.h"
#include <Sdk/External/entt/entt.hpp>

namespace ECS
{
    class Entity
    {
    public:
        Entity(entt::entity entity, entt::registry& registry)
            : m_registry(registry)
        {
            m_entity = entity;
        }

        Entity(entt::registry& registry)
            : m_registry(registry)
        {
            m_entity = m_registry.create();
        }

        void Destroy()
        {
            m_registry.destroy(m_entity);
        }

        template<typename T, typename... Args>
        T& AddComponent(Args&&... args)
        {
            assert(!HasComponent<T>());
            return m_registry.emplace<T>(m_entity, std::forward<Args>(args)...);
        }

        template<typename T, typename... Args>
        void AddOrReplaceComponent(Args&&... args)
        {
            m_registry.emplace_or_replace<T>(m_entity, std::forward<Args>(args)...);
        }

        template<typename... TComponent>
        decltype(auto) GetComponent()
        {
            return m_registry.get<TComponent...>(m_entity);
        }

        template<typename... TComponent>
        bool HasComponent()
        {
            return m_registry.has<TComponent...>(m_entity);
        }

        template<typename T, typename... Args>
        T& RemoveComponent(Args&&... args)
        {
            assert(HasComponent<T>());
            return m_registry.remove<T>(m_entity, std::forward<Args>(args)...);
        }

        template<typename T, typename... Args>
        std::size_t RemoveComponentIfExists(Args&&... args)
        {
            return m_registry.remove_if_exists<T>(m_entity, std::forward<Args>(args)...);
        }

        template<typename T, typename... Func>
        void PatchComponent(Func &&... func)
        {
            assert(HasComponent<T>());
            m_registry.patch<T>(m_entity, std::forward<Func>(func)...);
        }

    private:
        entt::entity m_entity;
        entt::registry& m_registry;
    };
}
