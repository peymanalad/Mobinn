using System;

namespace Chamran.Deed.Info.Dtos
{
    public class CreateLocationsDto 
    {
        public decimal UserLat { get; set; }

        public decimal UserLong { get; set; }
        public DateTime  CreationTime { get; set; }
    }
}