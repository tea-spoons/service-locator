# Change plan

> Draft. This file tracks what changed on the way to this repo and what I plan to change next. Edit freely.

## Origin

Originally developed at Bigpoint. Published here with Bigpoint's permission for research, education and other noncommercial use. Copyright (c) 2026 Bigpoint; see [LICENSE.md](LICENSE.md).

## Changes made before publishing

- Namespaces are now `TeaSpoons.*` and the package id is `com.tea-spoons.service-locator` (assemblies renamed to match).
- Internal build, registry and tracker references were removed; the repo uses GitHub Actions (`CI` and `Release`) built on `unity-ci-kit`.
- Added `LICENSE.md` (PolyForm Noncommercial 1.0.0), an install section in the README, and package metadata (author, license and documentation URLs).

## Planned changes

- [x] Tag and publish `v0.5.7` with the Release workflow.
- [ ] Run this package's tests in CI with `unity-ci-kit` (needs a small test-project helper in the kit).
<!-- review-items:start -->
- [ ] **P0** Replace the reflection call in `DoLateInitialization` with a direct delegate call inside a per-handler `try/catch` that keeps the original exception, and add a test with a throwing handler.
- [ ] **P1** Add a non-throwing `TryGet<T>` (today there is only `Get<T>`).
- [ ] **P1** Extend `ServicesTestMode`: a scope you can dispose (or a `Stop`) that restores the previous bindings, and an `Override<T>` for one service, so a test does not have to wipe and rebuild every binding.
- [ ] **P1** Declares `unity: 2022.3`, but only Unity 6000.3.8f1 was tested. Add a Unity version matrix to CI once package tests run there (see the `unity-ci-kit` plan), or raise the minimum.
- [ ] **P2** State in the README that the API is main-thread only, and when to choose a DI container such as VContainer instead.
- [ ] **P2** Add a `CHANGELOG.md`. Unity's package layout lists one next to `README.md`, and the `unity-ci-kit` validator warns without it.
<!-- review-items:end -->

<!-- review:start -->
## Review (September 2026)

Reviewed as a senior Unity engineer would: I read the code and compared the package with similar open-source projects (September 2026). Those projects are listed for ideas only. Nothing was copied from them, and their licenses are noted in case code is ever reused. Priorities: **P0** correctness bug or broken metadata, **P1** should be done soon, **P2** nice to have.

### Compared with

| Project | License | Worth noting |
|---|---|---|
| [hadashiA/VContainer](https://github.com/hadashiA/VContainer) | MIT | Dependency injection instead of a locator: constructor, method and property injection, nested lifetime scopes, an immutable container (thread safe), no allocation on resolve, an optional source generator and entry points on the PlayerLoop. |

This package is a locator, not a DI container, and should stay small. The comparison is there to state where each one fits.

### Findings from reading the code

- **[Bug]** `Services.DoLateInitialization` invokes handlers with `response.Method.Invoke(response.Target, null)` and logs `e.InnerException`. For an exception that is not a `TargetInvocationException` (for example one thrown by `Invoke` itself), `InnerException` is `null` and `Debug.LogException(null)` throws. Calling the delegate directly avoids both problems.
- **[Scope]** The only API is static, with `Get<T>()` and no `TryGet`. There are no scopes or child locators. For tests there is the editor-only `ServicesTestMode.Start()`, which deletes all bindings and lets you bind again. There is no matching `Stop`, and no way to override one service and put it back.
- **[Threading]** The binding dictionaries are plain `Dictionary` instances. It is a main-thread API and does not say so.
- **[Good]** Static state is reset at `SubsystemRegistration`, so it is safe with domain reload disabled.
<!-- review:end -->

## Notes and ideas

_Add your own here._
