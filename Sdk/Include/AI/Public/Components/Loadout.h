#pragma once

#include "GameObject/Public/Components/Inventory.h"

namespace AI
{
    struct PlayerLoadout
    {
        PlayerLoadout(int primaryCapacity, int otherCapacity)
            : DesiredInventory(primaryCapacity, otherCapacity)
        {
        }

        static PlayerLoadout CreateMeccLoadout()
        {
            return PlayerLoadout(GameObj::MeccNumPrimaryWeapons, GameObj::MeccNumOther);
        }

        static PlayerLoadout CreateReaperLoadout()
        {
            return PlayerLoadout(GameObj::ReaperNumPrimaryWeapons, GameObj::ReaperNumOther);
        }

        GameObj::InventoryState DesiredInventory;
    };
}