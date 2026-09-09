namespace DeliveryApi.Models.DtoModels;

public class OrderDto
{
    public Guid Id { get; set; }
    public Address? Address { get; set; } = new();
    public UserDto? Client { get; set; } = new();
}
