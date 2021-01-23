#pragma once

#include "BehaviorBase.h"
#include "AI/Public/Components/GetEquipmentBehaviorState.h"

namespace AI
{
    class GetEquipmentBehavior : public BehaviorBase<GetEquipmentBehaviorState>
    {
    public:
        static InnerBehaviorTree Build(ECS::Entity* entity);

    private:
        // Nodes
        static bool NeedsEquipment(ECS::Entity* entity, Object* object, BehaviorTreeContext& context);
        static bool SetMoveGoals(ECS::Entity* entity, Object* object, BehaviorTreeContext& context);
        static beehive::Status MoveToShop(ECS::Entity* entity, Object* object, BehaviorTreeContext& context);
        static beehive::Status EquipSelf(ECS::Entity* entity, Object* object, BehaviorTreeContext& context);

        static void EquipMeccLoadout(ECS::Entity* entity, Object* object, Object* shop);
        static void EquipReaperLoadout(ECS::Entity* entity, Object* object, Object* shop);
        static P3D SelectShopRefPt(Object* object, Object* shop);
    };
}