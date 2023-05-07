namespace EvenBus.Messages
{
    public interface IIntegrationEvent
    {
        DateTime CreatetionDate { get; }
        Guid Id { get; set; }
    }
}
