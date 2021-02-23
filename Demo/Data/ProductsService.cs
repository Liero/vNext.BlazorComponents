using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace vNext.BlazorComponents.Demo.Data
{
    public class ProductsService
    {
        private static readonly string[] Names = new[]
        {
            "Toy", "Tool", "Book",
        };
        public async Task<Product[]> GetProducts()
        {
            await Task.Delay(200);
            return Enumerable.Range(1, 100).Select(i => new Product
            {
                Id = i,
                Height = 10,
                Width = 5,
                Name = Names[i % Names.Length] + i,
                Length = i % 5 + 1
            }).ToArray();            
        }

        public async Task<DataEnvelope<Product>> GetProducts(int skip, int take)
        {
            var products = await GetProducts();
            return new DataEnvelope<Product>(products.Skip(skip).Take(take), products.Length);
        }
    }
}
