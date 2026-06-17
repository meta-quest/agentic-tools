#!/bin/bash

# Telemetry hook for the Meta Quest Agentic Tools (meta-vr) plugin.
#
# Reads the PostToolUse hook JSON from stdin, classifies skill / tool / reference
# activity, and forwards a single event through the metavr CLI (installed from
# npm) via its hidden `telemetry log-event` subcommand. metavr owns consent: the
# event is gated by the user's `metavr telemetry consent` setting, so this hook
# never decides what is or isn't allowed to be sent.
#
# === Event types ===
#   skill_invocation    Skill tool called with a meta-vr skill, OR a SKILL.md
#                       read from a recognized agentic-tools install path.
#   tool_invocation     A metavr MCP tool was called.
#   reference_file_read A non-SKILL.md file under a skill dir was read.
#
# === Privacy ===
#   * Set METAVR_SKILLS_TELEMETRY=false to disable this hook entirely.
#   * Failures are swallowed; the hook never blocks tool execution.
#   * Only the skill name / tool name / in-repo reference path is sent — never
#     file contents, arguments, or user input.

set +e # never fail the tool on a telemetry error

return_success() {
    echo '{"continue":true}'
    exit 0
}

# Opt-out + only run when there is stdin to read.
[ "${METAVR_SKILLS_TELEMETRY}" = "false" ] && return_success
[ -t 0 ] && return_success

rawInput=$(cat)
[ -z "$rawInput" ] && return_success

# --- minimal, portable JSON field extraction (sed; no jq dependency) ---

# Top-level "field": "value"
json_field() {
    echo "$1" | sed -n "s/.*\"$2\":[[:space:]]*\"\([^\"]*\)\".*/\1/p"
}

# Nested tool_input.<field> / toolArgs.<field> "value"
toolinput_field() {
    local v
    v=$(echo "$1" | sed -n "s/.*\"tool_input\":[[:space:]]*{[^}]*\"$2\":[[:space:]]*\"\([^\"]*\)\".*/\1/p")
    [ -z "$v" ] && v=$(echo "$1" | sed -n "s/.*\"toolArgs\":[[:space:]]*{[^}]*\"$2\":[[:space:]]*\"\([^\"]*\)\".*/\1/p")
    echo "$v"
}

# Path from tool_input (file_path / filePath / path)
toolinput_path() {
    local p
    for key in file_path filePath path; do
        p=$(toolinput_field "$1" "$key")
        [ -n "$p" ] && {
            echo "$p"
            return
        }
    done
}

# --- parse the hook payload (Claude Code / Codex / Cursor: snake_case;
#     Copilot CLI: camelCase) ---
toolName=$(json_field "$rawInput" "tool_name")
[ -z "$toolName" ] && toolName=$(json_field "$rawInput" "toolName")
sessionId=$(json_field "$rawInput" "session_id")
[ -z "$sessionId" ] && sessionId=$(json_field "$rawInput" "sessionId")
[ -z "$toolName" ] && return_success

# --- detect the host so we can attribute events ---
if [ "$COPILOT_CLI" = "1" ]; then
    clientName="copilot-cli"
elif echo "$rawInput" | grep -q '"hook_event_name"'; then
    transcript=$(json_field "$rawInput" "transcript_path" | tr '\\' '/')
    toolUseId=$(json_field "$rawInput" "tool_use_id")
    if [[ "$toolUseId" == *"__vscode"* ]] || [[ "$transcript" == */Code/* ]] || [[ "$transcript" == */Code\ -\ Insiders/* ]]; then
        clientName="vscode"
    elif [[ "$transcript" == *".codex/"* ]] || [ -n "$CODEX_HOME" ]; then
        clientName="codex"
    elif [[ "$transcript" == *".cursor/"* ]]; then
        clientName="cursor"
    else
        clientName="claude-code"
    fi
else
    clientName="unknown"
fi

# A path belongs to this plugin if it sits under a skills/ dir in a recognized
# meta-vr / agentic-tools install location.
is_skill_path() {
    local p
    p=$(echo "$1" | tr '[:upper:]' '[:lower:]' | tr '\\' '/' | sed 's|//*|/|g')
    case "$p" in
    */skills/*)
        case "$p" in
        *meta-vr/* | *agentic-tools/* | *ai-dev-tools/* | *.agents/skills/*) return 0 ;;
        esac
        ;;
    esac
    return 1
}

# Relative path after the last "skills/" segment.
rel_after_skills() {
    echo "$1" | tr '\\' '/' | sed 's|//*|/|g' | sed 's|.*/skills/||'
}

eventType=""
skillName=""
toolField=""
fileRef=""

# 1. Skill tool invocation (Claude/Codex/Cursor: "Skill"; Copilot: "skill").
if [ "$toolName" = "Skill" ] || [ "$toolName" = "skill" ]; then
    skillName=$(toolinput_field "$rawInput" "skill")
    [ -z "$skillName" ] && skillName=$(toolinput_field "$rawInput" "name")
    skillName="${skillName#meta-vr:}" # strip Claude/Codex plugin namespace
    [ -n "$skillName" ] && eventType="skill_invocation"
fi

# 2. metavr MCP tool invocation.
if [ -z "$eventType" ]; then
    case "$toolName" in
    mcp__plugin_meta-vr_metavr__* | mcp__hzdb__* | mcp_metavr_* | metavr-*)
        toolField="$toolName"
        eventType="tool_invocation"
        ;;
    esac
fi

# 3. File reads: SKILL.md => skill_invocation; other in-skill file => reference.
if [ -z "$eventType" ] && { [ "$toolName" = "Read" ] || [ "$toolName" = "read_file" ] || [ "$toolName" = "view" ]; }; then
    p=$(toolinput_path "$rawInput")
    if [ -n "$p" ] && is_skill_path "$p"; then
        rel=$(rel_after_skills "$p")
        case "$(echo "$p" | tr '[:upper:]' '[:lower:]')" in
        */skill.md)
            skillName="${rel%%/*}"
            eventType="skill_invocation"
            ;;
        *)
            fileRef="$rel"
            eventType="reference_file_read"
            ;;
        esac
    fi
fi

[ -z "$eventType" ] && return_success

# --- build the --data JSON payload and forward via the metavr CLI ---
data="{\"client\":\"$clientName\""
[ -n "$sessionId" ] && data="$data,\"session_id\":\"$sessionId\""
[ -n "$skillName" ] && data="$data,\"skill\":\"$skillName\""
[ -n "$toolField" ] && data="$data,\"tool\":\"$toolField\""
[ -n "$fileRef" ] && data="$data,\"file_reference\":\"$fileRef\""
data="$data}"

npx -y metavr telemetry log-event "$eventType" --source agentic_tools --data "$data" >/dev/null 2>&1 || true

return_success
