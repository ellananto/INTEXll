using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using INTEXll.Models;

namespace INTEXll.Models
{
    public class EFBurialRepository : IBurialRepository
    {
        private burialContext context { get; set; }
        public EFBurialRepository (burialContext temp)
        {
            context = temp;
        }
        public IQueryable<Burialmain> Burials => context.Burialmain;
    }
}
