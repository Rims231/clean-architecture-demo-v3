// ── UpdateProductCommand.cs ──────────────────────────────────────────────────
using Application.Contracts;
using MediatR;

namespace Application.Features.Products.Commands
{
    public class UpdateProductCommand : IRequest<Unit>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Rate { get; set; }

        public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Unit>
        {
            private readonly IProductRepository _repository;

            public UpdateProductCommandHandler(IProductRepository repository)
            {
                _repository = repository;
            }

            public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
            {
                var product = await _repository.GetByIdAsync(request.Id, cancellationToken);
                if (product == null) throw new Exception($"Product with Id {request.Id} not found.");

                product.Name = request.Name;
                product.Description = request.Description;
                product.Rate = request.Rate;
                product.ModifieldOn = DateTime.UtcNow;
                product.ModifieldBy = 1;

                await _repository.UpdateAsync(product, cancellationToken);
                return Unit.Value;
            }
        }
    }
}

