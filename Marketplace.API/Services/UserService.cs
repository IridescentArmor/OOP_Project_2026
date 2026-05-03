using Marketplace.API.DTOs;
using Marketplace.API.Interfaces;
using Marketplace.API.Models;
using Microsoft.AspNetCore.Identity;

namespace Marketplace.API.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserService(
        IUserRepository userRepository,
        IOrderRepository orderRepository,
        IPasswordHasher<User> passwordHasher)
    {
        _userRepository = userRepository;
        _orderRepository = orderRepository;
        _passwordHasher = passwordHasher;
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

    public UserResponse UpdateByAdmin(Guid userId, AdminUpdateUserRequest request)
    {
        var user = _userRepository.GetById(userId)
            ?? throw new InvalidOperationException("Користувача не знайдено");

        var email = request.Email.Trim().ToLowerInvariant();
        var existing = _userRepository.GetByEmail(email);
        if (existing != null && existing.Id != userId)
            throw new InvalidOperationException("Користувач з таким email вже існує");

        user.ChangeName(request.Name.Trim());
        user.ChangeEmail(email);
        user.ChangePhoneNumber(request.PhoneNumber.Trim());
        _userRepository.Update(user);
        return ToResponse(user);
    }

    public UserResponse UpdateMyProfile(Guid userId, UpdateMyProfileRequest request)
    {
        var user = _userRepository.GetById(userId)
            ?? throw new InvalidOperationException("Користувача не знайдено");

        var email = request.Email.Trim().ToLowerInvariant();
        var existing = _userRepository.GetByEmail(email);
        if (existing != null && existing.Id != userId)
            throw new InvalidOperationException("Користувач з таким email вже існує");

        user.ChangeName(request.Name.Trim());
        user.ChangeEmail(email);
        user.ChangePhoneNumber(request.PhoneNumber.Trim());
        _userRepository.Update(user);
        return ToResponse(user);
    }

    public void DeleteByAdmin(Guid requesterAdminUserId, int requesterAdminAccessLevel, Guid targetUserId)
    {
        var target = _userRepository.GetById(targetUserId)
            ?? throw new InvalidOperationException("Користувача не знайдено");

        if (requesterAdminUserId == targetUserId)
            throw new InvalidOperationException("Адміністратор не може видалити власний акаунт");

        if (IsSuperAdmin(target) && requesterAdminUserId != targetUserId)
            throw new InvalidOperationException("Змінювати супер-адміністратора може лише він сам");

        if (target.Roles.OfType<AdminRole>().Any() && requesterAdminAccessLevel < 100)
            throw new InvalidOperationException("Недостатньо прав для видалення іншого адміністратора");

        if (target.Roles.OfType<SellerRole>().Any() && _orderRepository.HasActiveOrdersForSeller(targetUserId))
            throw new InvalidOperationException("Продавець має незавершені замовлення. Видалення неможливе");

        _userRepository.Delete(target);
    }

    public UserResponse GrantAdminRoleBySuperAdmin(Guid requesterAdminUserId, Guid targetUserId, int accessLevel)
    {
        var requester = _userRepository.GetById(requesterAdminUserId)
            ?? throw new InvalidOperationException("Адміністратора-ініціатора не знайдено");

        var requesterAdminRole = requester.GetRole<AdminRole>()
            ?? throw new InvalidOperationException("Користувач не має ролі адміністратора");

        if (requesterAdminRole.AccessLevel < 100)
            throw new InvalidOperationException("Лише супер-адміністратор може видавати адмін-права");

        var target = _userRepository.GetById(targetUserId)
            ?? throw new InvalidOperationException("Користувача не знайдено");

        if (IsSuperAdmin(target) && requesterAdminUserId != targetUserId)
            throw new InvalidOperationException("Змінювати супер-адміністратора може лише він сам");

        var targetAdminRole = target.GetRole<AdminRole>();
        if (targetAdminRole == null)
            target.AddRole(new AdminRole(accessLevel));
        else
            targetAdminRole.UpdateAccessLevel(accessLevel);

        _userRepository.UpdateStoredRoles(target);
        return ToResponse(target);
    }

    public UserResponse RevokeAdminRoleBySuperAdmin(Guid requesterAdminUserId, Guid targetUserId)
    {
        var requester = _userRepository.GetById(requesterAdminUserId)
            ?? throw new InvalidOperationException("Адміністратора-ініціатора не знайдено");

        var requesterAdminRole = requester.GetRole<AdminRole>()
            ?? throw new InvalidOperationException("Користувач не має ролі адміністратора");

        if (requesterAdminRole.AccessLevel < 100)
            throw new InvalidOperationException("Лише супер-адміністратор може забирати адмін-права");

        var target = _userRepository.GetById(targetUserId)
            ?? throw new InvalidOperationException("Користувача не знайдено");

        if (targetUserId == requesterAdminUserId)
            throw new InvalidOperationException("Супер-адміністратор не може забрати адмін-права у себе");

        if (IsSuperAdmin(target))
            throw new InvalidOperationException("Змінювати супер-адміністратора може лише він сам");

        if (!target.RemoveRole<AdminRole>())
            throw new InvalidOperationException("Користувач не має ролі адміністратора");

        _userRepository.UpdateStoredRoles(target);
        return ToResponse(target);
    }

    public void ChangeMyPassword(Guid userId, ChangePasswordRequest request)
    {
        var user = _userRepository.GetById(userId)
            ?? throw new InvalidOperationException("Користувача не знайдено");

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
        if (verification == PasswordVerificationResult.Failed)
            throw new InvalidOperationException("Поточний пароль невірний");

        if (request.CurrentPassword == request.NewPassword)
            throw new InvalidOperationException("Новий пароль має відрізнятися від поточного");

        var newHash = _passwordHasher.HashPassword(user, request.NewPassword);
        user.ChangePasswordHash(newHash);
        _userRepository.Update(user);
    }

    public void DeleteMyAccount(Guid userId)
    {
        var user = _userRepository.GetById(userId)
            ?? throw new InvalidOperationException("Користувача не знайдено");

        if (user.Roles.OfType<SellerRole>().Any() && _orderRepository.HasActiveOrdersForSeller(userId))
            throw new InvalidOperationException("Не можна видалити акаунт продавця з активними замовленнями");

        _userRepository.Delete(user);
    }

    private static UserResponse ToResponse(User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        PhoneNumber = user.PhoneNumber,
        IsBlocked = user.IsBlocked,
        BlockReason = user.BlockReason,
        IsSellerApproved = user.Roles.OfType<SellerRole>().FirstOrDefault()?.IsApproved,
        SellerCompanyName = user.Roles.OfType<SellerRole>().FirstOrDefault()?.CompanyName,
        AdminAccessLevel = user.Roles.OfType<AdminRole>().FirstOrDefault()?.AccessLevel,
        RegistrationDate = user.RegistrationDate,
        RoleNames = user.Roles.Select(r => r.RoleName).ToList()
    };

    private static bool IsSuperAdmin(User user)
        => user.GetRole<AdminRole>()?.AccessLevel >= 100;
}
