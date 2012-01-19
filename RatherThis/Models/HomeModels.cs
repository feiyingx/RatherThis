using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RatherThis.Domain.Entities;

namespace RatherThis.Models
{
    public class HomeIndexViewModel
    {
        public List<Question> Questions { get; set; }
    }
}