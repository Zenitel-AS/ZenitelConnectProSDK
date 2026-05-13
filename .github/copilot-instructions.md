# Copilot Instructions

## Scope

These instructions apply to the Zenitel Connect Pro SDK repository, especially the shipped `Zenitel.ConnectPro.SDK` assembly and the SDK-facing code under `src/IntegrationModule` and `src/SharedComponents`.

The SDK is already used by external consumers. Treat this repository as compatibility-sensitive production SDK code, not as an experimental refactor target.

## Core principle

Prefer stability, backward compatibility, and minimal localized fixes over cleanup, modernization, renaming, or architectural improvement.

Do not try to make the code “nicer” unless the requested task explicitly asks for that exact change and accepts the compatibility risk.

## Public API compatibility

Treat every public symbol compiled into the shipped SDK assembly as compatibility-sensitive by default.

Public API includes:

- public classes
- public interfaces
- public enums
- public DTOs and models
- public constructors
- public methods
- public properties
- public fields
- public callback/event members
- delegate-valued public properties
- public namespaces
- public member names
- public signatures
- public accessibility
- public serialized/JSON/WAMP payload shapes

Do not rename, remove, hide, reorder, or change public API unless the task is explicitly marked as a breaking change.

Adding new public API must also be intentional. Once shipped, new public members become future compatibility surface.

## Do not rename legacy or typoed public members

Do not rename public members just because they are misspelled, oddly named, legacy-named, or stylistically inconsistent.

This includes, but is not limited to:

- `OnBussyStateChange`
- `Recconect`
- `RecoonectAsync`
- `ResetRecconectionCounter`
- snake_case public model/WAMP properties such as `call_id`, `queue_dirno`, `from_dirno`, `to_dirno`, `device_ip`

If a name is public, assume external consumers may already depend on it.

## Event and callback compatibility

Preserve existing callback/event names, payload types, firing conditions, broad semantics, and ordering.

Do not convert public delegate-valued callback properties into C# `event` members unless explicitly requested as a breaking API change.

Do not narrow or “correct” a callback based on its name alone.

Preserve the current behavior of:

- `OnCallQueueListValueChange`
- `OnDeviceRetrievalStart`
- `OnDeviceRetrievalEnd`
- `OnConnectionChanged`
- active-call events
- queued-call events
- device retrieval events
- group and broadcasting events
- door/access-control events
- audio analytics events
- logging-related callbacks

If a callback looks redundant, broad, misspelled, misleading, or awkward, treat it as compatibility-sensitive until proven otherwise.

## Specific event rules

`OnCallQueueListValueChange` must continue firing during active-call changes. This is intentional, even though the name suggests queue-only behavior.

`OnDeviceRetrievalEnd` is legacy-named but must be treated as a one-shot initial device sync completion signal. It must not be changed into a periodic reconciliation completion event.

Do not reuse `OnDeviceRetrievalEnd` for reconnect refreshes, background polling, or periodic inventory reconciliation. If periodic reconciliation needs a signal, add a separate explicitly named signal only when requested.

`OnConnectionChanged(true)` must not be treated as a simple low-level socket-connected signal unless the current implementation proves that in the touched code path. Distinguish low-level WAMP connection state from higher-level SDK readiness/retrieval behavior.

## Lifecycle and ordering

Do not reorder `Core.Start()` initialization casually. The current implemented construction order is compatibility-sensitive.

Preserve the current startup, connection, reconnect, trace-subscription, token-renewal, and device-retrieval sequencing unless the task explicitly targets lifecycle behavior.

Do not change one-shot versus repeated event behavior without explicit approval.

Do not remove delays, guards, one-shot flags, or duplicate-looking lifecycle calls unless the task is explicitly about that behavior and the compatibility impact is called out.

## Shared state and object identity

Preserve the shared `Collections` instance wiring between handlers.

Collections exposed through the SDK are observable by consumers. Mutation behavior, replacement behavior, and object identity can therefore be compatibility-sensitive.

Preserve object identity where the current implementation updates existing instances in place, especially for:

- `Device`
- `Group`

Do not replace runtime-bound objects just because fresh inventory data arrived.

Do not reset runtime-only state during polling or reconciliation unless the task explicitly requires it.

Preserve runtime state such as:

