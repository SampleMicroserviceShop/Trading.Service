using Automatonymous;
using Common.Library;
using GreenPipes;
using Trading.Service.Entities;
using Trading.Service.Exceptions;
using Trading.Service.StateMachines;

namespace Trading.Service.Activities;

public class CalculatePurchaseTotalActivity : Activity<PurchaseState, PurchaseRequested>
{
    private readonly IRepository<CatalogItem> repository;
    public CalculatePurchaseTotalActivity(IRepository<CatalogItem> repository)
    {
        this.repository = repository;
    }
    public void Probe(ProbeContext context)
    {
        // Provides information that could be used during visualization of the activity
        context.CreateScope("calculate-purchase-total");
    }
    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }
    public async Task Execute(
    BehaviorContext<PurchaseState, PurchaseRequested> context,
    Behavior<PurchaseState, PurchaseRequested> next)
    {
        var message = context.Data;
        var item = await repository.GetAsync(message.ItemId);
        if (item == null)
        {
            throw new UnknownItemException(message.ItemId);
        }
        context.Instance.PurchaseTotal = item.Price * message.Quantity;
        context.Instance.LastUpdated = DateTimeOffset.UtcNow;
        await next.Execute(context).ConfigureAwait(false);
    }
    public Task Faulted<TException>(
    BehaviorExceptionContext<PurchaseState, PurchaseRequested, TException> context,
    Behavior<PurchaseState, PurchaseRequested> next) where TException : System.Exception
    {
        return next.Faulted(context);
    }
}
