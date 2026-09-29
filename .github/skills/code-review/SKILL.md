---
name: code-review
description: Review code changes in Swashbuckle.AspNetCore for correctness, OpenAPI fidelity, compatibility, performance, and consistency with project conventions. Use when reviewing PRs or code changes.
---

# Swashbuckle.AspNetCore Code Review

Review code changes against the conventions and constraints of Swashbuckle.AspNetCore: a library that generates
OpenAPI documents describing the *actual* behavior of ASP.NET Core applications, and that is consumed by a very
large number of downstream applications, tools, and code generators.

**Reviewer mindset:** Be polite but skeptical. Your job is to help speed up the review process for maintainers,
which includes not only finding problems the PR author may have missed but also questioning the value of the PR in
its entirety. Treat the PR description and linked issues as claims to verify, not facts to accept.

## When to Use This Skill

Use this skill when:

- Reviewing a PR or code change in this repository
- Checking code for correctness, compatibility, performance, or consistency issues before submitting a PR
- Asked to review, critique, or provide feedback on code changes

## Review Process

### Step 0: Load Relevant Instructions

Before analyzing anything, load:

1. [`AGENTS.md`](../../../AGENTS.md) at the repository root. Its architecture overview, public API tracking rules,
   warnings policy, key conventions, and general guidelines are part of the rule set for this review and are not
   repeated here.
2. [`pr-assessment.md`](pr-assessment.md) in this directory: the criteria used to write the Motivation,
   Approach, and Summary fields of the review.
3. [`CONTRIBUTING.md`](../../../CONTRIBUTING.md), in particular the project goal that generated documents are driven
   by *actual* runtime behavior rather than *intended* behavior.

If any of these files cannot be loaded, say so in the review and fall back to a careful first-principles review.

### Step 1: Gather Code Context (No PR Narrative Yet)

Collect as much relevant **code** context as you can. **Do NOT read the PR description, linked issues, or existing
review comments yet.** Form your own independent assessment of what the code does, why it might be needed, and what
problems it has before being exposed to the author's framing.

1. **Diff and file list**: Fetch the full diff and the list of changed files.
2. **Full source files**: For every changed file, read well beyond the diff hunks. Diff-only review is the main cause of
   false positives and missed issues. Many generation classes (for example `SchemaGenerator`, `SwaggerGenerator`, and
   `JsonSerializerDataContractResolver`) are large; read the enclosing methods, their callers, and the types involved.
3. **Consumers and callers**: If the change modifies a public type, a filter interface, an options type, or an
   extension method, search for its usages across `src/`, `test/`, and `test/WebSites/`.
4. **Sibling code paths**: Swashbuckle.AspNetCore has many parallel implementations that usually need to change together.
   Check whether the same fix or feature is needed in:
   - `JsonSerializerDataContractResolver` (System.Text.Json) **and** `NewtonsoftDataContractResolver` (Newtonsoft.Json).
   - Synchronous **and** asynchronous variants of generator methods and filters (`IDocumentFilter`/`IDocumentAsyncFilter`,
     `IOperationFilter`/`IOperationAsyncFilter`, etc.).
   - Controller-based endpoints **and** minimal API endpoints.
   - OpenAPI 2.0, 3.0 **and** 3.1 serialization of the same document.
   - `SwaggerUI` **and** `ReDoc` middleware and options.
   - Annotations-based metadata (`Swashbuckle.AspNetCore.Annotations`), XML comments, and data annotations that describe
     the same concept.
5. **Git history**: Run `git log --oneline -20 -- <file>` for changed files. Look for related recent changes, reverts,
   or previous attempts to fix the same problem.
6. **Public API surface**: Check for changes to any `PublicAPI/PublicAPI.*.txt` file or new/changed `public` or
   `protected` members in `src/`. Note whether the public API changed; this is used in Step 4.
