using System;
using System.ComponentModel.DataAnnotations;

namespace smartcampusserviceportal.Models
{
    public class ServiceRequest
    {
        public int Id { get; set; }

        [Required]
        [StringLength(120)]
        [Display(Name = "Request title")]
        public string Title { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; }

        [Display(Name = "Created")]
        public DateTime CreatedAt { get; set; }
    }
}
