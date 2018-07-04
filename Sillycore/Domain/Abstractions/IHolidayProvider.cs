using System;

namespace Sillycore.Domain.Abstractions
{
    public interface IHolidayProvider
    {
        bool IsHoliday(DateTime date);
    }
}