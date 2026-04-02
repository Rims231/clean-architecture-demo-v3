using Application.Contracts;
using MediatR;

namespace Application.Features.Products.Commands
{
    public class DeleteProductCommand : IRequest<Unit>
    {
        public int Id { get; set; }

        public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Unit>
        {
            private readonly IProductRepository _repository;

            public DeleteProductCommandHandler(IProductRepository repository)
            {
                _repository = repository;
            }

            public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
            {
                await _repository.DeleteAsync(request.Id, cancellationToken);
                return Unit.Value;
            }
        }
    }
}