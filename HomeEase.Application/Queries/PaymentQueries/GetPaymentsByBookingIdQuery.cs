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
    public class GetPaymentsByBookingIdQuery : IRequest<IEnumerable<PaymentInfoDto>>
    {
        public Guid BookingId { get; set; }
    }
}


public class GetPaymentsByBookingIdQueryHandler : IRequestHandler<GetPaymentsByBookingIdQuery, IEnumerable<PaymentInfoDto>>
{
    private readonly IPaymentInfoRepository _paymentInfoRepository;
    private readonly IMapper _mapper;

    public GetPaymentsByBookingIdQueryHandler(IPaymentInfoRepository paymentInfoRepository, IMapper mapper)
    {
        _paymentInfoRepository = paymentInfoRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PaymentInfoDto>> Handle(GetPaymentsByBookingIdQuery request, CancellationToken cancellationToken)
    {
        var payments = await _paymentInfoRepository.GetByBookingIdAsync(request.BookingId);
        return _mapper.Map<IEnumerable<PaymentInfoDto>>(payments);
    }
}