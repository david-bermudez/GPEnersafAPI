using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GpEnerSaf.Models
{
    public class ADUser
    {
        public string Name { get; set; }
        public string Mail { get; set; }
        public string GivenName { get; set; }
        public string Sn { get; set; }
        public string UserPrincipalName { get; set; }
        public string DistinguishedName { get; set; }
    }
}