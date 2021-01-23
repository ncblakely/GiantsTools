#pragma once

#include "AI/Public/Goal.h"
#include "Navigation/Public/Path.h"
#include "ECS/Public/Component.h"
#include "Core/Private/EventDispatcher.h"

class Object;

namespace AI
{
	const float DefaultBackwardsMaximumDistance = 30.0f;
	const float DefaultBackwardsMaximumAngle = 40.0f;
	const float DefaultSideMaximumDistance = 50.f;
	const float DefaultSideMaximumAngle = 40.0f;
	const float DefaultSlowDistance = 6.0f;

	struct MoveEnactor
	{
		MoveEnactor(Object* object);

		Object* Object;

		std::shared_ptr<Nav::Path> Path;

		float GetRunMaxSpeed() const { return RunMaxSpeed; }
		float GetSlowRunSpeed() const { return SlowRunSpeed; }
		float GetTurnMaxSpeed() const { return TurnMaxSpeed; }
		
		float BackwardsMaximumDistance = DefaultBackwardsMaximumDistance; // If distance to goal is below threshold, we'll walk/run backwards
		float BackwardsMaximumAngle = DefaultBackwardsMaximumAngle; // If angle to goal is below threshold, we'll walk/run backwards
		float SideMaximumDistance = DefaultSideMaximumDistance; // If distance to goal is below threshold, we'll sidestep
		float SideMaximumAngle = DefaultSideMaximumAngle; // If angle to goal is below threshold, we'll sidestep
		float SlowDistance = DefaultSlowDistance; // Distance at which we'll use the slow run speed instead of max (1.0f)

	private:
		float RunMaxSpeed;
		float SlowRunSpeed;
		float TurnMaxSpeed;
	};
}