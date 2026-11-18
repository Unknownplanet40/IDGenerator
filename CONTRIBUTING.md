# Contributing

## How to contribute

1. Fork this repo (or copy `temps-plates/` for a local project).
2. Open `/discovery` in Antigravity, or follow `guides/01-setup-and-discovery.md` for the 7 MVP questions.
3. Implement one task at a time from `docs/TASKS.md`. Bump `VERSION` only via CI.
4. Run lint, typecheck, tests, and build before committing. Report real output.

## Code style

- TypeScript strict only. No `any`, no silent catch blocks. Comments explain why, not what.
- C#: MVVM with CommunityToolkit.Mvvm. Match existing style.
- Formatting: Prettier for JS/TS (`npm run format`), EditorConfig for all files. Run the project's linter and formatter.
- No secrets in code or logs. Use `process.env.*` only.

## Tests

- Bug fix: write a failing test first, then fix.
- New feature: tests for happy path, edge cases, and invalid input.
- Never delete or weaken a test to make it pass.

## Git workflow

- Small commits, conventional commits (`feat:`, `fix:`, `perf:`, `refactor:`, `docs:`, `chore:`).
- Follow `.agents/rules/commit-workflow.md`. Always ask before `git push`.
- `CHANGELOG.md` is auto-generated history. Release notes show only latest changes in app. Do not edit `VERSION` or `version.json` manually.
- Never force push or run destructive commands without explicit confirmation.

## Issues and PRs

Use the templates in `.github/ISSUE_TEMPLATE/` and `.github/pull_request_template.md`. Flag affected areas (UI, Storage, CI/CD, Updates) and severity for bugs.
