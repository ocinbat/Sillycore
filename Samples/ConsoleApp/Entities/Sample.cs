using System;
using Sillycore.Domain.Abstractions;

namespace ConsoleApp.Entities
{
    public class Sample : IEntity<long>, IAuditable, ISoftDeletable
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public int Size { get; set; }

        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }

        public bool IsDeleted { get; set; }
    }
}