using Bogus;
using MYP.Domain.Entities;

namespace MYP.Application.Tests.Common.Fakers;

public sealed class ProjectFaker : Faker<Project>
{
    private Guid? _userId;

    public ProjectFaker()
    {
        RuleFor(p => p.Id, f => f.Random.Guid());
        RuleFor(p => p.Name, f => f.Commerce.ProductName());
        RuleFor(p => p.Description, f => f.Lorem.Sentence());
        RuleFor(p => p.Color, f => f.Internet.Color());
        RuleFor(p => p.UserId, f => _userId ?? f.Random.Guid());
        RuleFor(p => p.CreatedAt, f => f.Date.Past());
        RuleFor(p => p.UpdatedAt, f => null);
    }

    public ProjectFaker WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }
}
