#pragma once

#include "MoveEnactor.h"
#include "../MoveUtil.h"

namespace ECS
{
#ifdef ENABLE_DEBUG
	template<>
	inline void EntityComponentEditor<AI::MoveEnactor>(AI::MoveEnactor& component)
	{
		if (ImGui::TreeNode("MoveEnactor"))
		{
			if (component.Path)
			{
				for (size_t i = 0; i < component.Path->MoveGoals.size(); i++)
				{
					const auto& goal = component.Path->MoveGoals[i];

					ImGui::Text("%sMoveGoal: %.2f %.2f %.2f", i == component.Path->CurrentGoal ? "(Active) " : "", goal.loc.x, goal.loc.y, goal.loc.z);
				}
			}

			if (ImGui::Button("Move to Player"))
			{
				/*
				MoveUtil::ClearAllGoals(component);

				AI::MoveGoal goal;
				goal.loc = PlayerObj->location;
				FlagSet(goal.gflags, AI::GoalFlag::Is3D | AI::GoalFlag::Flyer);

				component.Path.MoveGoals.push_back(goal);
				*/
			}

			if (ImGui::Button("Clear Goals"))
			{
				AI::MoveUtil::ClearAllGoals(component);
			}

			if (ImGui::Button("Move To Shop"))
			{
				
			}

			ImGui::TreePop();
		}
	}
#endif
}
