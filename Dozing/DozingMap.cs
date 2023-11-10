using Dozing.CustomMonoBehaviors;
using Dozing.Properties;
using Dozing.Utils;
using Il2CppAssets.Scripts.Data.MapSets;
using Il2CppAssets.Scripts.Models.Map;
using Il2CppAssets.Scripts.Models.Map.Gizmos;
using Il2CppAssets.Scripts.Models.Map.Spawners;
using Il2CppAssets.Scripts.Models.Map.Triggers;
using Il2CppAssets.Scripts.Models.Profile;
using Il2CppAssets.Scripts.Simulation.SMath;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Unity.Bridge;
using Il2CppAssets.Scripts.Unity.Map;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.Removables;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Collections.Generic;
using Il2CppTMPro;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using CashSource = Il2CppAssets.Scripts.Simulation.Simulation.CashSource;
using CashType = Il2CppAssets.Scripts.Simulation.Simulation.CashType;
using NKVector2 = Il2CppAssets.Scripts.Simulation.SMath.Vector2;
using Resources = Dozing.Properties.Resources;
using Scene = UnityEngine.SceneManagement.Scene;

namespace Dozing {
    internal static class DozingMap {
        private static readonly AssetBundle sceneBundle = Resources.GetAssetBundle("dozingscene");
        private static readonly AssetBundle texturesBundle = Resources.GetAssetBundle("dozingtextures");

        public const string MapId = "Dozing";
        public const string MapIconId = "DozingMapSelect";
        private const string TopPathName = "Top";
        private const string BottomPathName = "Bottom";
        public const float MapWideBloonSpeed = 1;

        public static string ScenePath { get; } = sceneBundle.GetAllScenePaths()[0];

        public static MapDetails Details => new() {
            id = MapId,
            difficulty = MapDifficulty.Expert,
            coopMapDivisionType = CoopDivision.FREE_FOR_ALL,
            unlockDifficulty = MapDifficulty.Beginner,
            mapMusic = "",
            mapSprite = new() { guidRef = $"Ui[{MapIconId}]" },
            theme = MapTheme.DarkGrass
        };

        /*public static MapModel Model => new(MapId,
            AreaModels,
            BlockerModels,
            CoopAreaLayoutModels,
            PathModels,
            RemoveableModels,
            MapGizmoModels,
            (int)MapDifficulty.Expert,
            PathSpawnerModel,
            MapEventModels,
            MapWideBloonSpeed);*/

        #region Areas

        public static Il2CppReferenceArray<AreaModel> AreaModels => new(6) {
            [0] = AreaWhole,
            [1] = AreaTopTrack,
            [2] = AreaBottomTrack,
            [3] = AreaLampost1,
            [4] = AreaLampost2,
            [5] = AreaLampost3
        };

        private static AreaModel AreaWhole => new("Whole",
            new Polygon(new List<NKVector2>().Add(new(-330, -330),
                                                  new(-330, 330),
                                                  new(330, 330),
                                                  new(330, -330))),
            new(0), 0, AreaType.land);


        private static AreaModel AreaTopTrack => new("TopTrack",
            new Polygon(new List<NKVector2>().Add(new(-150, -51),
                                                  new(-150, -25),
                                                  new(150, -25),
                                                  new(150, -51))),
            new(0), 0, AreaType.track);

        private static AreaModel AreaBottomTrack => new("BottomTrack",
            new Polygon(new List<NKVector2>().Add(new(-150, 27),
                                                  new(-150, 53),
                                                  new(150, 53),
                                                  new(150, 27))),
            new(0), 0, AreaType.track);

        private static AreaModel AreaLampost1 => new("Lampost 1",
            new Polygon(new List<NKVector2>().Add(new(-5, 0),
                                                  new(-3, 4),
                                                  new(3, 4),
                                                  new(5, 0),
                                                  new(3, -4),
                                                  new(-3, -4))),
            new(0), 0, AreaType.unplaceable);

        private static AreaModel AreaLampost2 => new("Lampost 2",
            new Polygon(new List<NKVector2>().Add(new(-125, 0),
                                                  new(-123, 4),
                                                  new(-117, 4),
                                                  new(-115, 0),
                                                  new(-117, -4),
                                                  new(-123, -4))),
            new(0), 0, AreaType.unplaceable);

        private static AreaModel AreaLampost3 => new("Lampost 3",
            new Polygon(new List<NKVector2>().Add(new(115, 0),
                                                  new(117, 4),
                                                  new(123, 4),
                                                  new(125, 0),
                                                  new(123, -4),
                                                  new(117, -4))),
            new(0), 0, AreaType.unplaceable);

