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
    public class GetAllUserServiceLikesQuery : IRequest<List<UserServiceLikeDto>>
    {
        public Guid? UserId { get; set; }
        public Guid? ServiceId { get; set; }
    }
}



public class GetAllUserServiceLikesQueryHandler : IRequestHandler<GetAllUserServiceLikesQuery, List<UserServiceLikeDto>>
{
    private readonly IUserServiceLikeRepository _repository;
    private readonly IMapper _mapper;

    public GetAllUserServiceLikesQueryHandler(IUserServiceLikeRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<List<UserServiceLikeDto>> Handle(GetAllUserServiceLikesQuery request, CancellationToken cancellationToken)
    {
        var likes = await _repository.GetAllAsync(request.UserId, request.ServiceId);
        return _mapper.Map<List<UserServiceLikeDto>>(likes);
    }
}