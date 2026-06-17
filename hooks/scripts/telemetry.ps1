# Telemetry hook for the Meta Quest Agentic Tools (meta-vr) plugin.
#
# Windows/PowerShell counterpart of telemetry.sh. Reads the PostToolUse
# hook JSON from stdin, classifies skill / tool / reference activity, and
# forwards a single event through the metavr CLI (from npm) via its hidden
# `telemetry log-event` subcommand. metavr owns consent.
#
# Privacy:
#   * Set METAVR_SKILLS_TELEMETRY=false to disable this hook entirely.
#   * Failures are swallowed; the hook never blocks tool execution.

$ErrorActionPreference = 'SilentlyContinue'

function Exit-Success {
    Write-Output '{"continue":true}'
    exit 0
}

if ($env:METAVR_SKILLS_TELEMETRY -eq 'false') { Exit-Success }

$rawInput = [Console]::In.ReadToEnd()
if ([string]::IsNullOrWhiteSpace($rawInput)) { Exit-Success }

try {
    $hook = $rawInput | ConvertFrom-Json
}
catch {
    Exit-Success
}

$toolName = if ($hook.tool_name) { $hook.tool_name } else { $hook.toolName }
$sessionId = if ($hook.session_id) { $hook.session_id } else { $hook.sessionId }
if (-not $toolName) { Exit-Success }

$toolInput = if ($null -ne $hook.tool_input) { $hook.tool_input } else { $hook.toolArgs }

# Detect host.
$transcript = ("$($hook.transcript_path)") -replace '\\', '/'
if ($env:COPILOT_CLI -eq '1') { $clientName = 'copilot-cli' }
elseif ($hook.hook_event_name) {
    if (("$($hook.tool_use_id)") -like '*__vscode*' -or $transcript -like '*/Code/*' -or $transcript -like '*/Code - Insiders/*') { $clientName = 'vscode' }
    elseif ($transcript -like '*.codex/*' -or $env:CODEX_HOME) { $clientName = 'codex' }
    elseif ($transcript -like '*.cursor/*') { $clientName = 'cursor' }
    else { $clientName = 'claude-code' }
}
else { $clientName = 'unknown' }

function Test-SkillPath([string]$p) {
    if (-not $p) { return $false }
    $n = ($p -replace '\\', '/').ToLower()
    if ($n -notmatch '/skills/') { return $false }
    return ($n -match 'meta-vr/' -or $n -match 'agentic-tools/' -or $n -match 'ai-dev-tools/' -or $n -match '\.agents/skills/')
}

function Get-RelAfterSkills([string]$p) {
    return (($p -replace '\\', '/') -replace '.*/skills/', '')
}

$eventType = ''
$skillName = ''
$toolField = ''
$fileRef = ''

if ($toolName -eq 'Skill' -or $toolName -eq 'skill') {
    $skillName = if ($toolInput.skill) { $toolInput.skill } else { $toolInput.name }
    $skillName = "$skillName" -replace '^meta-vr:', ''
    if ($skillName) { $eventType = 'skill_invocation' }
}

if (-not $eventType) {
    if ($toolName -like 'mcp__plugin_meta-vr_metavr__*' -or $toolName -like 'mcp__hzdb__*' -or $toolName -like 'mcp_metavr_*' -or $toolName -like 'metavr-*') {
        $toolField = $toolName
        $eventType = 'tool_invocation'
    }
}

if (-not $eventType -and ($toolName -eq 'Read' -or $toolName -eq 'read_file' -or $toolName -eq 'view')) {
    $p = $toolInput.file_path
    if (-not $p) { $p = $toolInput.filePath }
    if (-not $p) { $p = $toolInput.path }
    if ((Test-SkillPath $p)) {
        $rel = Get-RelAfterSkills $p
        if ($p.ToLower() -like '*/skill.md') {
            $skillName = ($rel -split '/')[0]
            $eventType = 'skill_invocation'
        }
        else {
            $fileRef = $rel
            $eventType = 'reference_file_read'
        }
    }
}

if (-not $eventType) { Exit-Success }

$payload = @{ client = $clientName }
if ($sessionId) { $payload.session_id = $sessionId }
if ($skillName) { $payload.skill = $skillName }
if ($toolField) { $payload.tool = $toolField }
if ($fileRef) { $payload.file_reference = $fileRef }
$data = $payload | ConvertTo-Json -Compress

try {
    & npx -y metavr telemetry log-event $eventType --source agentic_tools --data $data *> $null
}
catch {}

Exit-Success
