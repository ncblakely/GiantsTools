#pragma once

namespace GameObj
{
	struct ThrustParameters
	{
		float ThrustMaxHeight{};
		float ThrustPower{};
		float ThrustLowPct{};
		float ThrustHighPct{};
		float ThrustPowerMax{};
		float ThrustFwdSlowMax{};
		float ThrustDrowning{};
	};

    struct Jetpack
    {
		Jetpack(
			const ThrustParameters& thrustParameters)
			: ThrustParameters(thrustParameters)
		{
		}

		ThrustParameters ThrustParameters;
		SBYTE ThrustControl{}; // Thrust control state
    };
}