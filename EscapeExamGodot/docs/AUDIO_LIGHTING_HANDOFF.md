# Chapter 01 — Audio + Lighting Handoff (MODULE B)

Branch: `audio/chapter01-lookdev-ambience`  
Commit intent: `audio: polish chapter01 dark lookdev + ambience duck/sting`  
Scope: lookdev + ambience beds/duck/sting only. No teacher AI, map topology, or art Visual.

## What changed

### Lookdev (`scenes/chapters/chapter_01.tscn` — Lighting / WorldEnvironment only)

| Item | Value |
|------|--------|
| Ambient | Color `(0.16, 0.18, 0.22)`, energy `0.045` (cold grey-blue; floor-readable, not pitch black) |
| Warm window hero | `Lighting/Fixture0` Color `(1.0, 0.72, 0.45)`, energy `2.0`, **shadow on** |
| Cold exit heroes | `Fixture5` + `Fixture7` Color `(0.45, 0.7, 1.0)`, shadow on |
| Fill tubes | Fixture1/2/3/4/8 `shadow_enabled=false` |
| Shadow Omni count (measured) | **4** — Fixture0,5,6,7 (≤5 budget). Fixture3 shadow dropped (fill-like green) |
| GI | No VoxelGI / SDFGI on sealed exam room |
| Directional | Weak cool fill (`Sun` energy `0.12`, no shadow) |
| Tube flicker | Not added (optional; avoid new script outside MODULE B paths) |

### Audio buses (`default_bus_layout.tres`)

Exact names under Master:

```
Master
├── Music
├── Ambient   ← rain_bed + tube_hum
├── SFX
└── Sting     ← catch_sting
```

`ambience.gd` runtime `_ensure_buses()` also creates Music/Ambient/SFX/Sting if the layout is missing.

### Ambience (`scripts/systems/ambience.gd`)

- Beds: `rain_bed.wav` + `tube_hum.wav` on **Ambient** bus; imports already `edit/loop_mode=1`.
- Catch sting: `catch_sting.wav` on **Sting**; ducks Ambient to **-22 dB** (~0.08s), restores after ~0.85s.
- **Broadcast stop → true quiet**: `set_loop_quiet(true)` fades beds to -40 dB then **stops** players (not merely -28/-32 duck). Resume restarts loops.
- **Kill on stop/reset**: `reset_audio_state()` kills duck/bed Tweens, increments `_sting_gen` to invalidate pending restore timers, stops sting, resets Ambient bus dB.
- Beds never call acoustics / AI noise emitters.

### Broadcast hooks (`scripts/props/broadcast.gd`)

Mute/duck only: `stop` toggles still call `Ambience.set_loop_quiet(true/false)`. No AI/map changes.

## How to verify (editor / play)

1. **Buses** — Audio panel: Music, Ambient, SFX, Sting all present and send → Master.
2. **Catch sting** — Trigger a catch (or call `Ambience.play_catch_sting()` from debugger): Ambient duck audible, sting on Sting bus, beds restore ~0.85s later. Interrupt with chapter reset → no late duck restore (gen invalidated).
3. **Stop broadcast** — Operate broadcast `stop`: LoopAudio stops; rain/hum fade then go silent (players stopped). Toggle resume → beds restart at normal dB.
4. **Walk dark path** — Play camera height: floor/desk legs readable; window side warm, exit corridor cold; no SDFGI/VoxelGI stack.
5. **Shadow budget** — Inspector: only Fixture0/5/6/7 Omni `shadow_enabled=true` (count **4**).

Static audit only this pass — no playtest screenshots claimed.

## Remaining gaps for @游戏开发 merge → `feat/chapter01-module-pass`

- Optional single-tube Tween flicker still absent (easy follow-up; keep frequency low).
- No in-engine playtest / screenshot evidence from this branch (headless/static audit only).
- Confirm Game.caught → `Ambience.play_catch_sting()` still wired on feat branch after other modules merge.
- Art Visual / teacher AI / map topology owned by other modules — do not merge those from this worktree.
- If feat already diverged on `chapter_01.tscn` Lighting, re-apply Fixture3/5/7 deltas carefully (do not wholesale rewrite map).

## Files touched (allowed only)

- `EscapeExamGodot/default_bus_layout.tres`
- `EscapeExamGodot/scripts/systems/ambience.gd`
- `EscapeExamGodot/scripts/props/broadcast.gd`
- `EscapeExamGodot/scenes/chapters/chapter_01.tscn` (Lighting Omni colors/shadows only)
- `EscapeExamGodot/docs/AUDIO_LIGHTING_HANDOFF.md`
