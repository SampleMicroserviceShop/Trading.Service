using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Trading.Service.StateMachines;

namespace Trading.Service.Controllers;
[Route("[controller]")]
[ApiController]
[Authorize]
public class PurchaseController : ControllerBase
{
    readonly IPublishEndpoint publishEndpoint;
    private readonly IRequestClient<GetPurchaseState> purchaseClient;
    public PurchaseController(IPublishEndpoint publishEndpoint, IRequestClient<GetPurchaseState> purchaseClient)
    {
        this.publishEndpoint = publishEndpoint;
        this.purchaseClient = purchaseClient;
    }
    [HttpGet("status/{idempotencyId}")]
    public async Task<ActionResult<PurchaseDto>> GetStatusAsync(Guid idempotencyId)
    {
        var response = await purchaseClient.GetResponse<PurchaseState>(
        new GetPurchaseState(idempotencyId));
        var purchaseState = response.Message;
        var purchase = new PurchaseDto(
        purchaseState.UserId,
        purchaseState.ItemId,
        purchaseState.PurchaseTotal,
        purchaseState.Quantity,
        purchaseState.CurrentState,
        purchaseState.ErrorMessage,
        purchaseState.Received,
        purchaseState.LastUpdated);
        return Ok(purchase);
    }
    [HttpPost]
    public async Task<IActionResult> PostAsync(SubmitPurchaseDto purchase)
    {
        var userId = User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
        var message = new PurchaseRequested(
        Guid.Parse(userId),
        purchase.ItemId.Value,
        purchase.Quantity,
        purchase.IdempotencyId.Value);
        await publishEndpoint.Publish(message);
        //await Task.Delay(TimeSpan.FromSeconds(5));
        return AcceptedAtAction(nameof(GetStatusAsync), new { purchase.IdempotencyId }, new
        {
            purchase.IdempotencyId
        });
    }
}

