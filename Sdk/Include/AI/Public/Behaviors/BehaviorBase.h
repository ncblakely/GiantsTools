#pragma once

#include "IBehavior.h"
#include "ECS/Public/Entity.h"
#include "GameObject/Public/Core.h"

namespace AI
{
    template <typename TState>
    class BehaviorBase : public IBehavior
    {
    protected:
        inline static std::function<bool(BehaviorTreeContext& context)> CreateLeaf(
            ECS::Entity* entity, 
            std::function<bool(ECS::Entity* entity, Object* object, BehaviorTreeContext& context)> processFunction)
        {
            using namespace std::placeholders;

            Object* object = GetObjFromEntity(entity);
            return std::bind(processFunction, entity, object, _1);
        }

        inline static std::function<beehive::Status(BehaviorTreeContext& context)> CreateLeaf(
            ECS::Entity* entity,
            std::function<beehive::Status(ECS::Entity* entity, Object* object, BehaviorTreeContext& context)> processFunction)
        {
            using namespace std::placeholders;

            Object* object = GetObjFromEntity(entity);
            return std::bind(processFunction, entity, object, _1);
        }

    private:
        inline static Object* GetObjFromEntity(ECS::Entity* entity)
        {
            if (entity)
            {
                GameObj::ObjectRef& objectRef = entity->GetComponent<GameObj::ObjectRef>();
                return objectRef.GetObj();
            }

            return nullptr;
        }
    };
}