using Cronos;
using Shortener.Azure.Pocos;

namespace Shortener.AzureServices.Extensions
{
    public static class ScheduleExtensions
    {
        public static bool IsActive(this Schedule schedule, DateTime pointInTime)
        {
            var bufferStart = pointInTime.AddMinutes(-schedule.DurationMinutes);
            var expires = pointInTime.AddMinutes(schedule.DurationMinutes);

            CronExpression expression = CronExpression.Parse(schedule.Cron);
            var occurences = expression.GetOccurrences(bufferStart, expires);

            return occurences.Any(d => d < pointInTime && d < expires);
        }
    }
}
