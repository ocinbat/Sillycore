using System;
using Microsoft.Extensions.DependencyInjection;
using Sillycore.Domain.Abstractions;

namespace Sillycore.Extensions
{
    public static class DateTimeExtensions
    {
        private static readonly IHolidayProvider HolidayProvider = SillycoreApp.Instance.ServiceProvider.GetService<IHolidayProvider>();

        /// <summary>
        /// Returns a new <see cref="DateTime"/> that adds the specified number of work days to the value of this instance.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="days">Number of work days to add to specified instance.</param>
        /// <param name="isStaturdayWorkDay">Indicated whether to count Saturday as a work day or not.</param>
        /// <exception cref="ArgumentOutOfRangeException">Throws when trying to add negative days.</exception>
        public static DateTime AddWorkDays(this DateTime input, int days, bool isStaturdayWorkDay = true)
        {
            DateTime returnDate = new DateTime(input.Ticks);

            for (int i = 0; i < days; i++)
            {
                returnDate = returnDate.AddDays(1);

                if ((returnDate.DayOfWeek == DayOfWeek.Saturday && !isStaturdayWorkDay) || returnDate.DayOfWeek == DayOfWeek.Sunday || (HolidayProvider?.IsHoliday(returnDate) ?? false))
                {
                    days++;
                }
            }

            return returnDate;
        }
    }
}