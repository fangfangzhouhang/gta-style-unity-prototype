#!/usr/bin/env python3
"""Offline static check: count OmniLight3D under Lighting/*/Light with shadow_enabled=true."""
from __future__ import annotations
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SCENE = ROOT / "scenes" / "chapters" / "chapter_01.tscn"
text = SCENE.read_text(encoding="utf-8")
shadow_on: list[str] = []
for m in re.finditer(
    r'\[node name="Light" type="OmniLight3D" parent="Lighting/([^\"]+)"\](.*?)(?=\n\[node |\Z)',
    text,
    re.S,
):
    parent, block = m.group(1), m.group(2)
    if "shadow_enabled = true" in block:
        shadow_on.append(parent)
n = len(shadow_on)
print(f"SHADOW_OMNI_COUNT {n} fixtures={','.join(shadow_on)}")
if n > 5:
    print(f"FAIL shadow Omni count {n} exceeds budget ≤5", file=sys.stderr)
    sys.exit(1)
print(f"PASS shadow Omni budget ≤5 (measured={n}; expect 4)")
sys.exit(0)
