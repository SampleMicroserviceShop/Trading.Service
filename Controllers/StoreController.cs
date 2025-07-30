using Common.Library;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Trading.Service.Entities;

namespace Trading.Service.Controllers;

[ApiController]
[Route("store")]
[Authorize]
public class StoreController : ControllerBase
{
    private readonly IRepository<CatalogItem> catalogRepository;
    private readonly IRepository<ApplicationUser> usersRepository;
    private readonly IRepository<InventoryItem> inventoryRepository;
    public StoreController(
    IRepository<CatalogItem> catalogRepository,
    IRepository<ApplicationUser> userRepository,
    IRepository<InventoryItem> inventoryRepository)
    {
        this.catalogRepository = catalogRepository;
        this.usersRepository = userRepository;
        this.inventoryRepository = inventoryRepository;
    }
    [HttpGet]
    public async Task<ActionResult<StoreDto>> GetAsync()
    {
        string userId = User.FindFirstValue("sub");
        var catalogItems = await catalogRepository.GetAllAsync();
        var inventoryItems = await inventoryRepository.GetAllAsync(
        item => item.UserId == Guid.Parse(userId));
        var user = await usersRepository.GetAsync(Guid.Parse(userId));
        var storeDto = new StoreDto(
        catalogItems.Select(catalogItem =>
        new StoreItemDto
        (
        catalogItem.Id,
        catalogItem.Name,
        catalogItem.Description,
        catalogItem.Price,
        inventoryItems.FirstOrDefault(
        inventoryItem => inventoryItem.CatalogItemId == catalogItem.Id)?.Quantity ?? 0
        )),
        user?.Gil ?? 0);
        return Ok(storeDto);
    }
}
