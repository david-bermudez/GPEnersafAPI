using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace GpEnerSaf.Models
{
    public partial class ApplicationUser : IdentityUser
    {
        [IgnoreDataMember]
        public override string PasswordHash { get; set; }

        public string Name { get; set; }

        public string Profile { get; set; }
    }
}

