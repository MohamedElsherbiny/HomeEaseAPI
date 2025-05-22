using HomeEase.Application.Commands.ProviderCommands;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Entities;
using HomeEase.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Commands.ProviderCommands
{
    public class UpdateProviderScheduleCommand : IRequest<bool>
    {
        public Guid ProviderId { get; set; }
        public ProviderScheduleDto ScheduleDto { get; set; }
    }
}


// Command Handler
public class UpdateProviderScheduleCommandHandler : IRequestHandler<UpdateProviderScheduleCommand, bool>
{
    private readonly IProviderRepository _providerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProviderScheduleCommandHandler(IProviderRepository providerRepository, IUnitOfWork unitOfWork)
    {
        _providerRepository = providerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateProviderScheduleCommand request, CancellationToken cancellationToken)
    {
        var provider = await _providerRepository.GetByIdAsync(request.ProviderId);
        if (provider == null)
            return false;

        if (provider.Schedule == null)
        {
            provider.Schedule = new ProviderSchedule();
        }

        if (request.ScheduleDto.RegularHours != null && request.ScheduleDto.RegularHours.Any())
        {
            provider.Schedule.RegularHours = request.ScheduleDto.RegularHours.Select(wh => new WorkingHours
            {
                DayOfWeek = (DayOfWeek)wh.DayOfWeek,
                StartTime = wh.StartTime,
                EndTime = wh.EndTime,
                IsOpen = wh.IsOpen
            }).ToList();
        }

        if (request.ScheduleDto.SpecialDates != null && request.ScheduleDto.SpecialDates.Any())
        {
            provider.Schedule.SpecialDates = request.ScheduleDto.SpecialDates.Select(sd => new SpecialDate
            {
                Date = sd.Date,
                StartTime = sd.StartTime,
                EndTime = sd.EndTime,
                IsClosed = sd.IsClosed,
                Note = sd.Note
            }).ToList();
        }

        if (request.ScheduleDto.AvailableSlots != null && request.ScheduleDto.AvailableSlots.Any())
        {
            provider.Schedule.AvailableSlots = request.ScheduleDto.AvailableSlots.Select(ts => new TimeSlot
            {
                StartTime = ts.StartTime,
                EndTime = ts.EndTime,
                IsAvailable = ts.IsAvailable
            }).ToList();
        }

        _providerRepository.Update(provider);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}