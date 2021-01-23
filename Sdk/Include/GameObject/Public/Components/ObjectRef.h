#pragma once

class Object;

namespace GameObj
{
    // Interface back to legacy Object pointer from ECS land.
    struct ObjectRef
    {
        ObjectRef(Object* object) : Object(object) { }

        [[nodiscard]] Object* GetObj() const { return this->Object; }

    private:
        Object* Object;
    };
}