---
applyTo: '**'
---

# GitHub Flow Development Guide

This repository uses **GitHub Flow**. All code changes must follow this workflow.

---

## Flow Overview

```
main (protected branch)
 │
 ├─ Create feature/<issue-number>-<short-description> branch
 │   ├─ Work & commit (Conventional Commits)
 │   ├─ Verify with dotnet build / dotnet test
 │   └─ Create Draft PR
 │       ├─ When work is complete → change to Ready for Review
 │       ├─ Conduct review
 │       └─ Squash Merge → integrate into main
 │
 └─ Delete branch
```

---

## 1. Branch Naming

Always branch off `main`. **Never commit directly to `main`.**

```
<type>/<issue-number>-<short-description>
```

| Prefix | When to use |
|---|---|
| `feature/` | New functionality |
| `fix/` | Bug fix |
| `refactor/` | Refactoring |
| `docs/` | Documentation changes only |

Examples: `feature/42-add-partial-event-support`, `fix/15-incorrect-global-qualifier`

```bash
git switch main && git pull origin main
git switch -c feature/42-add-partial-event-support
```

---

## 2. Commit Convention (Conventional Commits)

All commit messages follow [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <description>
```

### Type

| type | Purpose |
|------|---------|
| `feat` | New feature |
| `fix` | Bug fix |
| `docs` | Documentation only |
| `style` | Formatting, whitespace (no logic change) |
| `refactor` | Refactor (no bug fix or feature) |
| `test` | Add or modify tests |
| `chore` | Build process or tooling changes |

### Scope (R3Events-specific)

| scope | Target |
|-------|--------|
| `generator` | R3EventsGenerator source generator implementation |
| `attributes` | R3EventsGenerator.Attributes public API |
| `tests` | Test projects |
| `docs` | Documentation |
| `ci` | GitHub Actions workflows |

Examples:
```
feat(generator): support partial event for record types
fix(attributes): correct nullability annotation on R3Event<T>
docs: translate spec to English
```

---

## 3. Build & Test Verification

Before creating a PR, verify locally:

```bash
dotnet build
dotnet test
```

Do not create a PR with build errors or failing tests.

---

## 4. Draft PR

Push your branch and create a **Draft Pull Request**.

PR title should follow the same Conventional Commits format:
```
feat(generator): support partial event for record types
```

PR body template:
```markdown
## Summary
<!-- Briefly describe the purpose and context of this change -->

## Changes
<!-- List the main changes -->
-

## Related Issue
Closes #<issue-number>

## Checklist
- [ ] `dotnet build` passes
- [ ] `dotnet test` passes
- [ ] New/changed logic has unit tests
```

---

## 5. Merge

Use **Squash and Merge** — keeps `main` history clean.

After merge, delete the feature branch.

---

## Prohibited

| ❌ Don't | ✅ Do instead |
|---|---|
| Commit directly to `main` | Always use a feature branch |
| Create PR with failing build/tests | Run `dotnet build` and `dotnet test` first |
| Force push to `main` | Use force push only on feature branches if needed |
| Merge without review | Always get at least one review approval |
| Use Merge Commit or Rebase Merge | Use Squash Merge only |
