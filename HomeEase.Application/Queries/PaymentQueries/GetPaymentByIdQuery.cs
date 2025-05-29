using AutoMapper;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Repos;
using HomeEase.Application.Queries.PaymentQueries;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Queries.PaymentQueries
{
    public class GetPaymentByIdQuery : IRequest<PaymentInfoDto>
    {
        public Guid Id { get; set; }
    }
}


public class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, PaymentInfoDto>
{
    private readonly IPaymentInfoRepository _paymentInfoRepository;
    private readonly IMapper _mapper;

    public GetPaymentByIdQueryHandler(IPaymentInfoRepository paymentInfoRepository, IMapper mapper)
    {
        _paymentInfoRepository = paymentInfoRepository;
        _mapper = mapper;
    }

    public async Task<PaymentInfoDto> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var paymentInfo = await _paymentInfoRepository.GetByIdAsync(request.Id);
        return paymentInfo == null ? null : _mapper.Map<PaymentInfoDto>(paymentInfo);
    }
}