using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McpServer1.Tools
{
    /// <summary>
    /// MCP tool for demonstration purposes.
    /// </summary>
    internal class DateTimeTools
    {
        /// <summary>
        /// Gets the current local date and time.
        /// </summary>
        /// <remarks>MCP exposes methods as a snake_cased_conversion of the PascalMethodName (e.g. this would be get_current_time) unless Name is specified.</remarks>
        /// <returns>A <see cref="DateTime"/> value representing the current date and time, expressed as local time.</returns>
        [McpServerTool(Name = "get_current_time")]
        [Description("Gets the current date and time.")]
        public DateTime GetCurrentTime()
        {
            return DateTime.Now;
        }
    }
}