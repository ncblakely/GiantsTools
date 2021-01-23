#pragma once

#include "entt/entt.hpp"

void EntityRegistryCreate();
void EntityRegistryDestroy();

extern std::unique_ptr<entt::registry> Registry;