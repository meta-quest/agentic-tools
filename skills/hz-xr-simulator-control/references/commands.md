# `metavr xrsim` — what `--help` does not tell you

`metavr xrsim readme` prints the group/command list straight from the binary and
`metavr xrsim --help` covers every flag. **Those are authoritative for spelling and
this file deliberately does not restate them** — a hand-copied flag table goes stale
silently, and a stale table is worse than no table because it gets trusted instead of
checked.

## Contents

- [Connection tier per group](#connection-tier-per-group)
- [Where the surface is irregular](#where-the-surface-is-irregular)
- [What the output means](#what-the-output-means)
- [Frontend shutdown scope](#frontend-shutdown-scope)
- [XR Operator backend port](#xr-operator-backend-port)
- [Multiple runtimes](#multiple-runtimes)
- [Log locations](#log-locations)
- [Older command spellings](#older-command-spellings)

What follows is only the parts the generated help cannot express: which tier a command
needs, where the surface is irregular enough to guess wrong, and what the output
actually means.

## Connection tier per group

| Tier | Commands | Needs |
|---|---|---|
| Local | `readme`, `runtime logs`, and on Linux bare `record --output … --duration …` | nothing running |
| Lifecycle | `app launch`/`quit`/`ensure-running` | nothing running — these *manage* the frontend |
| Frontend | `runtime activate`/`deactivate`/`list`, `env set`/`list` | the frontend app |
| Runtime | `runtime ping`/`status`/`info`/`fps`, `device *`, `input *`, `layer *`, `record start`/`stop`/`status` | the frontend **and** a connected XR app |

Two sit higher than they look. `device list` and `device refresh-rates` read like static
catalogs but the accepted values come from the live runtime. `input sources` prints a
fixed list yet still needs a connection, because the `input` group opens one before it
dispatches.

Two sit lower. No `app` subcommand requires a running frontend — the group is how you
start, stop and recover one, so it is the way out of the frontend-down error rather than
a casualty of it. And bare `record` on Linux never connects at all.

## Where the surface is irregular

These are the spots where a reasonable guess is wrong. Everything else follows from
`--help`.

| Shape | Commands | The trap |
|---|---|---|
| Positional, not a flag | `input follow <body\|head>`, `input compatibility-mode <on\|off>`, `layer toggle <id> <on\|off>` | `--mode`, `--active` and `--state` are all rejected here |
| `--active true\|false` | `input controller`, `input headset`, `input point-and-click` | takes a literal word, not a bare boolean flag — and `on`/`off` are not accepted, unlike the positional commands above |
| The value flag is named per command | `device set --name`, `device ipd --value`, `device refresh-rate --rate` | three different spellings for "the value" |
| `env set --id` | `env` | accepts a room ID *or* a display name, but the flag is `--id` either way; `--name` is rejected |
| Two different recorders under one name | `record` vs `record start`/`stop`/`status` | on Linux the bare form is a local ffmpeg framebuffer capture needing no connection, while the subcommands are runtime-tier and fail outright with `native recording subcommands require --runtime-addr on Linux` |

Validation errors list the legal values (`unsupported refresh rate 77 Hz; valid rates:
…`). Read the list out of the error rather than guessing a second time.

## What the output means

- **`runtime status` is partially fault-tolerant by default.** It queries device, input and FPS
  independently and can exit 0 having failed one of them, reporting `device_error` /
  `input_error` / `fps_error` alongside whatever succeeded. Use `--strict` in automation to make
  any failed query return non-zero while still printing the report; keep reading the error fields
  to identify the failed section.
- **Device setters distinguish requested and observed state.** `device set`, `device ipd`, and
  `device refresh-rate` read settings back after the mutation. Table output labels the read-back
  `OBSERVED`; JSON exposes `observed`, `observed_mm`, or `observed_hz`. Without `--wait` the single
  read may precede a slow update. With `--wait`, the command verifies convergence. `unknown` means
  the read-back failed.
- **`layer list` reports `ENABLED` in tables and both polarities in JSON.** Read `enabled` for the
  same polarity as `layer toggle`. The legacy `disabled` JSON field remains its exact inverse for
  existing consumers.
- **`app launch` / `app quit` report state, not just success.** Quit reports `pid` and `forced`
  alongside `was_running`, `killed`, and `confirmed_stopped`; read the fields, not only the exit
  code.
- **Only `--format json` is a stable contract.** Table columns are for humans and change
  freely.

## Frontend shutdown scope

`app quit` resolves the MetaXRSimulator listening on `--port` and asks only that process to close.
Use `--force` to terminate that same process only after graceful shutdown fails. Use `--all` only
when the intent is to stop every MetaXRSimulator on the host; it matches by image name and is not
a routine cleanup fallback.

If `app quit --help` lacks `--all` or `runtime status --help` lacks `--strict`, update metavr before
depending on these contracts.

## XR Operator backend port

The in-app XR Operator layer and metavr both read `AGENTICXR_MCP_PORT`, defaulting to 8720. Set the
same value in both process environments. metavr passes the resulting backend URL to the federated
proxy, so a mismatch sends the proxy to the wrong app server.

## Multiple runtimes

One frontend can host several runtimes. `runtime ping`, `device`, `input` and `layer`
follow the frontend's *current* endpoint, which is ambiguous once more than one is
attached. Enumerate with `runtime list`, then pin with the global `--runtime-addr`, which
also bypasses frontend discovery entirely — useful when the frontend is down but a
runtime is still alive.

## Log locations

Resolved without anything running, via `runtime logs` (add `--tail N` for content).

| Platform | Runtime logs | Frontend log |
|---|---|---|
| macOS | `~/Library/Application Support/MetaXR/MetaXrSimulator/logs/` | under the local app-data dir |
| Windows | `%APPDATA%\MetaXR\MetaXrSimulator\logs\` | same |
| Linux | `~/.local/share/MetaXR/MetaXrSimulator/logs/` | same |

## Older command spellings

A number of ungrouped spellings still parse (`status`, `ping`, `set-device`,
`toggle-layer`, …) and are hidden from `--help` so existing scripts keep working. Write
the grouped form in anything new: a hidden command has no help text, so the next reader
cannot discover its flags.
