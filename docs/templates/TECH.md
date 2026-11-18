# Tech

## Stack
- Ask user for stack if not specified. Don't pick a framework silently.
- Prefer the platform and standard library first. Add dependency only if it saves real work, is maintained, and is widely used.
- Before adding any package, state why and what the alternative was. Pin versions. No unmaintained or tiny single-author packages.

## Data and Auth
- [ ] Entities, fields, and DB constraints (NOT NULL, UNIQUE, FKs in DB, not only code)
- [ ] Auth method, roles, and protected routes
- [ ] Auth checks on every protected route and every API call
- [ ] Ownership checks server-side (don't trust client IDs)
- [ ] Indexes on filtered/joined columns; migrations only, no hand-edited schema

## APIs and Third-Party

## Secrets and Env Vars
- Secrets live in environment variables. Never in code or git. Provide .env.example with fake values.
- Rate limiting: auth endpoints 5 req / 15 min per IP, general API 100 req / min per IP via Upstash/Redis. Return 429 with Retry-After.

## Security
- Validate all input on the server. Client validation is UX only.
- Use parameterized queries only. Never build SQL from strings.
- Escape output. No innerHTML with user data.
- Hash passwords with Argon2id or bcrypt. Never roll your own crypto.
- CSRF protection, secure cookies (HttpOnly, Secure, SameSite).
- Never log passwords, tokens, or personal data.

## Deployment

## WinUI3 CI (only for native Windows projects)
- GitHub Actions + Azure Trusted Signing for MSIX build/sign/publish. See `.github/workflows/winui3-release.yml` template.
- Public MSIX requires a trusted signing certificate (trusted CA or Azure Trusted Signing). Self-signed cert is dev-only and must be installed on test machines under Trusted People.
