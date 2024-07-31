namespace Center_windows_winui.Contracts.Services;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}
