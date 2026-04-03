using Application.Contracts;
using Application.DTOs;
using AutoMapper;
using MediatR;

namespace Application.Features.Products.Queries
{
    public class GetAllProductsQuery : IRequest<IEnumerable<ProductDto>>
    {
        public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, IEnumerable<ProductDto>>
        {
            private readonly IProductRepository _repository;
            private readonly IMapper _mapper;

            public GetAllProductsQueryHandler(IProductRepository repository, IMapper mapper)
            {
                _repository = repository;
                _mapper = mapper;
            }

            public async Task<IEnumerable<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
            {
                var products = await _repository.GetAllAsync(cancellationToken);
                return _mapper.Map<IEnumerable<ProductDto>>(products);
            }
        }
    }
}