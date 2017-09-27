using System;
using Sillycore.Domain.Abstractions;

namespace Sillycore.Domain.Objects.DateTimeProviders
{
    public class LocalDateTimeProvider : IDateTimeProvider
    {
        public DateTimeKind Kind => DateTimeKind.Local;
        public DateTime Now => DateTime.Now;
    }
}