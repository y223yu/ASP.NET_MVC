using System;
using System.Collections.Generic;

namespace YYSail.Models
{
    public partial class Province
    {
        public Province()
        {
            Member = new HashSet<Member>();
            Town = new HashSet<Town>();
        }

        public string ProvinceCode { get; set; }
        public string Name { get; set; }
        public string CountryCode { get; set; }
        public string TaxCode { get; set; }
        public double TaxRate { get; set; }
        public string Capital { get; set; }

        public Country CountryCodeNavigation { get; set; }
        public ICollection<Member> Member { get; set; }
        public ICollection<Town> Town { get; set; }
    }
}
