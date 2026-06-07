# Contributing to Cirrus

Thank you for your interest in contributing to Cirrus! We welcome issues, bug reports, documentation improvements, and pull requests. To help us review and merge contributions quickly, please follow the guidelines below.

## Communication & issues
- Search existing issues and pull requests before opening a new one to avoid duplicates.
- When opening an issue, include:
  - A concise title
  - Environment information (Windows version, .NET SDK version, branch/commit)
  - Steps to reproduce (minimal reproduction preferred)
  - Expected vs actual behavior
  - Relevant logs, stack traces, and screenshots

## How to contribute code
- Fork the repository and create a topic branch from the default branch. Suggested branch names: `feat/<short>`, `fix/<short>`, `docs/<short>`, `chore/<short>`.
- Keep your branch up-to-date with the upstream default branch.
- Keep changes focused and atomic: one PR should address one logical change.

## Pull request guidelines
- Open a PR from your fork/branch and include in the description:
  - What problem you are solving and why
  - How to build and test your change locally
  - Any backwards-incompatible changes
  - Screenshots or recordings for UI changes
- Use conventional commit-style messages where possible (e.g., `feat(playback): add gapless playback`).
- Link related issues in the PR description (e.g., `Fixes #123`).

## Code style & quality
- Follow existing C# and project conventions. Match surrounding code style.
- Run static analysis and formatting before submitting. We recommend using `dotnet format`.
- Add unit or integration tests for bug fixes and new features when appropriate.
- Ensure all tests pass locally before submitting a PR.

## Branching & commit conventions
- Branch naming suggestions:
  - `feat/<short-description>`
  - `fix/<short-description>`
  - `docs/<short-description>`
  - `chore/<short-description>`
- Commit message format (recommended):
  - `type(scope): short description`
  - Example: `fix(network): handle connection timeout in client`

## Review & merging
- PRs should pass CI (build + tests) to be merged.
- At least one maintainer review is required for non-trivial changes.
- Maintainers may request changes, tests, or additional documentation before merging.

## Large or breaking changes
- For large or breaking changes, open an issue or RFC describing the design before implementing.
- Provide migration instructions and a plan to maintain backward compatibility where feasible.

## Code of conduct
- Be respectful and professional. Harassment, discrimination, or abusive behavior is not tolerated.
- Report violations to the maintainers.

## PR checklist (for contributors)
- [ ] I have read and followed `CONTRIBUTING.md`.
- [ ] I ran the build locally and verified it compiles.
- [ ] I added/updated tests where appropriate and ensured tests pass.
- [ ] I formatted my code according to project conventions.
- [ ] My PR description explains the change, how to test it, and any backward compatibility impacts.

## License note
This repository currently includes a `LICENSE` file. Please read it before contributing. If the license changes (for example, to restrict commercial use), maintainers will update this document and inform contributors; changing a project's license may require contributor consent for previously merged contributions.

---
Thank you for helping improve Cirrus. Your contributions are appreciated!