using System;

namespace Sillycore.EntityFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SequenceAttribute : Attribute
    {
        public string Name { get; set; }
    }
}