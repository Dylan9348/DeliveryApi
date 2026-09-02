using DeliveryApi.Models.DtoModels;

namespace DeliveryApi.Models;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public UserDto Client { get; set; } = new();
    public bool IsAtHome { get; set; } = false;
    public Address? ClientAddress { get; set; } = new();
    public Guid[] ProductsId { get; set; } = [];
    public bool HasDelivery { get; set; } = false;
    public UserDto Delivery { get; set; } = new();
    public string Code { get; set; } = "";
    public bool Finished { get; set; } = false;
}
