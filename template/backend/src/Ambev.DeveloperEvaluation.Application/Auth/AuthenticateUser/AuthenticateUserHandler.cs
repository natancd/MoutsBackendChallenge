using System.Threading;
using System.Threading.Tasks;
using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Specifications;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Auth.AuthenticateUser
{
    public class AuthenticateUserHandler : IRequestHandler<AuthenticateUserCommand, AuthenticateUserResult>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthenticateUserHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<AuthenticateUserResult> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            
            if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.Password))
            {
                throw new UnauthorizedAccessException(
                    "Authentication failed. Check your email and password and try again.");
            }

            var activeUserSpec = new ActiveUserSpecification();
            if (!activeUserSpec.IsSatisfiedBy(user))
            {
                throw new UnauthorizedAccessException(
                    "Authentication failed. The account is inactive or suspended.");
            }

            var token = _jwtTokenGenerator.GenerateToken(user);

            return new AuthenticateUserResult
            {
                Token = token,
                Id = user.Id,
                Email = user.Email,
                Name = user.Username,
                Phone = user.Phone,
                Role = user.Role.ToString()
            };
        }
    }
}
