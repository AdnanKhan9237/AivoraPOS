namespace AivoraPOS.Core.Interfaces.Security;

public interface ISessionTimeoutService
{
    TimeSpan IdleTimeout { get; set; }
    void RecordActivity();
    event EventHandler? SessionTimedOut;
    void Start();
    void Stop();
}
