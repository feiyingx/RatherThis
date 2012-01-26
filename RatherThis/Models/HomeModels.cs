using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RatherThis.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace RatherThis.Models
{
    public class ContactViewModel
    {
        public string Type { get; set; }
        [Required(ErrorMessage="Please enter your email address.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$", ErrorMessage = "Invalid email addresss.")]
        public string Email { get; set; }
        [Required(ErrorMessage ="Please enter a description.")]
        public string Description { get; set; }
    }
}