using System.Runtime.Serialization;

namespace Sillycore.Domain.Enums
{
    public enum OrderType
    {
        [EnumMember(Value = "asc")]
        Asc,

        [EnumMember(Value = "desc")]
        Desc
    }
}