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
    public class GetUserAddressesQuery : IRequest<IEnumerable<AddressDto>>
    {
        public Guid UserId { get; set; }

        public GetUserAddressesQuery(Guid userId)
        {
            UserId = userId;
        }
    }
}


// Query Handler
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