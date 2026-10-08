# AGENTS.md — instructions for coding agents

You are rebuilding a learning-oriented .NET microservices project, **one chapter at a time**.
The human owner wants to *understand* the architecture, so process matters as much as code.

## Source of truth

- Plan and acceptance criteria: `docs/ROADMAP.md` (read it fully before doing anything).
- Upstream reference (MIT, by Dao Hai Nam): https://github.com/daohainam/simple-store
  - Per-chapter design notes: `docs/vN-changes.md`, plus `docs/checkout-saga.md`, `docs/payment-service.md`, `docs/versioning.md`.
  - Vietnamese guide: `docs/guide/vi/`.
  - Upstream `CLAUDE.md` is a dense changelog; useful for cross-checking.
- Read the upstream chapter note **before** writing code. Prefer re-implementing from the design
  notes over bulk copy-paste. Adapting upstream code is allowed (MIT) — keep the attribution in
  README/LICENSE intact.

## Hard rules

1. **One chapter = exactly one commit + one annotated tag** (`v0`, `v1`, … `v8a`, `v8b`, …).
   Commit message: `chapter(vN): <short title>`. Amend locally until the chapter is done; never
   rewrite history that has already been pushed.
2. **Stay inside the chapter's scope.** Do not pull in features from later chapters
   (e.g. do not add RabbitMQ in v3, do not add API versioning before v11).
3. **Green build before commit:** `dotnet build SimpleStore.slnx -c Release` must succeed with 0 errors.
   If Docker is available, also run the AppHost and perform the chapter's smoke checks.
   If you cannot run something, say so explicitly in your report — never claim it passed.
4. **No secrets in git.** Use `dotnet user-secrets` / Aspire parameters (see `scripts/setup-secrets.sh`).
   Demo credentials from upstream (`admin@simplestore.local`, `demo@simplestore.local`) are seed data only.
5. **Write the chapter note** `docs/chapters/vN.md` **in Vietnamese** (code identifiers stay English), using
   `docs/chapters/_TEMPLATE.md`. It must explain the *why*, the trade-offs, and end with 3–5
   self-check questions. Do not just paraphrase upstream; explain it for a learner.
6. Keep placeholder projects compiling. A project that is not yet "awake" must still build.
7. Update the status table at the bottom of `docs/ROADMAP.md` as part of the chapter commit.

## Workflow per chapter

1. Read ROADMAP entry + upstream note for the chapter.
2. Implement; keep diffs focused.
3. Build, run, smoke-test.
4. Write `docs/chapters/vN.md`, tick the ROADMAP status.
5. `git add -A && git commit -m "chapter(vN): ..." && git tag -a vN -m "..."`
6. `git push origin main --follow-tags` when the owner's `origin` is configured; otherwise report that publishing is pending. Never push to the upstream reference repository.
7. **Stop and report** (what changed, what was verified, what was not, open questions).
   Wait for the owner to say "next" before starting the following chapter.

## Conventions

- Target `net10.0`, nullable enabled (see `Directory.Build.props`). Package versions: match upstream unless a build error forces otherwise.
- Minimal APIs, no controllers in backends. Services own their data; **no cross-database foreign keys**.
- Aspire resource names: kebab-case (`catalog-api`, `catalogdb`, `cart-redis`, `rabbitmq`, `kurrentdb`).
- Upstream has no test projects; adding tests is optional and must not widen a chapter's scope.

## Skeleton baseline

These rules also apply to `v-skeleton`. It creates placeholders only; wait for "next" before v0.
