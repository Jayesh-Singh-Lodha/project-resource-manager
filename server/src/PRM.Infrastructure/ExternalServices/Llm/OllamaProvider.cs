using System.Text;
using System.Text.Json;
using PRM.Application.Interfaces;

namespace PRM.Infrastructure.ExternalServices.Llm;

public class OllamaProvider : ILlmProvider
{
    private readonly HttpClient _httpClient;
    private readonly ISystemConfigService _configService;

    public OllamaProvider(HttpClient httpClient, ISystemConfigService configService)
    {
        _httpClient = httpClient;
        _configService = configService;
    }

    public string ProviderName => "Ollama";

    public async Task<string> GenerateResponseAsync(string systemPrompt, string userPrompt)
    {
        var config = await _configService.GetConfigByKeyAsync("OllamaApiKey");
        var apiKey = config?.Value;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new PRM.Core.Exceptions.DomainException("Ollama API key is not configured in SystemConfig.", "MISSING_API_KEY");
        }

        var url = "http://164.52.211.238/api/generate";
        
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
        requestMessage.Headers.Add("apikey", apiKey);

        var requestBody = new
        {
            model = "gemma3:12b-it-q8_0",
            prompt = $"{systemPrompt}\n\n{userPrompt}",
            stream = false
        };

        requestMessage.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(requestMessage);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new PRM.Core.Exceptions.DomainException($"Ollama API error: {response.StatusCode} - {error}", "API_ERROR");
        }

        var responseString = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(responseString);
        var root = jsonDoc.RootElement;
        
        try
        {
            var text = root.GetProperty("response").GetString();
            return text ?? "No response generated.";
        }
        catch (Exception)
        {
            return "Failed to parse response from Ollama.";
        }
    }
}
