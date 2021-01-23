#pragma once

#include "GameObject/Public/Components/Jetpack.h"

namespace AI
{
    class JetpackUtil
    {
    public:
        static SBYTE CalcThrustForHeight(GameObj::Jetpack& component, Object* obj, float minHeight, float height);
    };
}
