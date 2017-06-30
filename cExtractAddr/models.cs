using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cExtractAddr
{
    internal class models
    {
        public class Provinces
        {
            public string ProvinceCode { get; set; }
            public string ProvinceName { get; set; }
            public List<Districts> liDistrict { get; set; }
        }

        public class Districts
        {
            public string District { get; set; }
            public List<SubDistricts> liSubDistrict { get; set; }
        }

        public class SubDistricts
        {
            public string SubDistrict { get; set; }
        }
    }
}
