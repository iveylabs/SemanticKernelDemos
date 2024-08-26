namespace SemanticKernelDemos.Contracts.Services;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}
