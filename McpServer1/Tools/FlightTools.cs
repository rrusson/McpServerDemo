using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McpServer1.Tools
{
    internal class FlightTools
    {
        /// <summary>
        /// Gets airport codes (aka stations). Optionally gets codes just for Florida.
        /// </summary>
        /// <param name="floridaOnly">If set to "true" return only Florida airport codes. Accepts "true" or "false" (case-insensitive).</param>
        /// <remarks>MCP exposes methods as a snake_cased_conversion of the PascalMethodName (e.g. this is get_current_time) unless Name is specified.</remarks>
        /// <returns>A collection of airport codes (e.g. "FLL").</returns>
        [McpServerTool()]
        [Description("Gets airport codes (aka stations). Optionally gets codes just for Florida.")]
        public string[] GetAirportCodes(string floridaOnly = "false")
        {
            bool isFloridaOnly = string.Equals(floridaOnly, "true", StringComparison.OrdinalIgnoreCase);

            if (isFloridaOnly)
            {
                return new string[] { "FLL", "MCO", "TPA", "WPB" };
            }

            return new string[] { "ATL", "AUS", "AYC", "BOG", "BOS", "CLE", "CLT", "CTG", "CUN", "DFW", "ORD", "MCO", "TPA", "WPB", "JFK", "LAX", "ORD" };
        }
    }
}