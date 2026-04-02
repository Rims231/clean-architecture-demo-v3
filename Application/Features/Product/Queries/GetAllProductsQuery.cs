using Application.Contracts;
using Application.DTOs;
using MediatR;

namespace Application.Features.Products.Queries
{
    public class GetAllProductsQuery : IRequest<IEnumerable<ProductDto>>
    {
        public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, IEnumerable<ProductDto>>
        {
            private readonly IProductRepository _repository;

            public GetAllProductsQueryHandler(IProductRepository repository)
            {
                _repository = repository;
            }

            public async Task<IEnumerable<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
            {
                var products = await _repository.GetAllAsync(cancellationToken);

                return products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Rate = p.Rate
                });
            }
        }
    }
}