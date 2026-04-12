using Marketplace.API.DTOs;

namespace Marketplace.API.Interfaces;

public interface IUserService
{
    UserResponse Create(CreateUserRequest request);
    UserResponse? GetById(Guid id);
    IReadOnlyList<UserResponse> GetAll();
}