        #endregion

        #region Blockers

        public static Il2CppReferenceArray<BlockerModel> BlockerModels => new(0);

        #endregion

        #region Coop Areas

        public static Il2CppReferenceArray<CoopAreaLayoutModel> CoopAreaLayoutModels => new(1) {
            [0] = CoopAreaLayoutFFA
        };

        private static CoopAreaLayoutModel CoopAreaLayoutFFA => new(new Il2CppReferenceArray<CoopAreaModel>(1) {
            [0] = CoopAreaFFA
        }, AreaLayoutType.FREE_FOR_ALL);

        private static CoopAreaModel CoopAreaFFA => new(0,
            new Polygon(new List<NKVector2>().Add(new(-330, -300),
                                                  new(-330, 330),
                                                  new(330, 330),
                                                  new(330, -330))),
            new NKVector2(0, 0));

        #endregion

        #region Paths

        public static Il2CppReferenceArray<PathModel> PathModels => new(2) {
            [0] = TopPath,
            [1] = BottomPath
        };

        private static PathModel TopPath => new(TopPathName,
            new Il2CppReferenceArray<PointInfo>(2) {
                [0] = new() { point = new(-330, -40), bloonScale = 1, moabScale = 1, bloonSpeedMultiplier = 1 },
                [1] = new() { point = new(330, -40), bloonScale = 1, moabScale = 1, bloonSpeedMultiplier = 1 }
            }, true, false, new(-1000, -1000, -1000), new(-1000, -1000, -1000), null, null);

        private static PathModel BottomPath => new(BottomPathName,
            new Il2CppReferenceArray<PointInfo>(2) {
                [0] = new() { point = new(330, 40), bloonScale = 1, moabScale = 1, bloonSpeedMultiplier = 1 },
                [1] = new() { point = new(-330, 40), bloonScale = 1, moabScale = 1, bloonSpeedMultiplier = 1 }
            }, true, false, new(-1000, -1000, -1000), new(-1000, -1000, -1000), null, null);

        #endregion

        #region Removeables

        public static Il2CppReferenceArray<RemoveableModel> RemoveableModels => new(0);

        #endregion

        #region Gizmos

        public static Il2CppReferenceArray<MapGizmoModel> MapGizmoModels => new(0);

        #endregion

        #region Path Spawners

        public static PathSpawnerModel PathSpawnerModel => new("", SplitterModel, SplitterModel);

        private static SplitterModel SplitterModel => new MoabOnlySplitterModel("",
            new Il2CppStringArray(2) {
                [0] = TopPathName,
                [1] = BottomPathName
            }, new Il2CppStructArray<bool>(2) {
                [0] = false,
                [1] = true
            });

        #endregion

        #region Map Events

        public static Il2CppReferenceArray<MapEventModel> MapEventModels => new(0);

        #endregion

        #region Lampost Behaviors

        public const int lampRange = 55;
        private const int initialCost = 1000;
        private static int currentCost = initialCost;
        private const int costMult = 2;
        private static bool[] isLampOn = { false, false, false };
        private static readonly Lamp[] lamps = new Lamp[3];
        private static readonly NKVector2[] lampPos = new NKVector2[3] {
            new NKVector2(0, 0),
            new NKVector2(-120, 0),
            new NKVector2(120, 0)
        };

        public static void ModifyScene(Scene scene) {
            GameObject root = scene.GetRootGameObjects()[0];
            root.AddComponent<Map>();
            BoxCollider[] lampColliders = root.GetComponentsInChildren<BoxCollider>();
            for(int i = 0; i < 3; i++) {
                Lamp lamp = lampColliders[i].gameObject.AddComponent<Lamp>();
                lamp.index = i;
                lamps[i] = lamp;

                lamp.onClicked = OnLampClicked;
            }
        }

