namespace DeliveryApi.Models;

public class Address
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Street { get; set; } = "";
}
