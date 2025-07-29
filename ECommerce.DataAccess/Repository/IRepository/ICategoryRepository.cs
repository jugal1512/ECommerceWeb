using ECommerce.Models;

namespace ECommerce.DataAccess.Repository.IRepository
{
    public interface ICategoryRepository : IRepository<Category>
    {
        void Update(Category obj); 
    }
}
