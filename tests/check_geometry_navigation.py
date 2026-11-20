"""Source-backed preflight for GeomeTry navigation + BILATERAL LEGACY integration.

Only tests static assets: this is not a Unity Editor or physical Android test.
"""
from pathlib import Path
import json
import re

root = Path(__file__).resolve().parents[1]
upstream = root / "ThirdParty/GeomeTryOriginal/Assets"
expected = [
    "Main Menu", "Level", "Level Editor", "Customisation", "Level Selection",
    "Success", "Settings", "Stats", "Profile", "Credits",
]
build = (root / "ProjectSettings/EditorBuildSettings.asset").read_text(encoding="utf-8")
records = re.findall(
    r"  - enabled: ([01])\r?\n    path: ([^\r\n]+)\r?\n    guid: ([a-f0-9]{32})",
    build,
)
assert records, "Unity BuildSettings asset has no scenes"
active = [(p, g) for enabled, p, g in records if enabled == "1"]
expected_paths = [
    f"Assets/GeomeTryOriginal/Assets/Scenes/{n}.unity" for n in expected
]
assert [p for p, _ in active] == expected_paths, (
    f"Build Scene List must contain the 10 original scenes in order: {active}"
)
assert ("0", "Assets/Scenes/SampleScene.unity",
        "2cda990e2423bbf4892e6590ba056729") in records, (
    "BIOFIT original SampleScene must stay present but disabled"
)
for (p, g), n in zip(active, expected):
    scene = upstream / f"Scenes/{n}.unity"
    meta = upstream / f"Scenes/{n}.unity.meta"
    assert scene.is_file(), f"Missing upstream scene: {n}"
    assert f"guid: {g}" in meta.read_text(encoding="utf-8"), (
        f"Scene GUID from upstream changed: {n}"
    )

manager = (upstream / "Scripts/GameManager.cs").read_text(encoding="utf-8")
ui = (upstream / "Scripts/UIManager.cs").read_text(encoding="utf-8")
menu = (upstream / "Scenes/Main Menu.unity").read_text(encoding="utf-8")
assert "SceneExists(sceneName)" in manager
assert "SceneManager.LoadSceneAsync(sceneName)" in manager
assert "OnClickSwitchSceneButton(string sceneName)" in ui
assert "StartCoroutine(_gameManager.LoadScene(sceneName))" in ui
assert 'StartCoroutine(LoadScene("Level"))' in manager
transitions = re.findall(
    r"m_MethodName: OnClickSwitchSceneButton[\s\S]{0,650}?"
    r"m_StringArgument: ([^\r\n]*)",
    menu,
)
assert "Level Selection" in transitions, (
    "Main Menu Play button must target Level Selection"
)
assert set(transitions) <= set(expected), (
    "Original Main Menu refers to scenes not in build settings: "
    + repr(set(transitions) - set(expected))
)

maps = upstream / "Resources/maps/BASE"
music = set()
types = set()
counts = {}
for name in (
    "StereoMadness", "BackOnTrack", "Polargeist", "JumpyNightmare", "WaveVibe"
):
    info = json.loads((maps / f"{name}.json").read_text(encoding="utf-8"))
    assert info.get("name") and info.get("music") and info.get("map"), name
    music.add(info["music"])
    types.update(item["type"] for item in info["map"])
    counts[name] = len(info["map"])
for song in music:
    assert (upstream / f"Resources/Songs/Musics/{song}.mp3").is_file(), (
        f"Original level music missing: {song}"
    )
prefabs = (upstream / "Scripts/Prefab.cs").read_text(encoding="utf-8")
for kind in types - {"ground"}:
    assert re.search(r'["]' + re.escape(kind) + r'["]\s*,', prefabs), (
        f"Map object type has no matching original prefab: {kind}"
    )
for name in ("CubePrefab", "ShipPrefab", "WavePrefab"):
    assert (upstream / f"Resources/Prefabs/Player/{name}.prefab").is_file()
for name in ("GroundPrefab", "CeilPrefab"):
    assert (upstream / f"Resources/Prefabs/Decors/{name}.prefab").is_file()
assert (upstream / "Resources/Prefabs/UI/LevelButtonPrefab.prefab").is_file()

editor = (root / "Assets/BIOFIT/GeomeTry/Editor/BiofitGeomeTryOriginalBuild.cs").read_text(encoding="utf-8")
assert "VerifyNavigationAndResources();" in editor
assert 'Resources.Load<TextAsset>("maps/BASE/" + levelName)' in editor
bridge = (root / "Assets/BIOFIT/GeomeTry/BiofitGeomeTryLegacyBridge.cs").read_text(encoding="utf-8")
assert "VerifyMenuNavigationInEditor();" in bridge
assert "BilateralJumpCandidate += OnBilateralLegacyCandidate" in bridge
assert "cube.onClick();" in bridge
assert "monitor.PhaseFastPairs = false;" in bridge
assert "monitor.LiftCycleMode = false;" in bridge
assert "TryConsumeOrbPulse()" in bridge
compat = (root / "tools/apply_geometry_compat.ps1").read_text(encoding="utf-8")
assert "TryConsumeOrbPulse()" in compat, "BLE JumpOrb compatibility patch missing"
assert 'Resources.LoadAll<TextAsset>("maps/BASE")' in compat, (
    "Original menu resource-path normalization missing"
)
assert 'Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0)' in (
    upstream / "Scripts/JumpOrb.cs").read_text(encoding="utf-8"), (
    "Pristine upstream JumpOrb input must remain unchanged"
)

print("PASS: original scene list/menu buttons/scene GUIDs, "
      "5 full source maps, prefab/music resources and legacy BLE cube bridge")
print("Map object counts:", counts)
