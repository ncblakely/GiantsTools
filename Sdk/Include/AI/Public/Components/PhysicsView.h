#pragma once

namespace AI
{
    struct PhysicsView
    {
        P3D Location{};
        P3D LastLocation{};
        P3D Velocity{};
        P3D LastVelocity{};
        float Facing{};
        float Speed{};
        float ForwardSpeed{};
        P3D Force{};
    };
}