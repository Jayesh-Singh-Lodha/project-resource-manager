namespace PRM.Application.Interfaces;

public interface ILlmProvider
{
    string ProviderName { get; }
    Task<string> GenerateResponseAsync(string systemPrompt, string userPrompt);
}
