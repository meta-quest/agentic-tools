# metavr Documentation Search

Documentation commands let you search and retrieve Meta Quest developer documentation
directly from the command line. This is useful for quickly looking up APIs, guides,
and best practices without leaving your development environment.

## Commands Overview

| Command | Description |
|---|---|
| `metavr docs search <query>` | Search Meta Quest documentation |
| `metavr docs fetch <url>` | Fetch a specific documentation page |
| `metavr docs api-search <query>` | Search API references using BM25 ranking |
| `metavr docs api-details <name>` | Get full details for an API entry |
| `metavr docs api-stats` | Show statistics about loaded API indexes |

## Verify-first Workflow

For coding agents and AI-assisted developer tools, the safest default is:

1. Search first with `metavr docs search`
2. Fetch the exact page with `metavr docs fetch`
3. Only then answer or code against the result

This is especially important for:

- current SDK APIs
- build and deploy steps
- new Quest behavior
- policy and store questions
- SDK features that may have changed recently

When passing results to another tool or agent, include the canonical Meta docs URL
so the source can be re-fetched and verified later.

## metavr docs search

Search across the full Meta Quest developer documentation library.

```bash
# Search by topic
metavr docs search "hand tracking"

# Search for API references
metavr docs search "passthrough API"

# Search for platform-specific guides
metavr docs search "Unity spatial anchors"
```

Results include page titles, URLs, and brief descriptions. Use the returned URLs
with `metavr docs fetch` to read the full content.

For agent workflows, do not rely on snippets alone when the answer needs exact
steps, code, policy wording, or recency-sensitive behavior.

### Example

```bash
$ metavr docs search "scene understanding"

Results:
1. Scene Understanding Overview
   https://developers.meta.com/horizon/documentation/unity/unity-scene-overview
2. Scene API Reference
   https://developers.meta.com/horizon/documentation/native/scene-api
3. Spatial Data in Unreal Engine
   https://developers.meta.com/horizon/documentation/unreal/unreal-spatial-data
```

## metavr docs fetch

Retrieve the full content of a specific documentation page by URL.

```bash
# Fetch by full URL
metavr docs fetch https://developers.meta.com/horizon/documentation/unity/unity-scene-overview

# Fetch by short path (automatically expanded to full URL)
metavr docs fetch documentation/unity/unity-scene-overview.md
```

The page content is returned as structured markdown, making it easy to read in the
terminal or pipe to other tools.

### Fetching from Search Results

A common workflow is to search first, then fetch the most relevant result:

```bash
# 1. Search for the topic
metavr docs search "controller input"

# 2. Fetch the page you need
metavr docs fetch https://developers.meta.com/horizon/documentation/unity/unity-controller-input
```

For agent workflows, prefer quoting the fetched page rather than paraphrasing from
search-result snippets alone.

If your MCP framework supports structured outputs, return canonical URLs,
last-modified markers, and suggested follow-up fetch inputs so the model does not
have to guess the next call arguments.

## Documentation Categories

Meta Quest developer documentation is organized by platform and SDK. Each category
has its own documentation index.

### Unity

Guides and API references for building Quest apps with the Unity engine.

```bash
metavr docs search "Unity setup guide"
metavr docs search "Unity hand tracking"
metavr docs search "Unity passthrough"
metavr docs search "Unity interaction SDK"
```

Topics include: project setup, rendering, input handling, hand tracking, passthrough,
spatial anchors, scene understanding, interaction SDK, and performance optimization.

### Unreal Engine

Guides and API references for building Quest apps with Unreal Engine.

```bash
metavr docs search "Unreal Engine Quest setup"
metavr docs search "Unreal hand tracking"
metavr docs search "Unreal passthrough layer"
```

Topics include: project configuration, input mapping, hand tracking, passthrough,
scene capture, spatial anchors, and performance profiling.

### Meta Spatial SDK

Guides for the Meta Spatial SDK (Kotlin/Android-based development).

