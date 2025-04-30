/// <summary>
/// A generic interface for game event listeners.
/// </summary>
public interface IGameEventListener
{
    void OnEventRaised();
    void Register();
    void Deregister();
}