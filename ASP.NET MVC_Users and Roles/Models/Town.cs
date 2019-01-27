using System;
using System.Collections.Generic;

namespace YSSail.Models
{
    public partial class Town
    {
        public string TownName { get; set; }
        public string ProvinceCode { get; set; }

        public Province ProvinceCodeNavigation { get; set; }
    }
}
