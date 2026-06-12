using PRM.Console.Helpers;
using PRM.Console.Services;
using System.Text.Json;

namespace PRM.Console.Screens.Admin;

/// <summary>
/// System Config (BRD Screen 3.3).
/// Allows the Admin to manage specific system settings instead of generic key/value pairs.
/// </summary>
public class SystemConfigScreen
{
    private readonly ApiClient _apiClient;

    public SystemConfigScreen(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.DrawBox("SYSTEM CONFIG");

            try
            {
                var configs = await _apiClient.GetAsync<List<JsonElement>>("api/config");

                System.Console.WriteLine("Current Settings:");
                
                var configKeys = new List<string>();
                int index = 1;

                if (configs != null)
                {
                    foreach (var c in configs)
                    {
                        if (c.ValueKind != JsonValueKind.Object) continue;
                        
                        string key = c.TryGetProperty("key", out var k) || c.TryGetProperty("Key", out k) ? k.GetString() : string.Empty;
                        
                        if (string.IsNullOrEmpty(key)) continue;

                        string val = "Not Set";
                        if (c.TryGetProperty("value", out var v) || c.TryGetProperty("Value", out v))
                        {
                            if (v.ValueKind != JsonValueKind.Null && v.ValueKind != JsonValueKind.Undefined)
                            {
                                val = v.GetString() ?? "Not Set";
                            }
                        }

                        System.Console.WriteLine($" [{index}] {key,-25} : {val}");
                        configKeys.Add(key);
                        index++;
                    }
                }

                System.Console.WriteLine();
                System.Console.WriteLine(" [B] Back");
                System.Console.WriteLine();

                var option = ConsoleHelper.Prompt(configKeys.Count > 0 ? $"Select setting to update (1-{configKeys.Count}) or B" : "Press B to go back").ToUpper();

                if (option == "B") return;

                if (!int.TryParse(option, out int selectedIndex) || selectedIndex < 1 || selectedIndex > configKeys.Count)
                {
                    ConsoleHelper.WriteError("Invalid option.");
                    ConsoleHelper.WaitForKey();
                    continue;
                }

                string keyToUpdate = configKeys[selectedIndex - 1];

                System.Console.WriteLine();
                var newValue = ConsoleHelper.Prompt($"Enter new value for {keyToUpdate}");

                if (string.IsNullOrWhiteSpace(newValue))
                {
                    ConsoleHelper.WriteWarning("Value cannot be empty.");
                    ConsoleHelper.WaitForKey();
                    continue;
                }

                await _apiClient.PutAsync($"api/config/{keyToUpdate}", new { value = newValue });
                ConsoleHelper.WriteSuccess("Configuration updated successfully.");
                ConsoleHelper.WaitForKey();
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Failed to load or update config: {ex.Message}");
                ConsoleHelper.WaitForKey();
                return;
            }
        }
    }
}
