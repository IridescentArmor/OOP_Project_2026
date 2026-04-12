using Marketplace.API.DTOs;
using Marketplace.API.Interfaces;
using Marketplace.API.Models;

namespace Marketplace.API.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public UserResponse Create(CreateUserRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (_userRepository.GetByEmail(email) != null)
            throw new InvalidOperationException("Користувач з таким email вже існує");

        var user = User.Create(
            request.Name.Trim(),
            email,
            request.PhoneNumber.Trim(),
            request.PasswordHash);

        if (request.IncludeCustomerRole)
            user.AddRole(new CustomerRole());

        if (request.IncludeSellerRole)
            user.AddRole(new SellerRole(request.SellerCompanyName!.Trim()));

        if (request.IncludeAdminRole)
            user.AddRole(new AdminRole(request.AdminAccessLevel));

        _userRepository.Add(user);
        return ToResponse(user);
    }

    public UserResponse? GetById(Guid id)
    {
        var user = _userRepository.GetById(id);
        return user == null ? null : ToResponse(user);
    }

    public IReadOnlyList<UserResponse> GetAll()
        => _userRepository.GetAll().Select(ToResponse).ToList();

    private static UserResponse ToResponse(User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        PhoneNumber = user.PhoneNumber,
        RoleNames = user.Roles.Select(r => r.RoleName).ToList()
    };
}