7. **Snapshot changes**: Check for changes to `*.verified.*` files under `snapshots/` directories. Every snapshot change
   is an observable change in the generated output for users and must be explained by the code change.

### Step 2: Form an Independent Assessment

Based **only** on the code context gathered above, answer these questions:

1. **What does this change actually do?** What was the old behavior and what is the new behavior? What changes in the
   OpenAPI documents generated for existing applications?
2. **Why might this change be needed?** Infer the motivation from the code itself.
3. **Is this the right approach?** Would a simpler alternative be more consistent with the codebase? Could the goal be
   achieved with an existing extension point (a filter, `MapType`, `CustomSchemaIds`, `CustomOperationIds`, etc.) instead
   of new code or new options?
4. **What problems do you see?** Identify bugs, edge cases, OpenAPI spec violations, compatibility breaks, performance
   regressions, test gaps, and anything else that concerns you.

Write down your independent assessment, including a holistic assessment using [`pr-assessment.md`](pr-assessment.md),
before proceeding.

### Step 3: Incorporate PR Narrative and Reconcile

Now read the PR description, labels, linked issues (in full), existing review comments, and any related open issues.
Treat all of this as **claims to verify**.

1. **Linked issues** often contain the repro and the constraints the fix must satisfy. Check that the fix actually
   addresses the reported scenario, and not only a simplified version of it.
2. **Related issues**: Search for other open issues in the same area that the PR might also address or conflict with.
3. **Existing review comments**: Avoid duplicating feedback that has already been given.
4. **Reconcile.** Where your independent reading disagrees with the PR description, investigate further rather than
   deferring to the author. If the PR claims a bug fix or performance improvement, verify it against the code and the
   evidence provided. Problems your independent assessment found that the PR narrative does not acknowledge are more
   likely to be real, not less.
5. **Update your holistic assessment** only if the additional context genuinely changes your evaluation.

### Step 4: Repository-Specific Checks

Apply each of the following checks that is relevant to the diff.

#### Public API and compatibility

- Public API changes must be reflected in the `PublicAPI.Unshipped.txt` files, as described in `AGENTS.md`. The analyzer
  enforces the *mechanics*; your job is to judge whether the API change is *warranted* at all. `AGENTS.md` says not to
  change the public API unless specifically requested, so an unrequested public API change is a ❌ error.
- Removing or changing the signature of a shipped member, changing a default value, or changing an optional parameter
  list is a binary breaking change. It is caught by package validation against `PackageValidationBaselineVersion`, but
  only a maintainer can decide whether it is acceptable.
- Watch for **behavioral** breaking changes that no tool catches: different schema IDs, operation IDs, `$ref` shapes,
  property ordering, `required`/`nullable` semantics, enum representation, or parameter styles. These change the output
  of client code generators (NSwag, Kiota, OpenAPI Generator, etc.) for downstream users even when the document is still
  valid. Flag these explicitly, and suggest an opt-in option when the change is not a clear bug fix.
- Prefer adding overloads or new options over changing existing signatures. New options should default to the existing
  behavior.

#### OpenAPI correctness

- The generated document must be valid for **every** OpenAPI version the change affects (2.0, 3.0, and 3.1). Features
  that only exist in one version (for example 3.1 type arrays and `null` types, or 2.0 `formData` parameters) must
  degrade correctly in the others.
- The generated document must describe what the application *actually* does at runtime (see `CONTRIBUTING.md`). A
  schema that disagrees with how the configured serializer really reads/writes a type is a bug, even if the schema is
  valid OpenAPI.
- Check that `SchemaRepository` is used consistently: reusable types should be referenced via `$ref` in
  `components/schemas` (or `definitions` for 2.0), schema IDs must be unique and stable, and recursive/self-referencing
  types must not cause infinite recursion.

#### Serializer behavior

