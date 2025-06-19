using AutoMapper;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces;
using HomeEase.Application.Queries.UserQueries;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Queries.UserQueries
{
    public class GetUserServiceLikeByIdQuery : IRequest<UserServiceLikeDto>
    {
        public Guid Id { get; set; }
        public GetUserServiceLikeByIdQuery(Guid id) => Id = id;
    }

}


public class GetUserServiceLikeByIdQueryHandler : IRequestHandler<GetUserServiceLikeByIdQuery, UserServiceLikeDto>
{
    private readonly IUserServiceLikeRepository _repository;
    private readonly IMapper _mapper;

    public GetUserServiceLikeByIdQueryHandler(IUserServiceLikeRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<UserServiceLikeDto> Handle(GetUserServiceLikeByIdQuery request, CancellationToken cancellationToken)
    {
        var like = await _repository.GetByIdAsync(request.Id);
        return like != null ? _mapper.Map<UserServiceLikeDto>(like) : null;
    }
}