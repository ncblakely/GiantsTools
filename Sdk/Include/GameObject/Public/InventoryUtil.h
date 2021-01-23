#pragma once

#include "ECS/Public/Entity.h"

namespace GameObj
{
    class InventoryUtil
    {
    public:
        static void ClearInventory(ECS::Entity* entity);
        static void SetPrimaryWeapon(ECS::Entity* entity, IconId icon, int slot, int ammo);
    };
}
