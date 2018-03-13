using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication.Domain
{
    public class Sample
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}