#pragma once

namespace ECS
{
    struct ISystem
    {
        virtual ~ISystem() { }

        virtual const char* GetName() = 0;
        virtual int GetPriorityOrder() = 0;
        virtual void Startup() = 0;
        virtual void Update(float delta) = 0;
        virtual void Shutdown() = 0;
    };
}