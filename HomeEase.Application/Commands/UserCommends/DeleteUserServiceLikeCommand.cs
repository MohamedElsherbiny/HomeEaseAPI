using HomeEase.Application.Commands.UserCommends;
using HomeEase.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Commands.UserCommends
{
    public class DeleteUserServiceLikeCommand : IRequest<Unit>
    {
        public Guid Id { get; set; }
        public DeleteUserServiceLikeCommand(Guid id) => Id = id;
    }
}


public class DeleteUserServiceLikeCommandHandler : IRequestHandler<DeleteUserServiceLikeCommand, Unit>
{
    private readonly IUserServiceLikeRepository _repository;

    public DeleteUserServiceLikeCommandHandler(IUserServiceLikeRepository repository)
    {
        _repository = repository;
    }

    public async Task<Unit> Handle(DeleteUserServiceLikeCommand request, CancellationToken cancellationToken)
    {
        var like = await _repository.GetByIdAsync(request.Id);
        if (like != null)
        {
            await _repository.DeleteAsync(like);
        }
        return Unit.Value;
    }
}