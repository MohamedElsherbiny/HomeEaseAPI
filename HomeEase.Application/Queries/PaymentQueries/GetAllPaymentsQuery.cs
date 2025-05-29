using AutoMapper;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Repos;
using HomeEase.Application.Queries.PaymentQueries;
using HomeEase.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Queries.PaymentQueries
{
    public class GetAllPaymentsQuery : IRequest<PaginatedList<PaymentInfoDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}


public class GetAllPaymentsQueryHandler : IRequestHandler<GetAllPaymentsQuery, PaginatedList<PaymentInfoDto>>
{
    private readonly IPaymentInfoRepository _paymentInfoRepository;
    private readonly IMapper _mapper;

    public GetAllPaymentsQueryHandler(IPaymentInfoRepository paymentInfoRepository, IMapper mapper)
    {
        _paymentInfoRepository = paymentInfoRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedList<PaymentInfoDto>> Handle(GetAllPaymentsQuery request, CancellationToken cancellationToken)
    {
        var payments = await _paymentInfoRepository.GetAllAsync(request.PageNumber, request.PageSize);
        var totalCount = payments.Count();
        var dtos = _mapper.Map<List<PaymentInfoDto>>(payments); // Changed to List<PaymentInfoDto>
        return new PaginatedList<PaymentInfoDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}