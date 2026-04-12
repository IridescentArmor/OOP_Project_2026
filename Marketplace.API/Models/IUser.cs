using System;
using System.Collections.Generic;

namespace Marketplace.API.Models
{
    public interface IUser : IEntity
    {
        string Name { get; }
        string Email { get; }
        string PhoneNumber { get; }
        string PasswordHash { get; }
        DateTime RegistrationDate { get; }
        IReadOnlyList<IRole> Roles { get; }
        void AddRole(IRole role);
        bool HasRole<TRole>() where TRole : class, IRole;
        void ChangeName(string newName);
        void ChangePhoneNumber(string newPhoneNumber);
        void ChangePasswordHash(string newPasswordHash);
    }
}