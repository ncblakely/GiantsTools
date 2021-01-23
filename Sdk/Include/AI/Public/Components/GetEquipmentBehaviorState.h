#pragma once

namespace AI
{
    struct GetEquipmentBehaviorState
    {
        std::optional<P3D> ShopWantLocation{};
        std::optional<float> StuckTimer{};
    };
}