```bash
metavr docs search "Spatial SDK getting started"
metavr docs search "Spatial SDK spatial components"
```

Topics include: spatial panels, spatial components, scene management, and
object-based interactions.

### Android (Native Java/Kotlin)

Documentation for building Quest apps as standard Android applications.

```bash
metavr docs search "Android Quest development"
metavr docs search "Android VR activity"
```

Topics include: Android project setup, VR activity lifecycle, manifest configuration,
and platform-specific APIs.

### Native C/C++

Low-level native development documentation using OpenXR and the Meta OpenXR SDK.

```bash
metavr docs search "OpenXR native development"
metavr docs search "native rendering Quest"
metavr docs search "Vulkan Quest"
```

Topics include: OpenXR extensions, Vulkan rendering, native input, native spatial
anchors, and performance APIs.

### Web (WebXR)

Documentation for building VR experiences that run in the Meta Quest Browser.

```bash
metavr docs search "WebXR Quest"
metavr docs search "web VR development"
```

Topics include: WebXR device API, immersive sessions, controller input in WebXR,
and progressive web app support.

## Tips for Effective Searches

### Be Specific

Narrow your search to get more relevant results:

```bash
# Too broad
metavr docs search "tracking"

# Better — specifies the type of tracking
metavr docs search "hand tracking setup Unity"
```

### Include the Platform

If you are working with a specific engine or SDK, include it in the query:

```bash
metavr docs search "Unreal passthrough camera"
metavr docs search "Unity scene anchors"
metavr docs search "native OpenXR hand mesh"
```

### Use Feature Names

Search for the official feature name when possible:

```bash
metavr docs search "Shared Spatial Anchors"
metavr docs search "Scene Model"
metavr docs search "Interaction SDK"
metavr docs search "Passthrough API"
```

### Search for Error Messages

If you encounter an error, search for keywords from the error message:

```bash
metavr docs search "OVRManager initialization failed"
metavr docs search "XR_ERROR_RUNTIME_FAILURE"
```

## API Reference Search

metavr provides a dedicated API reference search using BM25 ranking, which is faster
and more precise for looking up specific classes, methods, or types.

### metavr docs api-search

Search API references by name, description, or signature:

```bash
# Search Unity API references
metavr docs api-search "OVRInput" --platform unity

# Search Unreal Engine 4 API references
metavr docs api-search "hand tracking" --platform unreal_ue4

# Search Unreal Engine 5 API references
metavr docs api-search "passthrough" --platform unreal_ue5

# Limit results
metavr docs api-search "spatial anchor" --platform unity -n 10
```

### metavr docs api-details

Get full documentation for a specific API entry:

```bash
# Get details for a Unity class
metavr docs api-details "OVRInput" --platform unity

# Get details for an Unreal class
metavr docs api-details "UOculusXRHandTrackingComponent" --platform unreal_ue5
```

Returns the full documentation text plus any child members (methods, properties, etc.).

### metavr docs api-stats

Show statistics about available API reference indexes:

```bash
metavr docs api-stats
```

Returns counts of entries by type (class, method, property, etc.) for each platform.

## Using Documentation in Development

### Informing Architecture Decisions

Before starting a new feature, search for relevant guides:

```bash
# Research available approaches
metavr docs search "multiplayer Quest"
metavr docs fetch <url_from_results>
```

### Checking API Availability

Verify that an API is available on your target platform and OS version:

```bash
metavr docs search "body tracking API requirements"
```

### Troubleshooting Build Issues

Search for platform-specific setup and configuration guides:

```bash
metavr docs search "Unity project settings Quest 3"
metavr docs search "Unreal Android signing"
metavr docs search "Gradle configuration Quest"
```

### Staying Up to Date

Documentation is regularly updated with new features and API changes. Use
`metavr docs search` and `metavr docs fetch` to always access the latest version
rather than relying on cached or outdated information.
