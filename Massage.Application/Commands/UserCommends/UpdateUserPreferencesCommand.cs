using AutoMapper;
using Massage.Application.DTOs;
using Massage.Application.Exceptions;
using Massage.Application.Interfaces.Services;
using Massage.Application.Interfaces;
using Massage.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Massage.Application.Commands.UserCommends;

namespace Massage.Application.Commands.UserCommends
{
    public class UpdateUserPreferencesCommand : IRequest<UserPreferencesDto>
    {
        public Guid UserId { get; set; }
        public bool EmailNotifications { get; set; }
        public bool SmsNotifications { get; set; }
        public string PreferredCurrency { get; set; }
        public string[] FavoriteServiceTypes { get; set; }

        public UpdateUserPreferencesCommand(UserPreferencesDto dto)
        {
            UserId = dto.UserId;
            EmailNotifications = dto.EmailNotifications;
            SmsNotifications = dto.SmsNotifications;
            PreferredCurrency = dto.PreferredCurrency;
            FavoriteServiceTypes = dto.FavoriteServiceTypes;
        }
    }
}


// Command Handler
public class UpdateUserPreferencesCommandHandler : IRequestHandler<UpdateUserPreferencesCommand, UserPreferencesDto>
{
    private readonly IUserPreferencesRepository _preferencesRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserPreferencesCommandHandler(
        IUserPreferencesRepository preferencesRepository,
        IUserRepository userRepository,
        IMapper mapper,
        IUnitOfWork unitOfWork)
    {
        _preferencesRepository = preferencesRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserPreferencesDto> Handle(UpdateUserPreferencesCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByIdAsync(request.UserId);
        if (user == null)
            throw new NotFoundException($"User with ID {request.UserId} not found.");

        var preferences = await _preferencesRepository.GetByUserIdAsync(request.UserId);
        if (preferences == null)
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

        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<UserPreferencesDto>(preferences);
    }
}