using FluentValidation;
using IdentityService.Application.DTOs;
using IdentityService.Domain.Repositories;
using MediatR;
using BCrypt.Net;

namespace IdentityService.Application.Commands;

public record RegisterCommand(RegisterRequest Request) : IRequest<AuthResponse>;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Request.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Request.Password).MinimumLength(6);
        RuleFor(x => x.Request.FullName).NotEmpty();
    }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;

    public RegisterCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _userRepository.ExistsByEmailAsync(request.Request.Email, cancellationToken))
        {
            throw new Exception("Email already exists");
        }

        var user = Domain.Entities.User.Create(
            request.Request.Email,
            BCrypt.Net.BCrypt.HashPassword(request.Request.Password),
            request.Request.FullName,
            request.Request.PhoneNumber
        );

        await _userRepository.AddAsync(user, cancellationToken);

        return new AuthResponse(string.Empty, user.Email, user.FullName, new List<string> { "User" });
    }
}