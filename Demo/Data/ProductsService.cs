using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using vNext.BlazorComponents.Data;

namespace vNext.BlazorComponents.Demo.Data
{
    public class ProductsService
    {
        private static readonly string[] Categories = new[]
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
                Name = Categories[i % Categories.Length] + i,
                Category = Categories[i % Categories.Length],
                Length = i % 5 + 1
            }).ToArray();            
        }

        public async Task<DataEnvelope<Product>> GetProducts(int skip, int take, SortDescriptor[] sortBy = null, IFilterDescriptor[] filters = null)
        {
            await Task.Delay(50);
            var query = (await GetProducts()).AsQueryable();           
            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    var predicate = CustomFilterPredicates.CreatePredicate<Product>(filter, addNullChecks: true /* false when using EF Core*/);
                    query = query.Where(predicate);
                }
            }
            if (sortBy != null)
            {
                foreach (var sort in sortBy)
                {
                    var lambda = FieldUtils.CreatePropertyLambda<Product>(sort.Field);
                    lambda = lambda.AddNullChecks(); // do this only when filtering in memory. Not needed in EF Core.
                    query = query.OrderBy(lambda, sort.Descending, thenBy: sort != sortBy[0]);
                }
            }
            var totalCount = query.Count();
            return new DataEnvelope<Product>(query.Skip(skip).Take(take), totalCount);
        }
    }
}
