---
description: Fetch latest articles about Claude Code, GitHub Copilot, and GLM updates
argument-hint: [claude-code|copilot|glm] [days]
allowed-tools: WebSearch, WebFetch
---

# AI Updates Command

This command fetches the latest articles and updates about Claude Code, GitHub Copilot, and GLM from official sources.

## Usage
- `/ai-updates` - Updates for all tools from the last 7 days
- `/ai-updates claude-code` - Claude Code updates from the last 7 days
- `/ai-updates copilot` - GitHub Copilot updates from the last 7 days
- `/ai-updates glm` - GLM updates from the last 7 days
- `/ai-updays claude-code 14` - Claude Code updates from the last 14 days
- `/ai-updates copilot 3` - GitHub Copilot updates from the last 3 days
- `/ai-updates 7` - Updates for all tools from the last 7 days

## Arguments
- `$ARGUMENTS`: Contains the command arguments passed by user

## Implementation

### 1. Parse Arguments
Extract tool names and day range from `$ARGUMENTS`:
- Default to 7 days if no number specified
- Default to all tools (claude-code, copilot, glm) if no tool specified
- Handle various argument orders (tool first, days first, or just one argument)

### 2. Calculate Date Range
Determine search period based on days parameter (default: 7 days back from today)

### 3. Search Official Sources
For each specified tool:
- **Claude Code**: Search Anthropic blog, official documentation, and announcements
- **GitHub Copilot**: Search GitHub blog, official announcements, and changelogs
- **GLM**: Search Zhipu AI official announcements and documentation

### 4. Fetch Article Details
For each search result found:
- Use WebFetch to get full article content by prepending `https://r.jina.ai/` to the URL (e.g., `https://r.jina.ai/https://github.blog/ai-and-ml/github-copilot/`). This ensures the page is downloaded in clean markdown format for easier processing.
- Extract title, publication date, and detailed summary
- Ensure information comes from official sources only
- **CRITICAL**: Extract and include direct links to the SPECIFIC articles, not just the main blog/domain pages. Users should be taken directly to the individual articles.

### 5. Generate Report
Format results as detailed report with:
- Clear section headings for each tool
- Chronologically ordered updates (most recent first)
- **CRITICAL**: Each update MUST include: title, publication date, summary, and DIRECT LINK TO THE SPECIFIC ARTICLE (not the main blog page)
- Total count of updates found per tool
- **ARTICLE-SPECIFIC LINKS**: Must be direct URLs to the individual articles, not domain or blog landing pages

## Error Handling
- Handle invalid tool names gracefully
- Default to all tools if no valid tool specified
- Provide clear feedback for invalid day ranges
- Return informative message if no updates found

## Output Format
- Chronologically ordered updates (most recent first)
- **CRITICAL**: Each update MUST include title, publication date, summary, and DIRECT LINK TO THE SPECIFIC ARTICLE
- Clear section headings for each tool
- Total count of updates found per tool
- **ARTICLE-SPECIFIC LINKS**: All links must be direct URLs to individual articles, not blog landing pages or domain homepages