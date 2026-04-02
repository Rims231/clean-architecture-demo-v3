using Domain.Entities;

namespace Application.Contracts
{
    public interface IProductRepository
    {
        
        Task<IEnumerable<Domain.Entities.Product>> GetAllAsync(CancellationToken cancellationToken);
        Task<Domain.Entities.Product?> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<int> CreateAsync(Domain.Entities.Product product, CancellationToken cancellationToken);
        Task UpdateAsync(Domain.Entities.Product product, CancellationToken cancellationToken);
        Task DeleteAsync(int id, CancellationToken cancellationToken);
    }
}