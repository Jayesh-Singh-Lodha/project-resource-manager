using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PRM.Console.Helpers;
using PRM.Console.Screens;
using PRM.Console.Screens.Admin;
using PRM.Console.Screens.Employee;
using PRM.Console.Screens.Manager;
using PRM.Console.Services;

try
{
    // Build Configuration
    var config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

    var apiBaseUrl = config["ApiBaseUrl"] ?? "https://localhost:5001";

    // Setup DI
    var services = new ServiceCollection();

    services.AddHttpClient<ApiClient>(client =>
    {
        client.BaseAddress = new Uri(apiBaseUrl);
    });

    services.AddTransient<LoginScreen>();
    services.AddTransient<ChangePasswordScreen>();

    var serviceProvider = services.BuildServiceProvider();
    var apiClient = serviceProvider.GetRequiredService<ApiClient>();
    var loginScreen = new LoginScreen(apiClient);

    while (true)
    {
        ConsoleHelper.ClearScreen();
        ConsoleHelper.DrawBox(
            "PROJECT RESOURCE MANAGER (PRM)",
            "Welcome! Please select an option.");

        System.Console.WriteLine(" [1] Login");
        System.Console.WriteLine(" [2] Exit");
        System.Console.WriteLine();

        var option = ConsoleHelper.Prompt("Enter option");

        if (option == "1")
        {
            ConsoleHelper.ClearScreen();
            var authResult = await loginScreen.ShowAsync();

            if (authResult is not null)
            {
                var (role, fullName) = authResult.Value;

                if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    var adminMenu = new AdminMenuScreen(apiClient, fullName);
                    await adminMenu.ShowAsync();
                }
                else if (role.Equals("Manager", StringComparison.OrdinalIgnoreCase))
                {
                    var managerMenu = new ManagerMenuScreen(apiClient, fullName);
                    await managerMenu.ShowAsync();
                }
                else if (role.Equals("Employee", StringComparison.OrdinalIgnoreCase))
                {
                    var employeeMenu = new EmployeeMenuScreen(apiClient, fullName);
                    await employeeMenu.ShowAsync();
                }
                else
                {
                    ConsoleHelper.WriteError($"Unknown user role: '{role}'");
                    ConsoleHelper.WaitForKey();
                }
            }
        }
        else if (option == "2")
        {
            ConsoleHelper.ClearScreen();
            System.Console.WriteLine("Goodbye!");
            break;
        }
        else
        {
            ConsoleHelper.WriteError("Invalid option. Please try again.");
            ConsoleHelper.WaitForKey();
        }
    }
}
catch (Exception ex)
{
    System.Console.ForegroundColor = ConsoleColor.Red;
    System.Console.WriteLine($"Critical error starting application: {ex.Message}");
    System.Console.ResetColor();
    System.Console.ReadLine();
}
