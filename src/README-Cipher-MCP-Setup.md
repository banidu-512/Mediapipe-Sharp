# Cipher MCP Server Setup Guide

## Installation Completed ✅

The Cipher MCP server from https://github.com/campfirein/cipher has been successfully installed and configured.

## What Was Set Up

1. **Cipher MCP Server**: Installed globally via npm (`@byterover/cipher`)
2. **Configuration File**: Created `cline_mcp_settings.json` with the server configuration
3. **Environment Template**: Created `.env.example` file for API keys

## Configuration Details

The `cline_mcp_settings.json` file contains:

```json
{
  "mcpServers": {
    "github.com/campfirein/cipher": {
      "type": "stdio",
      "command": "cipher",
      "args": ["--mode", "mcp"],
      "env": {
        "MCP_SERVER_MODE": "aggregator",
        "OPENAI_API_KEY": "your_openai_api_key_here",
        "ANTHROPIC_API_KEY": "your_anthropic_api_key_here"
      }
    }
  }
}
```

## Next Steps Required

### 1. Set Up API Keys
You need to configure at least one API key. Copy the environment template and add your keys:

```bash
cd cipher-mcp-server
copy .env.example .env
# Edit .env file with your actual API keys
```

### 2. Update cline_mcp_settings.json
Replace the placeholder API keys in the `cline_mcp_settings.json` file with your actual keys.

### 3. Supported LLM Providers
- OpenAI (requires `OPENAI_API_KEY`)
- Anthropic (requires `ANTHROPIC_API_KEY`)
- Google Gemini (requires `GEMINI_API_KEY`)
- Qwen (requires `QWEN_API_KEY`)

## Available Tools

Once configured, Cipher provides these MCP tools:
- `cipher_extract_and_operate_memory`: Extract and operate on memory
- `cipher_memory_search`: Semantic search over stored knowledge
- `cipher_store_reasoning_memory`: Store reasoning traces
- `cipher_workspace_search`: Search team workspace memory
- `cipher_workspace_store`: Store team project signals
- `cipher_bash`: Execute bash commands
- And many more for knowledge graph operations

## Testing the Installation

To test the MCP server, you can run:
```bash
set OPENAI_API_KEY=your_key_here
set ANTHROPIC_API_KEY=your_key_here
cipher --mode mcp
```

The server is now ready to be used with Cline and other MCP-compatible clients!
