# Contributing to meta-quest/agentic-tools

## Welcome

Thanks for your interest in contributing to this repository.

This repository contains agent skills and reference documentation for Meta Quest and Horizon OS development. Before contributing, please review our [Code of Conduct](CODE_OF_CONDUCT.md).

## How to Contribute

Contributions are accepted through GitHub pull requests.

1. Fork the repository.
2. Create a feature branch from `main`.
3. Make your changes in a focused commit series.
4. Open a pull request with a clear description of what changed and why.

For issues and feature requests, use GitHub Issues:
https://github.com/meta-quest/agentic-tools/issues

## Skill Structure

Each skill lives in its own directory under `skills/` and should follow this pattern:

- `SKILL.md` -- the skill definition and workflow instructions
- optional supporting directories such as `references/`, `scripts/`, `assets/`, `examples/`, and `agents/`

Use [skills/hz-vr-debug/SKILL.md](skills/hz-vr-debug/SKILL.md) and [skills/hzdb-cli/SKILL.md](skills/hzdb-cli/SKILL.md) as current examples for structure, style, and level of detail.


## Skill authoring summary

When writing or updating skills:

- Every skill requires a `SKILL.md` file.
- Supporting directories such as `references/`, `scripts/`, `assets/`, `examples/`, and `agents/` are optional. Do not assume `references/` is the only valid layout.
- Prefer linking to [docs/hzdb.md](docs/hzdb.md) or existing `skills/hzdb-cli/references/` content instead of duplicating common hzdb command guidance.
- Keep guidance direct, practical, and action-oriented
- Use clear GitHub Flavored Markdown (GFM) formatting
- Prefer short sections and scannable checklists
- Keep documentation concise and avoid unnecessary background
- Include concrete commands and verification steps when helpful

## Reporting Bugs

If you find an issue in a skill or reference document, open an issue at:
https://github.com/meta-quest/agentic-tools/issues

Please include:

- The skill name and file path
- A clear description of the problem
- Suggested correction (if available)

## Contributor License Agreement (CLA)

All contributors must sign the Meta Contributor License Agreement (CLA) before pull requests can be merged.

## Code of Conduct

This project follows the [Contributor Covenant Code of Conduct](CODE_OF_CONDUCT.md). Please report unacceptable behavior to opensource-conduct@meta.com.
