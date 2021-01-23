#pragma once

#include "DebugDraw.h"
#include "DetourDebugDraw.h"

namespace Nav
{
	class PathDebugDraw : public duDebugDraw
	{
	public:
		//////////////////////////////////////////////////
		// Recast interface implementation
		virtual void depthMask(bool state) { }

		virtual void texture(bool state) { }

		virtual void begin(duDebugDrawPrimitives prim, float size = 1.0f) override
		{
			m_primitiveType = prim;
		}

		virtual void vertex(const float* pos, unsigned int color) override
		{
			vertex(pos[0], pos[1], pos[2], color, 0.0f, 0.0f);
		}

		virtual void vertex(const float x, const float y, const float z, unsigned int color) override
		{
			vertex(x, y, z, color, 0.0f, 0.0f);
		}

		virtual void vertex(const float* pos, unsigned int color, const float* uv) override
		{
			vertex(pos[0], pos[1], pos[2], color, uv[0], uv[1]);
		}

		virtual void vertex(const float x, const float y, const float z, unsigned int color, const float u, const float v) override;

		virtual void end() override;

		//////////////////////////////////////////////////
		// Giants-specific code

		void StartFrame();
		void EndFrame();

	private:
		std::vector<P3D> m_lineVertices;
		std::vector<uint> m_lineColors;

		duDebugDrawPrimitives m_primitiveType = DU_DRAW_LINES;
	};

	extern PathDebugDraw g_PathDebugDraw;
}