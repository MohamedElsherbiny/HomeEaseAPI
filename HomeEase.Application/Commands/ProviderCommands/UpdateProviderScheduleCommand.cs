using HomeEase.Application.DTOs.Common;
using HomeEase.Application.DTOs.Provider;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Entities;
using HomeEase.Domain.Repositories;
using HomeEase.Resources;
using MediatR;

namespace HomeEase.Application.Commands.ProviderCommands;

public class UpdateProviderScheduleCommand : IRequest<EntityResult>
{
    public Guid ProviderId { get; set; }
    public ProviderScheduleDto ScheduleDto { get; set; }
}


public class UpdateProviderScheduleCommandHandler(IProviderRepository providerRepository, IUnitOfWork unitOfWork) : IRequestHandler<UpdateProviderScheduleCommand, EntityResult>
{
    public async Task<EntityResult> Handle(UpdateProviderScheduleCommand request, CancellationToken cancellationToken)
    {
        var provider = await providerRepository.GetByIdAsync(request.ProviderId);
        if (provider is null)
        {
            return EntityResult.Failed(new EntityError(nameof(Messages.ProviderNotFound), string.Format(Messages.ProviderNotFound, request.ProviderId)));
        }

        provider.Schedule ??= new ProviderSchedule
        {
            Id = Guid.NewGuid(),
        };

        // Regular Hours
        if (request.ScheduleDto.RegularHours != null && request.ScheduleDto.RegularHours.Any())
        {
            provider.Schedule.RegularHours = request.ScheduleDto.RegularHours.Select(wh => new WorkingHours
            {
                Id = wh.Id ?? Guid.NewGuid(),
                DayOfWeek = (DayOfWeek)wh.DayOfWeek,
                StartTime = wh.StartTime,
                EndTime = wh.EndTime,
                IsOpen = wh.IsOpen
            }).ToList();
        }

        // Special Dates
        if (request.ScheduleDto.SpecialDates != null && request.ScheduleDto.SpecialDates.Any())
        {
            provider.Schedule.SpecialDates = request.ScheduleDto.SpecialDates.Select(sd => new SpecialDate
            {
                Id = sd.Id ?? Guid.NewGuid(),
                Date = sd.Date,
                StartTime = sd.StartTime,
                EndTime = sd.EndTime,
                IsClosed = sd.IsClosed,
                Note = sd.Note
            }).ToList();
        }

        // Available Slots
        if (request.ScheduleDto.AvailableSlots != null && request.ScheduleDto.AvailableSlots.Any())
        {
            provider.Schedule.AvailableSlots = request.ScheduleDto.AvailableSlots.Select(ts => new TimeSlot
            {
                Id = ts.Id ?? Guid.NewGuid(),
                StartTime = ts.StartTime,
                EndTime = ts.EndTime,
                IsAvailable = ts.IsAvailable
            }).ToList();
        }

        providerRepository.Update(provider);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return EntityResult.Success;
    }
}