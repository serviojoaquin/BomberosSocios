namespace BaseApp.Domain.Events;

public class CrearCajaEvent : BaseEvent
{
    public CrearCajaEvent(string name, string externalId)
    {
        Name = name;
        ExternalId = externalId;
    }

    public string Name { get; }

    public string ExternalId { get; }
}