- `Device.Gpio`
- GPIO listener/attachment state
- `Group.IsBusy`
- `Group.BroadcastedMessageName`
- active-call state
- queued-call state
- current call/video/popup-related state

Polling and inventory refreshes may update inventory/descriptive fields, but they must not destroy runtime call state, GPIO state, or identity-sensitive objects unless explicitly requested.

## DTO, model, JSON, and WAMP payload safety

Do not rename DTO/model/WAMP properties that may be serialized or consumed externally.

Preserve existing JSON property names, WAMP payload names, public property names, enum values, and payload shapes.

Do not convert snake_case payload properties to PascalCase unless explicitly requested as a breaking protocol/API change.

When comments or conversion methods show that a runtime property is intentionally excluded from DTOs, preserve that separation.

## Handler and collection behavior

Handlers exposed through `Core` are part of the observable SDK surface. Do not change their public methods, constructors, properties, or behavior casually.

Be especially careful with:

- `DeviceHandler`
- `CallHandler`
- `ConnectionHandler`
- `BroadcastingHandler`
- `AccessControlHandler`
- `CallForwardingHandler`
- `AudioEventHandler`
- `SystemMonitor`
- `Log`

Do not replace handler construction, shared dependency wiring, or collection ownership without explicit approval.

## Refactoring discipline

Do not perform broad or unsolicited refactors.

Do not modernize syntax, normalize naming, reorganize files, introduce new abstractions, or “clean up” legacy-looking behavior unless explicitly requested.

Prefer minimal, localized, backward-compatible changes.

If code looks awkward but is tied to event timing, lifecycle, polling, logging, WAMP payloads, REST payloads, reconciliation, or SDK callbacks, treat it as intentional until proven otherwise.

Do not collapse separate concepts just because they look similar. For example, runtime queued calls and configured queue definitions are different concepts.

## Documentation mismatch rule

When documentation, README examples, comments, and implementation disagree, do not silently change code to match documentation.

Report the mismatch and identify whether the requested task is to:

- update code,
- update documentation,
- preserve current behavior,
- or create a migration/breaking-change plan.

Prefer preserving implemented behavior unless the task explicitly asks to change it.

## Package and distribution safety

Do not change package metadata casually.

This includes:

- package ID
- assembly name
- target frameworks
- versioning
- NuGet packing settings
- license metadata
- strong-name/signing settings
- dependency versions
- dependency visibility
- `.nuspec` / `.csproj` package configuration

Package metadata is part of the public SDK distribution contract.

If package metadata differs between files, report the inconsistency instead of silently choosing one.

## Architecture discussion rules

When discussing architecture changes or refactor status, clearly distinguish:

- current implemented behavior,
- intended target behavior,
- incomplete intermediate behavior,
- proposed migration steps.

Do not describe an incomplete intermediate model as if it were final.

For architecture discussions, provide implementation guidance, migration steps, rationale, and usage examples. Do not produce a direct patch unless explicitly requested.

## Safety warning requirement

Before changing any of the following, explicitly call out compatibility risk:

- public API
- event/callback behavior
- event ordering
- object identity
- connection lifecycle
- device retrieval lifecycle
- polling behavior
- reconciliation logic
- DTO/model/WAMP/JSON payload shape
- package metadata
- target frameworks
- dependency versions

If the task does not explicitly authorize breaking behavior, keep the change non-breaking and minimal.

## Repository-specific preserved behaviors

Preserve firing `OnCallQueueListValueChange` during active-call changes.

Treat `OnDeviceRetrievalEnd` as one-shot initial device sync completion, not as periodic reconciliation completion.

Preserve `Group` object identity in `BroadcastingHandler` group polling. Do not recreate or replace existing `Group` instances during polling refreshes.

Preserve `Group.IsBusy` across polls because it represents runtime call state and must not be reset by polling.

Preserve runtime state separately from inventory state.

Preserve implemented behavior over comments or README examples when they disagree, unless the task explicitly asks to fix the mismatch.

## Work style

Be strict and factual.

Do not optimize for pleasing the requester.

If a requested change is risky, say so clearly.

If repository evidence is incomplete, say what is confirmed and what is not confirmed.

Do not invent behavior.

Do not make large changes when a small targeted fix is sufficient.