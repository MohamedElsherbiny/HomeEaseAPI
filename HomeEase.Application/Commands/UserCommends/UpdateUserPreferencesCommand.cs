using AutoMapper;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Application.Interfaces;
using HomeEase.Domain.Entities;
using MediatR;
using HomeEase.Application.Commands.UserCommends;
using HomeEase.Domain.Exceptions;

namespace HomeEase.Application.Commands.UserCommends;

public class UpdateUserPreferencesCommand(UserPreferencesDto dto) : IRequest<UserPreferencesDto>
{
    public Guid UserId { get; set; } = dto.UserId;
    public bool EmailNotifications { get; set; } = dto.EmailNotifications;
    public bool SmsNotifications { get; set; } = dto.SmsNotifications;
    public string PreferredCurrency { get; set; } = dto.PreferredCurrency;
    public string[] FavoriteServiceTypes { get; set; } = dto.FavoriteServiceTypes;
}

public class UpdateUserPreferencesCommandHandler(
    IUserPreferencesRepository _preferencesRepository,
    IUserRepository _userRepository,
    IMapper _mapper,
    IUnitOfWork _unitOfWork) : IRequestHandler<UpdateUserPreferencesCommand, UserPreferencesDto>
{
    public async Task<UserPreferencesDto> Handle(UpdateUserPreferencesCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByIdAsync(request.UserId);
        if (user is null)
        {
            throw new BusinessException($"User with ID {request.UserId} not found.");
        }

        var preferences = await _preferencesRepository.GetByUserIdAsync(request.UserId);
        if (preferences is null)
        {
            preferences = new UserPreferences
            {
                UserId = request.UserId,
                EmailNotifications = request.EmailNotifications,
                SmsNotifications = request.SmsNotifications,
                PreferredCurrency = request.PreferredCurrency,
                FavoriteServiceTypes = request.FavoriteServiceTypes
            };
            await _preferencesRepository.AddAsync(preferences);
        }
        else
        {
            preferences.EmailNotifications = request.EmailNotifications;
            preferences.SmsNotifications = request.SmsNotifications;
            preferences.PreferredCurrency = request.PreferredCurrency;
            preferences.FavoriteServiceTypes = request.FavoriteServiceTypes;
            _preferencesRepository.Update(preferences);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserPreferencesDto>(preferences);
    }
}