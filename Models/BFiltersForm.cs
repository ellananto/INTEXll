using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace INTEXll.Models
{
    public class BFiltersForm
    {
            public string SquareNS { get; set; }
            public string NS { get; set; }
            public string SquareEW { get; set; }
            public string EW { get; set; }
            public string Area { get; set; }
            public int? BurialNumber { get; set; }
            public string HeadDirection { get; set; }
            public string AgeAtDeath { get; set; }
            public string Sex { get; set; }
            public string HairColor { get; set; }
        }

    }

