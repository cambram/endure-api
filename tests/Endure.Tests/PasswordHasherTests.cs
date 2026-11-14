using Endure.Infrastructure.Authentication;

namespace Endure.Tests;

public sealed class PasswordHasherTests
{
    private readonly PasswordHasher _passwordHasher = new();

    [Fact]
    public void VerifyAcceptsCorrectPasswordAndRejectsWrongPassword()
    {
        var storedHash = _passwordHasher.Hash("correct horse battery");

        Assert.True(_passwordHasher.Verify("correct horse battery", storedHash));
        Assert.False(_passwordHasher.Verify("correct horse batterz", storedHash));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-hash")]
    [InlineData("v1.600000.not-base64.not-base64")]
    [InlineData("600000.c2FsdA==.aGFzaA==")]
    public void VerifyReturnsFalseForMissingOrMalformedHash(string? storedHash)
    {
        Assert.False(_passwordHasher.Verify("any password", storedHash));
    }
}
