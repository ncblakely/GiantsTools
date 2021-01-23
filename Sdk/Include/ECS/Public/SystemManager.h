#pragma once

#include "ECS/Public/ISystem.h"

namespace ECS
{
    class SystemManager
    {
    public:
        template<typename TSystem>
        void AddSystem()
        {
            m_systems.push_back(std::make_unique<TSystem>());
        }

        void StartSystems();
        void UpdateSystems(float delta);
        void ShutdownSystems();

    private:
        std::vector<std::unique_ptr<ISystem>> m_systems;
    };
}