- Changes to how types are described must honor the configured `JsonSerializerOptions` (naming policy, number handling,
  enum converters, ignore conditions, `JsonPolymorphic`/`JsonDerivedType`, required members, etc.) and, for the
  Newtonsoft package, the equivalent `JsonSerializerSettings` and contract resolver behavior.
- If a change applies to only one serializer, check whether that asymmetry is intentional.

#### Target frameworks

- Libraries multi-target the supported .NET versions (see `DefaultTargetFrameworks` in `Directory.Build.props`). Check
  that framework-specific code uses the correct `#if NET*` conditions, that APIs used exist on every target, and that
  per-TFM `PublicAPI` files and snapshot directories are updated consistently.
- `test/WebSites/WebApi.Aot` sets `PublishAot`, so the trimming and native AOT analyzers run over the code it uses.
  Flag new reflection patterns that are not trim-safe in code paths that such applications depend on.

#### Tests

- As required by `AGENTS.md`, a bug fix **must** include a test that fails without the fix. Verify this by reasoning
  about the test against the old code, not just by the presence of a new test.
- New behavior should be covered at the appropriate level: unit tests in the `*.Test` projects for generator logic, and
  integration tests (with a sample application under `test/WebSites/` if needed) for end-to-end behavior.
- Snapshot (`*.verified.*`) changes must be consistent with the code change and must be updated for **all** target
  frameworks. Unexplained snapshot churn (for example reordering, or changes in unrelated documents) is a warning sign
  of an unintended behavior change.
- Tests should assert on the relevant parts of the generated document rather than relying solely on large snapshots
  when a targeted assertion expresses the intent more clearly.

#### UI packages

- `SwaggerUI` and `ReDoc` embed npm packages. Changes to their bundled versions are made via `package.json` and the
  lock file in the relevant `src/` directory, not by committing modified vendor files.
- Changes to the HTML/JavaScript served by the middleware must not introduce XSS, for example by writing unencoded
  configuration values into the page.

#### Documentation

- User-facing features and options should be documented under `docs/`. Code samples in the documentation are
  generated from `test/WebSites/DocumentationSnippets` using MarkdownSnippets, so samples should be added there rather
  than written directly into Markdown.

### Step 5: Detailed Analysis

1. **Focus on what matters.** Prioritize bugs, incorrect or invalid generated documents, compatibility breaks, security
   issues, performance regressions, and test gaps. Do not comment on trivial style issues.
2. **Consider collateral damage.** For every changed code path, brainstorm what other types, endpoints, options, and
   serializer configurations flow through it. Swashbuckle.AspNetCore processes arbitrary user types and APIs, so edge
   cases (generics, nested/recursive types, nullable value and reference types, dictionaries, enums, polymorphism,
   `IFormFile`, records, init-only and required members) are common in practice. If you identify a plausible risk, even
   one you cannot fully confirm, surface it.
3. **Be specific and actionable.** Every comment should say exactly what to change and why, and include evidence of how
   you verified the issue is real (e.g. "looked at all callers of `GenerateSchema` and none pass a `memberInfo` here").
4. **Flag severity clearly:**
   - ❌ **error** — Must fix before merge. Bugs, invalid OpenAPI, security issues, unrequested public API or breaking
     changes, and missing tests for behavior changes.
   - ⚠️ **warning** — Should fix. Performance issues, missing sibling changes (e.g. Newtonsoft or async variants),
     unexplained snapshot changes, and inconsistency with established patterns.
   - 💡 **suggestion** — Consider changing. Minor readability improvements or optional optimizations.
5. **Don't pile on.** If the same issue appears many times, flag it once and list all affected locations.
6. **Respect existing style.** The style of the file being changed takes precedence over general preferences.
7. **Don't flag what CI catches.** The compiler (with `TreatWarningsAsErrors`), analyzers, public API analyzers,
   package validation, markdownlint, actionlint, and zizmor all run in CI. Do not flag issues they would report.
