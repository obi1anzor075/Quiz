using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Models
{
    public class User:IdentityUser
    {
        public int? Id { get; set; }
        public string? GoogleId { get; set; }
        public string? Email { get; set; }
        [StringLength(100)]
        [MaxLength(100)]
        [Required]
        public string? Name { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}