namespace Endure.Application.Common.Services;

public interface ICurrentUser
{
    Guid? AppUserId { get; }
}
