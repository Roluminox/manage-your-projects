using Bogus;
using MYP.Domain.Entities;

namespace MYP.Application.Tests.Common.Fakers;

public sealed class UserFaker : Faker<User>
{
    public UserFaker()
    {
        RuleFor(u => u.Id, f => f.Random.Guid());
        RuleFor(u => u.Email, f => f.Internet.Email());
        RuleFor(u => u.Username, f => f.Internet.UserName());
        RuleFor(u => u.PasswordHash, f => f.Random.Hash());
        RuleFor(u => u.DisplayName, f => f.Name.FullName());
        RuleFor(u => u.FirstName, f => f.Name.FirstName());
        RuleFor(u => u.LastName, f => f.Name.LastName());
        RuleFor(u => u.AvatarUrl, f => f.Internet.Avatar());
        RuleFor(u => u.IsActive, f => true);
        RuleFor(u => u.CreatedAt, f => f.Date.Past());
        RuleFor(u => u.UpdatedAt, f => null);
        RuleFor(u => u.LastLoginAt, f => null);
    }
}
