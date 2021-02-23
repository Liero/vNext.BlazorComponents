using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace vNext.BlazorComponents.Demo.Data
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Range(0, 100)]
        public double Height { get; set; }

        [Range(0, 100)]
        public double Width { get; set; }

        [Range(0, 200)]
        public double Length { get; set; }
        public double Volume => Height * Width * Length;
    }
}
