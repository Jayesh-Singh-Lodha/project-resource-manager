using System.Text;
using System.Text.Json;
using PRM.Application.Interfaces;

namespace PRM.Infrastructure.ExternalServices.Llm;

public class GeminiProvider : ILlmProvider
{
    private readonly HttpClient _httpClient;
    private readonly ISystemConfigService _configService;

    public GeminiProvider(HttpClient httpClient, ISystemConfigService configService)
    {
        _httpClient = httpClient;
        _configService = configService;
    }

    public string ProviderName => "Gemini";

    public async Task<string> GenerateResponseAsync(string systemPrompt, string userPrompt)
    {
        var config = await _configService.GetConfigByKeyAsync("GeminiApiKey");
        var apiKey = config?.Value;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new PRM.Core.Exceptions.DomainException("Gemini API key is not configured in SystemConfig. Please ask an Admin to configure it.", "MISSING_API_KEY");
        }

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";
        
        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = $"{systemPrompt}\n\n{userPrompt}" } } }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new PRM.Core.Exceptions.DomainException($"Gemini API error: {response.StatusCode} - {error}", "API_ERROR");
        }

        var responseString = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(responseString);
        var root = jsonDoc.RootElement;
        
        try
        {
            var text = root.GetProperty("candidates")[0]
                           .GetProperty("content")
                           .GetProperty("parts")[0]
                           .GetProperty("text").GetString();
                           
            return text ?? "No response generated.";
        }
        catch (Exception)
        {
            return "Failed to parse response from Gemini.";
        }
    }
}
