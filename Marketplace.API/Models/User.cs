using System;
using System.Collections.Generic;
using System.Linq;

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

    public IReadOnlyList<IRole> Roles => _roles;

    public User(Guid id, string name, string email, string phoneNumber, string passwordHash, DateTime registrationDateUtc)
    {
        ValidateIdentity(name, email, phoneNumber, passwordHash);
        Id = id; 
        Name = name;
        Email = email;
        PhoneNumber = phoneNumber?.Trim() ?? string.Empty;
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

    public void ChangeName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName) || newName.Length < 2)
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
        if (string.IsNullOrWhiteSpace(newPhoneNumber) || newPhoneNumber.Length < 7)
            throw new ArgumentException("Некоректний номер телефону");
        PhoneNumber = newPhoneNumber.Trim();
    }

    public void ChangePasswordHash(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Некоректний пароль");
        PasswordHash = newPasswordHash;
    }

    public static void ValidateIdentity(string name, string email, string phoneNumber, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length < 2)
            throw new ArgumentException("Некоректне ім'я");
        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            throw new ArgumentException("Некоректний email");
        if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length < 7)
            throw new ArgumentException("Некоректний номер телефону");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Некоректний пароль");
    }
}