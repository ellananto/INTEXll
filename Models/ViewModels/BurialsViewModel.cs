using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INTEXll.Models.ViewModels
{
    public class BurialsViewModel
    {
        public IQueryable<Burialmain> Burials { get; set; }
        public PageInfo PageInfo { get; set; }
    }
}
