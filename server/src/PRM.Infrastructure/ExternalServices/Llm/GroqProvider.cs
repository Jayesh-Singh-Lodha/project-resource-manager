using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PRM.Application.Interfaces;

namespace PRM.Infrastructure.ExternalServices.Llm;

public class GroqProvider : ILlmProvider
{
    private readonly HttpClient _httpClient;
    private readonly ISystemConfigService _configService;

    public GroqProvider(HttpClient httpClient, ISystemConfigService configService)
    {
        _httpClient = httpClient;
        _configService = configService;
    }

    public string ProviderName => "Groq";

    public async Task<string> GenerateResponseAsync(string systemPrompt, string userPrompt)
    {
        var config = await _configService.GetConfigByKeyAsync("GroqApiKey");
        var apiKey = config?.Value;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Groq API key is not configured in SystemConfig.");
        }

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var requestBody = new
        {
            model = "llama-3.1-8b-instant",
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            }
        };

        requestMessage.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(requestMessage);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Groq API error: {response.StatusCode} - {error}");
        }

        var responseString = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(responseString);
        var root = jsonDoc.RootElement;
        
        try
        {
            var text = root.GetProperty("choices")[0]
                           .GetProperty("message")
                           .GetProperty("content").GetString();
                           
            return text ?? "No response generated.";
        }
        catch (Exception)
        {
            return "Failed to parse response from Groq.";
        }
    }
}
