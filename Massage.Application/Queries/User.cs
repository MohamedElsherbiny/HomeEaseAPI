using Massage.Application.DTOs;
using Massage.Application.Queries;
using MediatR;
using AutoMapper;
using Massage.Application.Exceptions;
using Massage.Application.Interfaces;
using Massage.Application.Interfaces.Services;

namespace Massage.Application.Queries
{
    public class GetUserByIdQuery : IRequest<UserDto>
    {
        public Guid UserId { get; set; }

        public GetUserByIdQuery(Guid userId)
        {
            UserId = userId;
        }
    }

    public class GetUserPreferencesQuery : IRequest<UserPreferencesDto>
    {
        public Guid UserId { get; set; }

        public GetUserPreferencesQuery(Guid userId)
        {
            UserId = userId;
        }
    }

    public class GetUserAddressesQuery : IRequest<IEnumerable<AddressDto>>
    {
        public Guid UserId { get; set; }

        public GetUserAddressesQuery(Guid userId)
        {
            UserId = userId;
        }
    }

    public class GetAllUsersQuery : IRequest<IEnumerable<UserDto>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchTerm { get; set; }
        public string SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
    }
}


namespace Massage.Application.Handlers.QueryHandlers
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GetUserByIdQueryHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserByIdAsync(request.UserId);
            if (user == null)
                throw new NotFoundException($"User with ID {request.UserId} not found.");

            return _mapper.Map<UserDto>(user);
        }
    }

    public class GetUserPreferencesQueryHandler : IRequestHandler<GetUserPreferencesQuery, UserPreferencesDto>
    {
        private readonly IUserPreferencesRepository _preferencesRepository;
        private readonly IMapper _mapper;

        public GetUserPreferencesQueryHandler(IUserPreferencesRepository preferencesRepository, IMapper mapper)
        {
            _preferencesRepository = preferencesRepository;
            _mapper = mapper;
        }

        public async Task<UserPreferencesDto> Handle(GetUserPreferencesQuery request, CancellationToken cancellationToken)
        {
            var preferences = await _preferencesRepository.GetByUserIdAsync(request.UserId);
            if (preferences == null)
                throw new NotFoundException($"Preferences for user {request.UserId} not found.");

            return _mapper.Map<UserPreferencesDto>(preferences);
        }
    }

    public class GetUserAddressesQueryHandler : IRequestHandler<GetUserAddressesQuery, IEnumerable<AddressDto>>
    {
        private readonly IAddressRepository _addressRepository;
        private readonly IMapper _mapper;

        public GetUserAddressesQueryHandler(IAddressRepository addressRepository, IMapper mapper)
        {
            _addressRepository = addressRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AddressDto>> Handle(GetUserAddressesQuery request, CancellationToken cancellationToken)
        {
            var addresses = await _addressRepository.GetByUserIdAsync(request.UserId);
            return _mapper.Map<IEnumerable<AddressDto>>(addresses);
        }
    }

    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, IEnumerable<UserDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GetAllUsersQueryHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _userRepository.GetAllAsync(
                request.Page,
                request.PageSize,
                request.SearchTerm,
                request.SortBy,
                request.SortDescending);

            return _mapper.Map<IEnumerable<UserDto>>(users);
        }
    }
}

