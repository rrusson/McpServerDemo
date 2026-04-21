// Copyright (c) Microsoft. All rights reserved.
using System.ClientModel;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.OpenAI;

using ModelContextProtocol.Client;

using OpenAI;

namespace ConsoleApp1
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .AddEnvironmentVariables()
                .Build();

            Console.WriteLine("Press any key to begin...");
            Console.ReadKey();

            //var uri = new Uri("http://localhost:6274/mcp");
            var uri = new Uri("https://localhost:5106");
            var options = new HttpClientTransportOptions
            {
                Endpoint = uri,
                TransportMode = HttpTransportMode.AutoDetect,
                ConnectionTimeout = TimeSpan.FromMinutes(10)
            };

            IClientTransport transport = new HttpClientTransport(options);

            // Create an MCPClient for the local McpServer1 (HTTP transport)
            var mcpClient = await McpClient.CreateAsync(transport);

            // Retrieve the list of tools available on McpServer1
            var tools = await mcpClient.ListToolsAsync().ConfigureAwait(false);
            foreach (var tool in tools)
            {
                Console.WriteLine($"{tool.Name}: {tool.Description}");
            }

            // Prepare and build kernel with the MCP tools as Kernel functions
            var ollamaClient = new OpenAIClient(
                new ApiKeyCredential("ollama"),
                new OpenAIClientOptions { Endpoint = new Uri("http://localhost:11434/v1"), NetworkTimeout = TimeSpan.FromMinutes(10) });

            var builder = Kernel.CreateBuilder();
            builder.Services
                .AddLogging(c => c.AddDebug().SetMinimumLevel(LogLevel.Trace))
                .AddOpenAIChatCompletion(
                    modelId: "llama3.2:1b",
                    openAIClient: ollamaClient);
            Kernel kernel = builder.Build();
            kernel.Plugins.AddFromFunctions("McpServer1", tools.Select(aiFunction => aiFunction.AsKernelFunction()));

            // Enable automatic function calling
#pragma warning disable SKEXP0001 // RetainArgumentTypes is experimental
            OpenAIPromptExecutionSettings executionSettings = new()
            {
                Temperature = 0,
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new() { RetainArgumentTypes = true })
            };
#pragma warning restore SKEXP0001

            // Test using McpServer1 tools
            var prompt = "Give me the current server time in English and Spanish.";
            var result = await kernel.InvokePromptAsync(prompt, new(executionSettings)).ConfigureAwait(false);
            Console.WriteLine($"\n\n{prompt}\n{result}");

            // Define the agent
            ChatCompletionAgent agent = new()
            {
                Instructions = "Respond to queries about the date/time and random numbers.",
                Name = "McpAgent",
                Kernel = kernel,
                Arguments = new KernelArguments(executionSettings),
            };

            // Respond to user input, invoking functions where appropriate.
            ChatMessageContent response = await agent.InvokeAsync("What is today's date?").FirstAsync();
            Console.WriteLine($"\n\nResponse from McpAgent:\n{response.Content}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
