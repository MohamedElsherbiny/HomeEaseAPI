using FluentValidation;
using HomeEase.Application.DTOs;
using HomeEase.Domain.Entities;
using HomeEase.Resources;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;


namespace HomeEase.Application.Commands.AuthCommands;

public class ResetPasswordCommand(PasswordResetDto dto) : IRequest<EntityResult>
{
    public string OtpCode { get; set; } = dto.OtpCode;
    public string Email { get; set; } = dto.Email;
    public string NewPassword { get; set; } = dto.NewPassword;
}

public class ResetPasswordCommandHandler(UserManager<User> userManager) : IRequestHandler<ResetPasswordCommand, EntityResult>
{
    public async Task<EntityResult> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return EntityResult.Failed(new EntityError(nameof(Messages.UserNotFoundByEmail), string.Format(Messages.UserNotFoundByEmail, request.Email)));
        }

        var isValid = await userManager.VerifyUserTokenAsync(user, "OtpProvider", "ResetPassword", request.OtpCode);
        if (!isValid)
        {
            return EntityResult.Failed(new EntityError(nameof(Messages.InvalidOrExpiredOtp), Messages.InvalidOrExpiredOtp));
        }

        var result = await userManager.ResetPasswordAsync(user, await userManager.GeneratePasswordResetTokenAsync(user), request.NewPassword);
        if (!result.Succeeded)
        {
            return EntityResult.Failed(result.Errors.Select(e => new EntityError("NewPassword", e.Description)).ToArray());
        }

        return EntityResult.Success;
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
