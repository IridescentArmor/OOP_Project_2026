using Marketplace.API.DTOs;

namespace Marketplace.API.Interfaces;

public interface IUserService
{
    UserResponse Create(CreateUserRequest request);
    UserResponse? GetById(Guid id);
    IReadOnlyList<UserResponse> GetAll();
    UserResponse UpdateByAdmin(Guid userId, AdminUpdateUserRequest request);
    void DeleteByAdmin(Guid requesterAdminUserId, int requesterAdminAccessLevel, Guid targetUserId);
    UserResponse GrantAdminRoleBySuperAdmin(Guid requesterAdminUserId, Guid targetUserId, int accessLevel);
    UserResponse RevokeAdminRoleBySuperAdmin(Guid requesterAdminUserId, Guid targetUserId);
    UserResponse UpdateMyProfile(Guid userId, UpdateMyProfileRequest request);
    void ChangeMyPassword(Guid userId, ChangePasswordRequest request);
    void DeleteMyAccount(Guid userId);
}
