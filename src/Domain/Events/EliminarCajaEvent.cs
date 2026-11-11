namespace BaseApp.Domain.Events;

public class EliminarCajaEvent : BaseEvent
{
    public EliminarCajaEvent(int cobradorId)
    {
        CobradorId = cobradorId;
    }

    public int CobradorId { get; }
}
