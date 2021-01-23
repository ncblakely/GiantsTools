#pragma once

namespace GameObj
{
    const int MeccNumPrimaryWeapons = 4;
    const int MeccNumOther = 3;

    const int ReaperNumPrimaryWeapons = 4;
    const int ReaperMaxSpells = 5;
    const int ReaperNumOther = 2;

    struct InventoryIcon
    {
        int IconId{};
        int Count{};
    };

    struct SpellInventoryIcon
    {
        int IconId{};
        float ManaCost{};
        float Energy{};
    };

    struct InventoryState
    {
        InventoryState(int primaryCapacity, int otherCapacity)
        {
            PrimaryIcons.resize(primaryCapacity);
            OtherIcons.resize(otherCapacity);
        }

        static InventoryState CreateMeccInventory()
        {
            return InventoryState(MeccNumPrimaryWeapons, MeccNumOther);
        }

        static InventoryState CreateReaperInventory()
        {
            return InventoryState(ReaperNumPrimaryWeapons, ReaperNumOther);
        }

        std::vector<InventoryIcon> PrimaryIcons;
        std::vector<InventoryIcon> OtherIcons;
        InventoryIcon SpecialIcon;
    };

    struct SpellInventoryState
    {
        SpellInventoryState(int numSpells = ReaperMaxSpells)
        {
            SpellIcons.reserve(numSpells);
        }

        std::vector<SpellInventoryIcon> SpellIcons;
    };
}