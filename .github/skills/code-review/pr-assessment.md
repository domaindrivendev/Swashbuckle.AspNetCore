# PR Assessment

Review-only criteria: these apply when assessing a pull request, not when authoring code. The coding rules themselves
live in [`AGENTS.md`](../../../AGENTS.md) and in the repository-specific checks in [`SKILL.md`](SKILL.md).

Use the criteria below to write the Motivation, Approach, and Summary fields of your review output.

Before reviewing individual lines of code, evaluate the PR as a whole. Consider whether the change is justified,
whether it takes the right approach, and whether it will be a net positive for the project and its users.

## Motivation & Justification

- **Every PR must articulate what problem it solves and why.** Ask for the rationale if none is provided. When the PR
  links to an issue or prior discussion that already establishes the motivation, that is sufficient.
- **Check alignment with the project's goals.** As described in [`CONTRIBUTING.md`](../../../CONTRIBUTING.md), the
  generated documentation should be driven by *actual* runtime behavior rather than *intended* behavior. Features that
  leverage built-in ASP.NET Core and serializer metadata (e.g. `[Authorize]`, `[Required]`, `JsonSerializerOptions`) are
  preferred over new custom attributes or options that only affect the documentation.
- **Challenge every addition with "Do we need this?"** New options, attributes, filters, and public types must justify
  their existence. If the scenario can already be handled by an existing extension point (such as a custom schema,
  operation, or document filter), prefer documenting that over adding new API surface.
- **UI-only requests belong upstream.** Bugs and features in swagger-ui or ReDoc themselves should be raised in those
  projects, not worked around here.

## Evidence & Data

- **Require a reproduction for bug fixes.** The PR should include a test that fails without the fix (see `AGENTS.md`).
- **Require measurable data for performance PRs.** Ask for BenchmarkDotNet results (see the `performance-benchmark`
  skill) comparing the baseline and the change. Never accept performance claims at face value.
- **Distinguish real wins from micro-benchmark noise.** Document generation typically runs once per document (and is
  often cached), so improvements to cold paths need a strong justification if they add complexity. Improvements should
  be demonstrated with realistic APIs and models, not only trivial inputs.

## Approach & Alternatives

- **Check whether the PR solves the right problem at the right layer.** For example, a schema problem caused by how a
  type is serialized should usually be fixed in the data contract resolver, not patched in a filter afterwards.
- **When a PR takes a fundamentally wrong approach, redirect early.** Don't iterate on details of a flawed design.
- **Ask "Why not just X?"** Prefer the simplest solution that works. The burden of proof is on the complex solution.

## Cost-Benefit & Complexity

- **Explicitly weigh whether the change is a net positive** for the typical user, not just for a narrow scenario.
- **Reject overengineering.** Unnecessary abstractions, indirection, and options for marginal gains are a cost.
- **Every addition creates a maintenance obligation.** Every new option multiplies the configurations that must keep
  working across serializers, OpenAPI versions, and target frameworks.

## Scope & Focus

- **Require large or mixed PRs to be split into focused changes.** Each PR should address one concern.
- **Defer tangential improvements to follow-up PRs**, including unrequested refactoring, dependency updates, and
  formatting changes to unrelated code. Maintainers may waive this requirement.

## Risk & Compatibility

- **Flag breaking changes.** This includes binary breaking changes to the public API and behavioral changes to the
  generated documents (schema IDs, operation IDs, `$ref` structure, nullability, required properties, enum
  representation, etc.) that affect downstream client code generation. Behavioral changes that are not clear bug fixes
  should normally be opt-in.
- **Assess regression risk proportional to the blast radius.** Changes to `SchemaGenerator`, `SwaggerGenerator`, or
  the data contract resolvers affect every user of the library and need proportionally more validation.

## Codebase Fit & History

- **Ensure new code matches existing patterns and conventions**, including sync/async and System.Text.Json/Newtonsoft
  parity where applicable.
- **Check whether a similar approach has been tried and rejected before** by searching closed PRs and issues. If a
  previous attempt didn't work, require a clear explanation of what is different this time.
