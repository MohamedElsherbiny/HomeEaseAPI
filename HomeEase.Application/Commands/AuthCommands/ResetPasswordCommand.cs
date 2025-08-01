using FluentValidation;
using FluentValidation.Results;
using HomeEase.Application.DTOs;
using HomeEase.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;


namespace HomeEase.Application.Commands.AuthCommands;

public class ResetPasswordCommand(PasswordResetDto dto) : IRequest<bool>
{
    public string OtpCode { get; set; } = dto.OtpCode;
    public string Email { get; set; } = dto.Email;
    public string NewPassword { get; set; } = dto.NewPassword;
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
{
    private readonly UserManager<User> _userManager;

    public ResetPasswordCommandHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("Email", "User not found.")
            });
        }

        var isValid = await _userManager.VerifyUserTokenAsync(user, "OtpProvider", "ResetPassword", request.OtpCode);
        if (!isValid)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("OtpCode", "Invalid or expired code.")
            });
        }

        var result = await _userManager.ResetPasswordAsync(user, await _userManager.GeneratePasswordResetTokenAsync(user), request.NewPassword);
        if (!result.Succeeded)
        {
            throw new ValidationException(result.Errors.Select(e => new ValidationFailure("NewPassword", e.Description)));
        }

        return true;
    }
}


public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("The Email field is required.")
            .EmailAddress().WithMessage("The Email field is not a valid e-mail address.");

        RuleFor(x => x.OtpCode)
            .NotEmpty().WithMessage("The OtpCode field is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("The NewPassword field is required.")
            .MinimumLength(6).WithMessage("The NewPassword must be at least 6 characters long.");
    }
}


public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        var errors = exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        var response = new
        {
            message = errors
        };

        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var response = new
        {
            message = new Dictionary<string, string[]> { { "General", new[] { exception.Message } } }
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}