        private static void OnLampClicked(Lamp lamp) {
            Button confirmBtn = RemovablePanel.instance.confirmBtn;
            Button cancelBtn = RemovablePanel.instance.cancelBtn;
            Button bgBtn = RemovablePanel.instance.background.GetComponent<Button>();
            TextMeshProUGUI costTxt = RemovablePanel.instance.costTxt;

            UnityAction onConfirm = null, onCancel = null;
            onConfirm = new System.Action(() => {
                if (InGame.Bridge.GetAvailableCash() < currentCost || isLampOn[lamp.index])
                    return;

                InGame.Bridge.Simulation.RemoveCash(currentCost, CashType.Normal, InGame.Bridge.GetInputId(), CashSource.MapInteractableUsed);
                currentCost *= costMult;

                TurnOnLamp(lamp);

                isLampOn[lamp.index] = true;

                RemovablePanel.instance.ShowPanel(false);
                confirmBtn.onClick.RemoveListener(onConfirm);
                cancelBtn.onClick.RemoveListener(onCancel);
                bgBtn.onClick.RemoveListener(onCancel);
            });
            onCancel = new System.Action(() => {
                RemovablePanel.instance.ShowPanel(false);
                confirmBtn.onClick.RemoveListener(onConfirm);
                cancelBtn.onClick.RemoveListener(onCancel);
                bgBtn.onClick.RemoveListener(onCancel);
            });
            confirmBtn.onClick.AddListener(onConfirm);
            cancelBtn.onClick.AddListener(onCancel);
            bgBtn.onClick.AddListener(onCancel);

            costTxt.text = $"Turn on for ${currentCost}";
            confirmBtn.interactable = InGame.Bridge.GetAvailableCash() >= currentCost;

            RemovablePanel.instance.ShowPanel(true);
        }

        #endregion

        private static bool IsDozing => MapId.Equals(InGame.instance.SelectedMap);

        private static void TurnOnLamp(Lamp lamp) {
            lamp.GetComponent<MeshRenderer>().material.mainTexture = texturesBundle.GetResource<Texture2D>("Lamp On");
            lamp.transform.parent.Find("Glow").gameObject.SetActive(true);
            lamp.onClicked = null;

            SleepyTowers.RemoveSnoozersAroundPoint(lampPos[lamp.index]);
        }

        private static void TurnOffLamp(Lamp lamp) {
            lamp.GetComponent<MeshRenderer>().material.mainTexture = texturesBundle.GetResource<Texture2D>("Lamp Off");
            lamp.transform.parent.Find("Glow").gameObject.SetActive(false);
            lamp.onClicked = OnLampClicked;
        }

        public static void OnTowerPlaced(Tower tower) {
            if (IsDozing) {
                for(int i = 0; i < 3; i++) {
                    if (isLampOn[i] && SleepyTowers.IsTowerInRangeOfPoint(tower, lampPos[i]))
                        return;
                }

                SleepyTowers.RegisterSleeper(tower);
            }
        }

        public static void OnRoundEnd(List<TowerToSimulation> towerList, int round) {
            if (IsDozing) {
                foreach (TowerToSimulation tts in towerList)
                    SleepyTowers.TrySnoozers(tts.tower, round);
            }
        }

        public static void OnTowerSave(Tower tower, TowerSaveDataModel saveData) {
            if (IsDozing)
                SleepyTowers.SaveSleeper(tower, saveData);
        }

        public static void OnTowerLoad(Tower tower, TowerSaveDataModel saveData) {
            if (IsDozing)
                SleepyTowers.LoadSleeper(tower, saveData);
        }

        public static void OnTowerDestroy(Tower tower) {
            if (IsDozing)
                SleepyTowers.RemoveSleeper(tower);
        }

        public static void OnMapSave(MapSaveDataModel saveData) {
            if (IsDozing)
                saveData.metaData[nameof(isLampOn)] = string.Join(',', isLampOn);
        }

        public static void OnMapLoad(MapSaveDataModel saveData) {
            if (IsDozing && saveData.metaData.ContainsKey(nameof(isLampOn))) {
                bool[] loaded = saveData.metaData[nameof(isLampOn)].Split(',').Select(s => bool.Parse(s)).ToArray();
                if (loaded.Length < 3)
                    loaded = new bool[] { false, false, false };
                isLampOn = loaded;

                for(int i = 0; i < 3; i++) {
                    if (isLampOn[i]) {
                        TurnOnLamp(lamps[i]);
                        currentCost *= costMult;
                    } else
                        TurnOffLamp(lamps[i]);
                }
            }
        }

        public static void OnMapReset(bool fullQuit) {
            if (IsDozing) {
                SleepyTowers.RemoveSleepers();
                currentCost = initialCost;
                isLampOn = new bool[] { false, false, false };
                if (!fullQuit) {
                    for(int i = 0; i < 3; i++)
                        TurnOffLamp(lamps[i]);
                }
            }
        }

        public static Sprite LoadSprite(string name) => texturesBundle.GetResource<Sprite>(name);
    }
}
