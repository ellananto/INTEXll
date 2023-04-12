using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INTEXll.Models
{
    public class HousingData
    {
        public float SquareNorthSouth { get; set; }
        public float Depth { get; set; }
        public float SouthToHead { get; set; }
        public float FieldbookPage { get; set; }
        public float SquareEastwest { get; set; }
        public float WestToHead { get; set; }
        public float Length { get; set; }
        public float BurialNumber { get; set; }
        public float WestToFeet { get; set; }
        public float SouthToFeet { get; set; }
        public float FieldbookExcavationYear { get; set; }
        public float HeadDirection_E { get; set; }
        public float HeadDirection_W { get; set; }
        public float NorthSouth_N { get; set; }
        public float EastWest_E { get; set; }
        public float EastWest_W { get; set; }
        public float AdultSubAdult_A { get; set; }
        public float AdultSubAdult_C { get; set; }
        public float SamplesCollected_Other { get; set; }
        public float SamplesCollected_False { get; set; }
        public float SamplesCollected_True { get; set; }
        public float Area_NE { get; set; }
        public float Area_NW { get; set; }
        public float Area_SE { get; set; }
        public float Area_SW { get; set; }
        public float AgeAtDeath_A { get; set; }
        public float AgeAtDeath_C { get; set; }
        public float AgeAtDeath_I { get; set; }
        public float AgeAtDeath_N { get; set; }
        public float AgeAtDeath_Other { get; set; }

        public Tensor<float> AsTensor()
        {
            float[] data = new float[]
            {
                SquareNorthSouth, Depth, SouthToHead, FieldbookPage, SquareEastwest, WestToHead, Length, BurialNumber, WestToFeet, SouthToFeet, FieldbookExcavationYear,
                HeadDirection_E, HeadDirection_W, NorthSouth_N, EastWest_E, EastWest_W, AdultSubAdult_A, AdultSubAdult_C, SamplesCollected_Other, SamplesCollected_False,
                SamplesCollected_True, Area_NE, Area_NW, Area_SE, Area_SW, AgeAtDeath_A, AgeAtDeath_C, AgeAtDeath_I, AgeAtDeath_N, AgeAtDeath_Other
            };
            int[] dimensions = new int[] { 1, 30 };
            return new DenseTensor<float>(data, dimensions);
        }
    }
}
