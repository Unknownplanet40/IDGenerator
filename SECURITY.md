# Security Policy

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| Latest minor in main | Yes                  |
| Older minors         | No - upgrade please  |

## Reporting a Vulnerability

Report security vulnerabilities privately through [GitHub Security Advisories](https://github.com/Unknownplanet40/temp-plate/security/advisories/new).

Do not open a public issue for a vulnerability. The maintainers will acknowledge within 48 hours and triage within 5 business days.

## What to include

- A clear summary of the issue
- Affected component (web or WinUI3)
- Steps to reproduce
- Impact assessment (data exposure, privilege escalation, DoS)
- Any proof-of-concept code

## Disclosure timeline

1. Maintainer acknowledges receipt
2. Investigates and patches in a private branch
3. Releases a fix with `fix(security):` or `fix():` commit
4. Publishes a GitHub Advisory with CVE if applicable
5. Updates `CHANGELOG.md` in the next release

Reporters are credited in the release notes unless they request anonymity.
