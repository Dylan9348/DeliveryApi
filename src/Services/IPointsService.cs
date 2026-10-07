
namespace DeliveryApi.Services;

public interface IPointsService
{
    Task AddPointsBalanceAsync(int points, Guid userId);

    Task ResetPointsAsync(Guid userId);

    Task DiscountPointsAsync(int points, Guid userid);
}