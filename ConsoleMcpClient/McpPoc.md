**Workflow Sequence Diagram**

This diagram below shows the step-by-step flow of a request, from the user's prompt to the final answer.
1.	The ConsoleMcpClient starts and fetches the available tools from McpServer1.
2.	It configures Semantic Kernel with these tools.
3.	The user provides a prompt.
4.	Semantic Kernel sends the prompt and tool definitions to the LLM.
5.	The LLM determines a tool is needed and asks the Kernel to execute it.
6.	The Kernel invokes the tool through the MCP client, which calls the McpServer1.
7.	The server runs the tool and returns the result.
8.	The Kernel sends the tool's result back to the LLM, which generates the final human-readable answer.
9.	The client displays the answer to the user.

-----
```mermaid
%%{init: {'theme': 'base', 'themeVariables': { 'textColor': '#000000', 'primaryTextColor': '#000000', 'secondaryTextColor': '#000000', 'tertiaryTextColor': '#000000', 'lineColor': '#333333' }}}%%
flowchart TB
    subgraph Client["Client Process"]
        direction LR
        U["User"]
        CC["ConsoleMcpClient"]
        SK["Semantic Kernel"]
        MC["MCP Client"]
        LLM["LLM Service<br/>(e.g., Ollama)"]

        U --> CC
        CC -->|Submit prompt / show response| SK
        SK -->|LLM inference| LLM
        SK -->|Invoke tools through| MC
    end

    MCPHTTP@{ shape: bolt, label: "HTTP/S MCP calls" }

    subgraph Server["Server Process"]
        direction LR
        MS["McpServer1<br/>ASP.NET Core"]
        DT["DateTimeTools"]
        FT["FlightTools"]
        GT["GetCurrentTime()"]

        MS -->|Hosts / executes| DT
        MS -->|Hosts / executes| FT
        DT --> GT
    end

    Client --> MCPHTTP --> Server

    classDef user fill:#D5E8D4,stroke:#82B366,stroke-width:2px,color:#000000;
    classDef client fill:#DAE8FC,stroke:#6C8EBF,stroke-width:2px,color:#000000;
    classDef kernel fill:#E1D5E7,stroke:#9673A6,stroke-width:2px,color:#000000;
    classDef llm fill:#FFE6CC,stroke:#D79B00,stroke-width:2px,color:#000000;
    classDef server fill:#F8CECC,stroke:#B85450,stroke-width:2px,color:#000000;
    classDef tool fill:#F5F5F5,stroke:#666666,stroke-width:2px,color:#000000;
    classDef transport fill:#FFF2CC,stroke:#D6B656,stroke-width:2px,color:#000000;
    classDef default color:#000000;

    class U user;
    class CC,MC client;
    class SK kernel;
    class LLM llm;
    class MS server;
    class DT,FT,GT tool;
    class MCPHTTP transport;

    style Client color:#000000,stroke:#999999,fill:#FFFFFF;
    style Server color:#000000,stroke:#999999,fill:#FFFFFF;

    click CC call linkCallback("/c:/Temp/McpServer1/ConsoleMcpClient/Program.cs#L11")
    click DT call linkCallback("/c:/Temp/McpServer1/McpServer1/Tools/DateTimeTools.cs#L8")
    click FT call linkCallback("/c:/Temp/McpServer1/McpServer1/Tools/FlightTools.cs#L5")
    click GT call linkCallback("/c:/Temp/McpServer1/McpServer1/Tools/DateTimeTools.cs#L17")
```
