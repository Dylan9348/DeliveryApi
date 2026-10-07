
using DeliveryApi.DataBase;

namespace DeliveryApi.Services;

public class PointsService(Context database) : IPointsService
{
    private readonly Context _database = database;

    public async Task AddPointsBalanceAsync(int points, Guid userId)
    {
        var user = await _database.Users.FindAsync(userId);

        if (user is null)
            return;
        
        user.Points += points;

        await _database.SaveChangesAsync();
    }

    public async Task ResetPointsAsync(Guid userId)
    {
        var user = await _database.Users.FindAsync(userId);

        if (user is null)
            return;

        user.Points = 0;

        await _database.SaveChangesAsync();
    }

    public async Task DiscountPointsAsync(int points, Guid userId)
    {
        var user = await _database.Users.FindAsync(userId);

        if (user is null)
            return;
        
        user.Points -= points;

        await _database.SaveChangesAsync();
    }
}
