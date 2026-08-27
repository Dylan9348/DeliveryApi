
namespace DeliveryApi.Models;

public class SuspendAccountRequest
{
    public string Username { get; set; } = "";
    public int Days { get; set; }
    public int Hours { get; set; }
}
