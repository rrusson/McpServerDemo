using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.Client;
using OpenAI;
using System.ClientModel;

namespace ConsoleMcpClient
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddUserSecrets<Program>()
                .AddEnvironmentVariables()
                .Build();

            string apiKey = GetRequiredSetting(configuration, "ApiKey");
            string llmEndpoint = GetRequiredSetting(configuration, "LlmEndpoint");
            string mcpEndpoint = GetRequiredSetting(configuration, "McpEndpoint");
            string modelId = GetRequiredSetting(configuration, "ModelId");
            string deploymentName = GetRequiredSetting(configuration, "DeploymentName");
            bool useLocalModel = bool.TryParse(configuration["UseLocalModel"], out bool parsedUseLocalModel) && parsedUseLocalModel;

            Console.WriteLine("Press any key to begin...");
            Console.ReadKey();

            var mcpUri = new Uri(mcpEndpoint);

            var options = new HttpClientTransportOptions
            {
                Endpoint = mcpUri,
                TransportMode = HttpTransportMode.AutoDetect,
                ConnectionTimeout = TimeSpan.FromMinutes(10)
            };

            IClientTransport transport = new HttpClientTransport(options);

            // Create an MCPClient for the local McpServer1 (HTTP transport)
            var mcpClient = await McpClient.CreateAsync(transport);

            // Retrieve the list of tools available on McpServer1
            Console.WriteLine("\r\nAvailable MCP tools:");
            var tools = await mcpClient.ListToolsAsync().ConfigureAwait(false);
            foreach (var tool in tools)
            {
                Console.WriteLine($"{tool.Name}: {tool.Description}");
            }

            // Prepare and build kernel with the MCP tools as Kernel functions
            var builder = Kernel.CreateBuilder();

            builder.Services.AddLogging(c => c.AddDebug().SetMinimumLevel(LogLevel.Trace));

            if (useLocalModel)
            {
                var llmClient = new OpenAIClient(
                    new ApiKeyCredential(apiKey),
                    new OpenAIClientOptions { Endpoint = new Uri(llmEndpoint), NetworkTimeout = TimeSpan.FromMinutes(10) });

                builder.Services.AddOpenAIChatCompletion(
                    modelId: modelId,
                    openAIClient: llmClient);
            }
            else
            {
                builder.Services.AddAzureOpenAIChatCompletion(
                    deploymentName: deploymentName,
                    endpoint: llmEndpoint,
                    apiKey: apiKey);
            }

            Kernel kernel = builder.Build();
            kernel.Plugins.AddFromFunctions("RyansMcpServer", tools.Select(aiFunction => aiFunction.AsKernelFunction()));            

#pragma warning disable SKEXP0001 // RetainArgumentTypes is experimental
            OpenAIPromptExecutionSettings executionSettings = new()
            {
                // For dumber models a lot of coaching helps.
                ChatSystemPrompt = "You are a helpful AI assistant. You have access to a set of tools. Use these tools when you need to. Do not show the user the tool calls. Instead, call the tools and use their output to answer the user's question.",
                Temperature = 1,
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new() { RetainArgumentTypes = true })
            };
#pragma warning disable SKEXP0001 // RetainArgumentTypes is experimental

            // Test using MCP tools
            string prompt = "Give me the current date and time in English and Spanish.";
            FunctionResult result = await kernel.InvokePromptAsync(prompt, new(executionSettings)).ConfigureAwait(false);
            Console.WriteLine($"\r\nPROMPT: {prompt}\r\n\r\n{result}\r\n");

            //string prompt2 = "Give me airport codes for Florida."; 
            //FunctionResult result2 = await kernel.InvokePromptAsync(prompt2, new(executionSettings)).ConfigureAwait(false);
            //Console.WriteLine($"\r\nPROMPT: {prompt2}\r\n\r\n{result2}");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static string GetRequiredSetting(IConfiguration configuration, string key)
        {
            string? value = configuration[key];
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"Missing required configuration value '{key}'.");
            }

            return value;
        }
    }
}
