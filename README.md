<p align="center">
  <img src="./lightinject-banner.svg" alt="LightInject banner" width="100%" />
</p>

# LightInject (WIP)

LightInject is a lightweight, straightforward dependency injection approach for Unity.

This is intentionally aimed at **smaller games** and simple composition roots, especially for injecting game systems.

## Scope

- Mostly hands-off after setup.
- Runtime injection is done through generated code.
- You still need to:
  1. `Record` dependencies into the context first.
  2. `Inject` targets later.

The expected flow is: **Record first, Inject second**.

## Important Characteristics

- Designed for small/medium complexity projects.
- Best fit for system wiring, not a full enterprise-style DI container.
- No runtime reflection-based object graph construction.
- Code generation is used to fulfill `[InjectAttributeSpecific]` field contracts.

## Current Limitations

- No circular dependency checks yet.
- No lazy injection support yet.
- No support yet for:
  - generic injectable targets,
  - nested target types,
  - array-type injections.
- Work in progress.

## Setup

1. Add/reference `com.utils.lightinject` in your Unity project.
2. Make sure your game assembly definition (`.asmdef`) references `Utils.LightInject`.

Generation depends on this reference. If your assembly does not reference `Utils.LightInject`, injector code will not be generated for it.

## Basic Usage

1. Mark fields with `[InjectAttributeSpecific]`.
2. Generate injectors (menu: `LightInject/Regenerate Static Injector`).
3. At runtime:
   - Record dependencies into `Injector` / context.
   - Inject into target instances after recording is complete.

## Example Flow

```csharp
var injector = new Runtime.Injector();

// 1) Record dependencies/services/systems
injector.Record(gridDisplaySystem);
injector.Record(gameCreator);

// 2) Inject targets
injector.TryInject(stencilSystem);
```

## Notes

- Generated files are auto-generated and should not be manually edited.
- This package is still evolving; APIs and generation details may change while limitations are being addressed.
