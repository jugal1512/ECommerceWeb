using ECommerce.Models;

namespace ECommerce.DataAccess.Repository.IRepository
{
    public interface IProductRepository : IRepository<Product>
    {
        void Update(Product obj); 
    }
}
