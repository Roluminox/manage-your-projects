using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using NSubstitute;

namespace MYP.Application.Tests.Common;

public class TestFixture : IDisposable
{
    public IApplicationDbContext DbContext { get; }

    public TestFixture()
    {
        DbContext = Substitute.For<IApplicationDbContext>();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