8. **Avoid false positives.** Verify each concern against the full context, skip theoretical concerns with negligible
   real-world probability, and if you are unsure either investigate further or clearly phrase the concern as a
   question. Never assert that an API "does not exist" or "is deprecated" based on training data alone.
9. **Ensure code suggestions are valid.** Suggested code must compile, be complete, and match the surrounding
   indentation and code style.
10. **Label in-scope vs. follow-up.** Be explicit when a suggestion is a follow-up rather than a blocker.

## Multi-Model Review

When the environment supports launching sub-agents with different models, you may run the review in parallel across
2-3 distinct model families to get diverse perspectives. Only do this for substantial or risky changes, as it multiplies
the cost of the review.

1. Pick only models explicitly listed as available, choose the most capable model from each family, do not pick models
   labeled "mini", "fast", or "cheap", and do not pick the model already running the primary review.
2. Give each sub-agent the same prompt: the diff, the rules from this skill, and the severity format defined above.
3. Synthesize the results: deduplicate findings, elevate issues flagged by multiple models, and include unique findings
   that meet the confidence bar. If a sub-agent has not completed after 10 minutes, proceed with the results you have.
   Note which models contributed.

---

## Review Output Format

Use the following structure when presenting the final review, whether as a PR comment or as output to the user.

> 📝 **AI-generated content disclosure:** When posting review content to GitHub (PR reviews or comments) under a
> user's credentials — i.e. the account is **not** a dedicated bot account or app (e.g. `github-actions[bot]`,
> `copilot`) — you **MUST** include a concise, visible note (e.g. a `> [!NOTE]` alert) at the bottom of the content
> stating which agent generated it. Skip this only if the user explicitly asks you to omit it.

### Structure

```markdown
## PR Review

**Motivation**: <1-2 sentences on whether the PR is justified and the problem is real>

**Approach**: <1-2 sentences on whether the change takes the right approach>

**Summary**: <✅ LGTM / ⚠️ Needs Human Review / ⚠️ Needs Changes / ❌ Reject>. <2-3 sentence summary of the verdict and
key points. If "Needs Human Review", state which findings you are uncertain about and what a human should focus on.>

---

### Detailed Findings

#### ✅/⚠️/❌/💡 <Category Name> — <Brief description>

<Explanation with specifics. Reference code, line numbers, and example generated output where relevant.>

(Repeat for each finding category. Group related findings under a single heading.)

> [!NOTE]
> This review was generated by <agent name>.
```

### Guidelines

- Begin the review with `## PR Review`, followed by the `**Motivation**:`, `**Approach**:`, and `**Summary**:`
  fields in that order.
- Use emoji-prefixed category headers in **Detailed Findings**: ✅ for aspects verified to be correct, ⚠️ for warnings,
  ❌ for errors, and 💡 for minor suggestions.
- Include **cross-cutting analysis** when relevant (see "Sibling code paths" in Step 1).
- Assess **test quality** as its own finding when tests are part of the PR.
- Keep the review concise but thorough. Every claim should be backed by evidence from the code.

### Verdict Consistency Rules

1. **The verdict must reflect your most severe finding.** Any ⚠️ finding rules out "LGTM". Only use "LGTM" when all
   findings are ✅ or 💡 and you are confident the change is correct and complete.
2. **When uncertain, escalate to human review.** A false LGTM is far worse than an unnecessary escalation.
3. **Separate code correctness from approach completeness.** Correct code that fixes one instance of a problem but not
   its siblings, or that masks a root cause, is not an LGTM.
4. **Classify each ⚠️ and ❌ finding as merge-blocking or advisory.** If you would not be comfortable with any of them
   merging as-is, the verdict is "Needs Changes". If you are unsure, the verdict is "Needs Human Review".
5. **Devil's advocate check.** Before finalizing, re-read your ⚠️ findings and confirm the verdict reflects any
   unresolved concerns about approach, scope, or compatibility risk.
