---
title: MinecraftThroughTime code map
scope: MinecraftThroughTime
tags:
- code-map
- mermaid
- structure
- auto
summary: 'Code map: MinecraftThroughTime — 7 components, 11 call-dependencies (top);
  core: CDL, Program, Launcher_Profiles, Update'
related: []
web: []
---

Code map: MinecraftThroughTime — 7 components, 11 call-dependencies (top); core: CDL, Program, Launcher_Profiles, Update

_Auto-generated (deterministic, no AI) from the symbol index._

## Core components (PageRank — most depended-upon)
- `CDL` — 0.279
- `Program` — 0.158
- `Launcher_Profiles` — 0.143
- `Update` — 0.121
- `Bake` — 0.103
- `Launcher` — 0.102
- `Make` — 0.094

## Call dependencies (who calls whom)
```mermaid
graph LR
  n_Bake["Bake"]
  n_CDL["CDL"]
  n_Launcher["Launcher"]
  n_Launcher_Profiles["Launcher_Profiles"]
  n_Make["Make"]
  n_Program["Program"]
  n_Update["Update"]
  n_Program -->|8| n_CDL
  n_Make -->|6| n_CDL
  n_Update -->|4| n_CDL
  n_Program -->|4| n_Update
  n_Make --> n_Program
  n_Launcher --> n_Launcher_Profiles
  n_Program --> n_Bake
  n_Launcher --> n_Program
  n_Update --> n_Program
  n_Program --> n_Make
  n_Update --> n_Launcher
```

## Largest components (by member count)
- `CDL` — 12 members
- `Program` — 11 members
- `Update` — 8 members
- `Make` — 4 members
- `Launcher` — 3 members
- `FileDownloads` — 3 members
- `Launcher_Profiles` — 2 members
- `Bake` — 2 members
- `version_manifest` — 2 members
- `Vmv2` — 2 members
- `MTTProfile` — 2 members
- `ClientDownloads` — 1 members
- `ServerDownloads` — 1 members
- `MTTProfileEntry` — 1 members
