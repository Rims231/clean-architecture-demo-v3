using Application.Contracts;
using MediatR;

namespace Application.Features.Products.Commands
{
    public class CreateProductCommand : IRequest<int>
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Rate { get; set; }

        public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
        {
            private readonly IProductRepository _repository;

            public CreateProductCommandHandler(IProductRepository repository)
            {
                _repository = repository;
            }

            public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
            {
                var product = new Domain.Entities.Product
                {
                    Name = request.Name,
                    Description = request.Description,
                    Rate = request.Rate,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = 1
                };

                return await _repository.CreateAsync(product, cancellationToken);
            }
        }
    }
}