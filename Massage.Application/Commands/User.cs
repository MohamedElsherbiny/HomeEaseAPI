using Massage.Application.Commands;
using Massage.Application.DTOs;
using Massage.Domain.Entities;
using MediatR;
using AutoMapper;
using Massage.Application.Exceptions;
using Massage.Application.Interfaces;
using Massage.Application.Interfaces.Services;


// Commands
namespace Massage.Application.Commands
{
    public class UpdateUserCommand : IRequest<UserDto>
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string ProfileImageUrl { get; set; }

        public UpdateUserCommand(Guid userId, UpdateUserDto dto)
        {
            UserId = userId;
            FirstName = dto.FirstName;
            LastName = dto.LastName;
            PhoneNumber = dto.PhoneNumber;
            ProfileImageUrl = dto.ProfileImageUrl;
        }
    }

    public class ChangePasswordCommand : IRequest<bool>
    {
        public Guid UserId { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }

        public ChangePasswordCommand(Guid userId, ChangePasswordDto dto)
        {
            UserId = userId;
            CurrentPassword = dto.CurrentPassword;
            NewPassword = dto.NewPassword;
        }
    }

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

    public class AddUserAddressCommand : IRequest<AddressDto>
    {
        public Guid UserId { get; set; }
        public AddressDto Address { get; set; }

        public AddUserAddressCommand(Guid userId, AddressDto address)
        {
            UserId = userId;
            Address = address;
        }
    }

    public class UpdateUserAddressCommand : IRequest<AddressDto>
    {
        public Guid UserId { get; set; }
        public Guid AddressId { get; set; }
        public AddressDto Address { get; set; }

        public UpdateUserAddressCommand(Guid userId, Guid addressId, AddressDto address)
        {
            UserId = userId;
            AddressId = addressId;
            Address = address;
        }
    }

    public class DeleteUserAddressCommand : IRequest<bool>
    {
        public Guid UserId { get; set; }
        public Guid AddressId { get; set; }

        public DeleteUserAddressCommand(Guid userId, Guid addressId)
        {
            UserId = userId;
            AddressId = addressId;
        }
    }

    public class DeactivateUserCommand : IRequest<bool>
    {
        public Guid UserId { get; set; }

        public DeactivateUserCommand(Guid userId)
        {
            UserId = userId;
        }
    }
}


// Command Handlers
namespace Massage.Application.Handlers.CommandHandlers
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateUserCommandHandler(IUserRepository userRepository, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserByIdAsync(request.UserId);
            if (user == null)
                throw new NotFoundException($"User with ID {request.UserId} not found.");

            user.FirstName = request.FirstName ?? user.FirstName;
            user.LastName = request.LastName ?? user.LastName;
            user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
            user.ProfileImageUrl = request.ProfileImageUrl ?? user.ProfileImageUrl;
            user.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<UserDto>(user);
        }
    }

    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;

        public ChangePasswordCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserByIdAsync(request.UserId);
            if (user == null)
                throw new NotFoundException($"User with ID {request.UserId} not found.");

            if (!_passwordHasher.VerifyPassword(user.PasswordHash, request.CurrentPassword))
                throw new ValidationException("Current password is incorrect.");

            user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }

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

    public class AddUserAddressCommandHandler : IRequestHandler<AddUserAddressCommand, AddressDto>
    {
        private readonly IAddressRepository _addressRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public AddUserAddressCommandHandler(
            IAddressRepository addressRepository,
            IUserRepository userRepository,
            IMapper mapper,
            IUnitOfWork unitOfWork)
        {
            _addressRepository = addressRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<AddressDto> Handle(AddUserAddressCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserByIdAsync(request.UserId);
            if (user == null)
                throw new NotFoundException($"User with ID {request.UserId} not found.");

            var address = _mapper.Map<Address>(request.Address);
            address.UserId = request.UserId;
            address.Id = Guid.NewGuid();
            address.CreatedAt = DateTime.UtcNow;

            await _addressRepository.AddAsync(address);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<AddressDto>(address);
        }
    }

    public class UpdateUserAddressCommandHandler : IRequestHandler<UpdateUserAddressCommand, AddressDto>
    {
        private readonly IAddressRepository _addressRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateUserAddressCommandHandler(
            IAddressRepository addressRepository,
            IMapper mapper,
            IUnitOfWork unitOfWork)
        {
            _addressRepository = addressRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<AddressDto> Handle(UpdateUserAddressCommand request, CancellationToken cancellationToken)
        {
            var address = await _addressRepository.GetByIdAsync(request.AddressId);
            if (address == null || address.UserId != request.UserId)
                throw new NotFoundException($"Address not found or does not belong to user {request.UserId}.");

            address.Street = request.Address.Street ?? address.Street;
            address.City = request.Address.City ?? address.City;
            address.State = request.Address.State ?? address.State;
            address.PostalCode = request.Address.PostalCode ?? address.PostalCode;
            address.Country = request.Address.Country ?? address.Country;
            address.Latitude = request.Address.Latitude ?? address.Latitude;
            address.Longitude = request.Address.Longitude ?? address.Longitude;
            address.UpdatedAt = DateTime.UtcNow;

            _addressRepository.Update(address);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<AddressDto>(address);
        }
    }

    public class DeleteUserAddressCommandHandler : IRequestHandler<DeleteUserAddressCommand, bool>
    {
        private readonly IAddressRepository _addressRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteUserAddressCommandHandler(
            IAddressRepository addressRepository,
            IUnitOfWork unitOfWork)
        {
            _addressRepository = addressRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeleteUserAddressCommand request, CancellationToken cancellationToken)
        {
            var address = await _addressRepository.GetByIdAsync(request.AddressId);
            if (address == null || address.UserId != request.UserId)
                throw new NotFoundException($"Address not found or does not belong to user {request.UserId}.");

            _addressRepository.Delete(address);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }

    public class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeactivateUserCommandHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserByIdAsync(request.UserId);
            if (user == null)
                throw new NotFoundException($"User with ID {request.UserId} not found.");

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            user.DeactivatedAt = DateTime.UtcNow;

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}


