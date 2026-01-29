using Bogus;
using MYP.Domain.Entities;

namespace MYP.Application.Tests.Common.Fakers;

public sealed class TagFaker : Faker<Tag>
{
    public TagFaker()
    {
        RuleFor(t => t.Id, f => f.Random.Guid());
        RuleFor(t => t.Name, f => f.Lorem.Word());
        RuleFor(t => t.Color, f => f.Internet.Color());
        RuleFor(t => t.UserId, f => f.Random.Guid());
        RuleFor(t => t.CreatedAt, f => f.Date.Past());
        RuleFor(t => t.UpdatedAt, f => null);
    }

    public TagFaker WithUserId(Guid userId)
    {
        RuleFor(t => t.UserId, userId);
        return this;
    }
}
