using Cronos;
using Shortener.Azure.Entities;

namespace Cloud5mins.domain
{
    public static class ScheduleExtensions
    {
        public static bool IsActive(this Schedule schedule, DateTime pointInTime)
        {
            var bufferStart = pointInTime.AddMinutes(-schedule.DurationMinutes);
            var expires = pointInTime.AddMinutes(schedule.DurationMinutes);

            CronExpression expression = CronExpression.Parse(schedule.Cron);
            var occurences = expression.GetOccurrences(bufferStart, expires);

            foreach (DateTime d in occurences)
            {
                if (d < pointInTime && d < expires)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
