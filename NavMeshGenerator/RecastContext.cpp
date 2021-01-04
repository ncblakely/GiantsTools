#include "RecastContext.h"

#include <windows.h>

RecastContext::RecastContext(bool enableLogging)
	: m_enableLogging(enableLogging)
{
}

void RecastContext::doResetLog()
{
}

void RecastContext::doLog(const rcLogCategory category, const char* msg, const int len)
{
	if (m_enableLogging)
		printf("RECAST: %s\n", msg);
}

void RecastContext::doResetTimers()
{
	for (int i = 0; i < RC_MAX_TIMERS; ++i)
		m_accTime[i] = -1;
}

void RecastContext::doStartTimer(const rcTimerLabel label)
{
	m_startTime[label] = getPerfTime();
}

void RecastContext::doStopTimer(const rcTimerLabel label)
{
	const TimeVal endTime = getPerfTime();
	const TimeVal deltaTime = endTime - m_startTime[label];
	if (m_accTime[label] == -1)
		m_accTime[label] = deltaTime;
	else
		m_accTime[label] += deltaTime;
}

int RecastContext::doGetAccumulatedTime(const rcTimerLabel label) const
{
	return getPerfTimeUsec(m_accTime[label]);
}
