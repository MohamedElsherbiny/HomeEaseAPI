using AutoMapper;
using HomeEase.Application.Commands.UserCommends;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces;
using HomeEase.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Commands.UserCommends
{
    public class CreateUserServiceLikeCommand : IRequest<UserServiceLikeDto>
    {
        public Guid UserId { get; set; }
        public Guid ServiceId { get; set; }
    }
}



public class CreateUserServiceLikeCommandHandler : IRequestHandler<CreateUserServiceLikeCommand, UserServiceLikeDto>
{
    private readonly IUserServiceLikeRepository _repository;
    private readonly IMapper _mapper;

    public CreateUserServiceLikeCommandHandler(IUserServiceLikeRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<UserServiceLikeDto> Handle(CreateUserServiceLikeCommand request, CancellationToken cancellationToken)
    {
        var like = new UserServiceLike
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            ServiceId = request.ServiceId,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(like);
        return _mapper.Map<UserServiceLikeDto>(like);
    }
}