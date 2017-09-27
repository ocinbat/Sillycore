using System;

namespace Sillycore.Domain.Abstractions
{
    public interface IDateTimeProvider
    {
        DateTimeKind Kind { get; }

        DateTime Now { get; }
    }
}