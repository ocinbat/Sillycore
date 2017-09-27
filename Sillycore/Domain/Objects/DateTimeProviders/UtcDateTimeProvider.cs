using System;
using Sillycore.Domain.Abstractions;

namespace Sillycore.Domain.Objects.DateTimeProviders
{
    public class UtcDateTimeProvider : IDateTimeProvider
    {
        public DateTimeKind Kind => DateTimeKind.Utc;
        public DateTime Now => DateTime.UtcNow;
    }
}