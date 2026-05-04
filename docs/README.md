# Incremental Source Generator — Events → R3 Observable

## Overview

This project's source generator automatically generates the boilerplate needed to convert public events defined by a target type into extension methods returning `global::R3.Observable<T>`. Generation is triggered by annotating any `static partial class` with `R3Events.R3EventAttribute` in the consuming project.

C# 8 or later is required on the consumer side (generated code contains nullable reference type syntax). C# 11 or later also enables the generic attribute `R3Event<T>`.

## Goals

- Reduce event-subscription boilerplate.
- Auto-generate a type-safe Observable API.

This directory contains the specification and usage examples for the generator.

## Files

- `docs/spec.md` — Detailed specification (including diagnostics R3I001 and the code-fix specification)
- `docs/examples.md` — Usage examples and generated code samples
