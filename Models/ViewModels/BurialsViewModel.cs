using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INTEXll.Models.ViewModels
{
    public class BurialsViewModel
    {
        public IQueryable<Burialmain> Burials { get; set; }
        public IQueryable<Bodyanalysischart> Bodyanalysischarts { get; set; }
        public IQueryable<Textile> Textiles { get; set; }
        public IQueryable<Color> TextileColors { get; set; }
        public IQueryable<Textilefunction> TextileFunctions { get; set; }
        public IQueryable<Structure> Structure { get; set; }
        public PageInfo PageInfo { get; set; }
        public string CurrentFilter { get; set; }
    }
}
