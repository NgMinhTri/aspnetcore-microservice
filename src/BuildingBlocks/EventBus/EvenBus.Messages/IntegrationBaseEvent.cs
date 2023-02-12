namespace EvenBus.Messages
{
    public record IntegrationBaseEvent : IIntegrationEvent
    {
        public DateTime CreatetionDate { get; } = DateTime.UtcNow;
        public Guid Id { get; set; }
    }
}
