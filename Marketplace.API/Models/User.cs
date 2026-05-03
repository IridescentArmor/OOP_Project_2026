using System;
using System.Collections.Generic;
using System.Linq;
using Marketplace.API.Validation;

namespace Marketplace.API.Models;

public class User : IUser
{
    private readonly List<IRole> _roles = new();

    private User()
    {
        Name = string.Empty;
        Email = string.Empty;
        PhoneNumber = string.Empty;
        PasswordHash = string.Empty;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PhoneNumber { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public DateTime RegistrationDate { get; private set; }

    public bool IsBlocked { get; private set; }
    public string? BlockReason { get; private set; }

    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockoutUntilUtc { get; private set; }

    public bool IsLockedOut(DateTime utcNow)
        => LockoutUntilUtc.HasValue && LockoutUntilUtc.Value > utcNow;

    public IReadOnlyList<IRole> Roles => _roles;

    public User(Guid id, string name, string email, string phoneNumber, string passwordHash, DateTime registrationDateUtc)
    {
        ValidateIdentity(name, email, phoneNumber, passwordHash);
        Id = id; 
        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        PhoneNumber = ValidationRules.NormalizePhoneNumber(phoneNumber);
        PasswordHash = passwordHash;
        RegistrationDate = registrationDateUtc;
    }

    public static User Create(string name, string email, string phoneNumber, string passwordHash)
        => new User(Guid.NewGuid(), name, email, phoneNumber, passwordHash, DateTime.UtcNow);

    public static User WithFixedId(Guid id, string name, string email, string phoneNumber, string passwordHash)
        => new User(id, name, email, phoneNumber, passwordHash, DateTime.UtcNow);

    public void AddRole(IRole role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role), "Роль не може бути порожньою");
        _roles.Add(role);
    }

    public bool HasRole<TRole>() where TRole : class, IRole
        => _roles.OfType<TRole>().Any();

    public TRole? GetRole<TRole>() where TRole : class, IRole
        => _roles.OfType<TRole>().FirstOrDefault();

    public bool RemoveRole<TRole>() where TRole : class, IRole
    {
        var before = _roles.Count;
        _roles.RemoveAll(r => r is TRole);
        return _roles.Count < before;
    }

    public void ChangeName(string newName)
    {
        if (!ValidationRules.IsValidPersonName(newName))
            throw new ArgumentException("Некоректне ім'я");
        Name = newName.Trim();
    }

    public void ChangeEmail(string newEmail)
    {
        if (string.IsNullOrWhiteSpace(newEmail) || !newEmail.Contains("@"))
            throw new ArgumentException("Некоректний email");
        Email = newEmail.Trim();
    }

    public void ChangePhoneNumber(string newPhoneNumber)
    {
        if (!ValidationRules.IsValidPhoneNumber(newPhoneNumber))
            throw new ArgumentException("Некоректний номер телефону");
        PhoneNumber = ValidationRules.NormalizePhoneNumber(newPhoneNumber);
    }

    public void ChangePasswordHash(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Некоректний пароль");
        PasswordHash = newPasswordHash;
    }

    public void Block(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Причина блокування обов'язкова");

        IsBlocked = true;
        BlockReason = reason.Trim();
    }

    public void Unblock()
    {
        IsBlocked = false;
        BlockReason = null;
    }

    public void RegisterFailedLoginAttempt(DateTime utcNow, int maxAttempts, TimeSpan lockoutDuration)
    {
        if (maxAttempts <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxAttempts));

        if (lockoutDuration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(lockoutDuration));

        FailedLoginAttempts++;
        if (FailedLoginAttempts >= maxAttempts)
        {
            LockoutUntilUtc = utcNow.Add(lockoutDuration);
            FailedLoginAttempts = 0;
        }
    }

    public void RegisterSuccessfulLogin()
    {
        FailedLoginAttempts = 0;
        LockoutUntilUtc = null;
    }

    public static void ValidateIdentity(string name, string email, string phoneNumber, string passwordHash)
    {
        if (!ValidationRules.IsValidPersonName(name))
            throw new ArgumentException("Некоректне ім'я");
        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            throw new ArgumentException("Некоректний email");
        if (!ValidationRules.IsValidPhoneNumber(phoneNumber))
            throw new ArgumentException("Некоректний номер телефону");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Некоректний пароль");
    }
}