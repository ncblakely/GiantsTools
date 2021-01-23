#pragma once

#include "Jetpack.h"

namespace ECS
{
#ifdef ENABLE_DEBUG
	template<>
	inline void EntityComponentEditor<GameObj::Jetpack>(GameObj::Jetpack& component)
	{
		if (ImGui::TreeNode("Jetpack"))
		{
			EntityPropertyEditor("ThrustMaxHeight", component.ThrustParameters.ThrustMaxHeight);
			EntityPropertyEditor("ThrustPower", component.ThrustParameters.ThrustPower);
			EntityPropertyEditor("ThrustLowPct", component.ThrustParameters.ThrustLowPct);
			EntityPropertyEditor("ThrustHighPct", component.ThrustParameters.ThrustHighPct);
			EntityPropertyEditor("ThrustPowerMax", component.ThrustParameters.ThrustPowerMax);
			EntityPropertyEditor("ThrustFwdSlowMax", component.ThrustParameters.ThrustDrowning);
			EntityPropertyEditor("ThrustDrowning", component.ThrustParameters.ThrustFwdSlowMax);
			EntityPropertyEditor("ThrustMode", component.ThrustControl);

			ImGui::TreePop();
		}
	}
#endif
}