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

        [Required]
        public string Category { get; set; }

        [Range(0, 100)]
        public double Height { get; set; }

        [Range(0, 100)]
        public double Width { get; set; }

        [Range(0, 200)]
        public double Length { get; set; }
        public double Volume => Height * Width * Length;

        public double Rating { get; set; }
        public bool? NullableBoolean => Id % 3 == 0 ? null : true;

        public ProductDetails Details { get; set; }
    }

    public class ProductDetails
    {
        public DateTime? DateTimeProperty => DateTime.Now;
        public string Manufacturer { get; set; }
    }

}
