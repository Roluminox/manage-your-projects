using Bogus;
using MYP.Domain.Entities;

namespace MYP.Application.Tests.Common.Fakers;

public sealed class SnippetFaker : Faker<Snippet>
{
    private static readonly string[] Languages = { "csharp", "typescript", "javascript", "python", "sql" };

    public SnippetFaker()
    {
        RuleFor(s => s.Id, f => f.Random.Guid());
        RuleFor(s => s.Title, f => f.Lorem.Sentence(3));
        RuleFor(s => s.Code, f => f.Lorem.Paragraph());
        RuleFor(s => s.Language, f => f.PickRandom(Languages));
        RuleFor(s => s.Description, f => f.Lorem.Sentence(10));
        RuleFor(s => s.IsFavorite, f => false);
        RuleFor(s => s.UserId, f => f.Random.Guid());
        RuleFor(s => s.CreatedAt, f => f.Date.Past());
        RuleFor(s => s.UpdatedAt, f => null);
    }

    public SnippetFaker WithUserId(Guid userId)
    {
        RuleFor(s => s.UserId, userId);
        return this;
    }
}
