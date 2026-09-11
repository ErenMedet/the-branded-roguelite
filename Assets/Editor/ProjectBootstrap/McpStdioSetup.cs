using UnityEditor;
using UnityEngine;

namespace ProjectBootstrap
{
    // Claude's UnityMCP server runs over stdio, but MCP for Unity defaults to HTTP.
    // Switches the bridge to stdio once, then reloads scripts so the stdio bridge auto-starts.
    [InitializeOnLoad]
    static class McpStdioSetup
    {
        const string UseHttpKey = "MCPForUnity.UseHttpTransport";

        static McpStdioSetup()
        {
            if (!EditorPrefs.GetBool(UseHttpKey, true)) return;

            EditorPrefs.SetBool(UseHttpKey, false);
            Debug.Log("[McpStdioSetup] MCP for Unity switched to stdio transport; reloading scripts.");
            EditorApplication.delayCall += EditorUtility.RequestScriptReload;
        }
    }
}
