using Common.Library;

namespace Trading.Service.Entities;

public class InventoryItem : IEntity
{
    public Guid UserId { get; set; }
    public Guid CatalogItemId { get; set; }
    public int Quantity { get; set; }
}
