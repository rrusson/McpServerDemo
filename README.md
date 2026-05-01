This is a very basic implementation of an MCP server in C#, using Microsoft's [quickstart template](https://learn.microsoft.com/en-us/dotnet/ai/quickstarts/build-mcp-server?pivots=visualstudio).

The solution inclues a simple console app that uses an LLM hosted on a [local instance of Ollama](http://localhost:11434)
or [Azure OpenAI](https://ai.azure.com/). Configure the console app as needed (I've been using Llama 3.2, one of the smaller models that supports tools).
