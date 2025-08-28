using Automatonymous;
using Common.Library;
using MassTransit;
using Trading.Service.Entities;
using Trading.Service.Exceptions;
using Trading.Service.StateMachines;

namespace Trading.Service.Activities;

public class CalculatePurchaseTotalActivity : IStateMachineActivity<PurchaseState, PurchaseRequested>
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
    IBehavior<PurchaseState, PurchaseRequested> next)
    {
        var message = context.Message;
        var item = await repository.GetAsync(message.ItemId);
        if (item == null)
        {
            throw new UnknownItemException(message.ItemId);
        }
        context.Saga.PurchaseTotal = item.Price * message.Quantity;
        context.Saga.LastUpdated = DateTimeOffset.UtcNow;
        await next.Execute(context).ConfigureAwait(false);
    }
    public Task Faulted<TException>(
    BehaviorExceptionContext<PurchaseState, PurchaseRequested, TException> context,
    IBehavior<PurchaseState, PurchaseRequested> next) where TException : System.Exception
    {
        return next.Faulted(context);
    }

}
