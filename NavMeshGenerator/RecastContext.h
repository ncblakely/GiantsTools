#pragma once

#include "Recast.h"
#include "Framework/PerfTimer.h"

class RecastContext : public rcContext
{
public:
	RecastContext(bool enableLogging);

protected:
	virtual void doResetLog();
	virtual void doLog(const rcLogCategory category, const char* msg, const int len);
	virtual void doResetTimers();
	virtual void doStartTimer(const rcTimerLabel label);
	virtual void doStopTimer(const rcTimerLabel label);
	virtual int doGetAccumulatedTime(const rcTimerLabel label) const;

private:
	bool m_enableLogging{};
	TimeVal m_startTime[RC_MAX_TIMERS]{};
	TimeVal m_accTime[RC_MAX_TIMERS]{};
};