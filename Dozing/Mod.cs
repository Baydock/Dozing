using Dozing.CustomMonoBehaviors;
using Dozing.Utils;
using HarmonyLib;
using Il2CppAssets.Scripts.Data;
using Il2CppAssets.Scripts.Models.Map;
using Il2CppAssets.Scripts.Models.Profile;
using Il2CppAssets.Scripts.Simulation.Bloons;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Unity.Bridge;
using Il2CppAssets.Scripts.Unity.Display;
using Il2CppAssets.Scripts.Unity.Map;
using Il2CppAssets.Scripts.Unity.UI_New;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.Main.ModeSelect;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.Threading.Tasks;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using Map = Il2CppAssets.Scripts.Simulation.Track.Map;
using Path = Il2CppAssets.Scripts.Simulation.Track.Path;
using Scene = UnityEngine.SceneManagement.Scene;

[assembly: MelonInfo(typeof(Dozing.Mod), "Dozing", "1.0.0", "Baydock")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]
[assembly: MelonAuthorColor(255, 255, 104, 0)]

namespace Dozing {
    [HarmonyPatch]
    public class Mod : MelonMod {
        public static MelonLogger.Instance Logger { get; set; }

        public override void OnInitializeMelon() {
            Logger = LoggerInstance;

            ClassInjector.RegisterTypeInIl2Cpp<Lamp>(new() { LogSuccess = false });
        }

        #region Game logic

        [HarmonyPatch(typeof(Tower), nameof(Tower.OnPlace))]
        [HarmonyPostfix]
        public static void WhenPlaceTower(Tower __instance) => DozingMap.OnTowerPlaced(__instance);

        [HarmonyPatch(typeof(UnityToSimulation), nameof(UnityToSimulation.OnEarlyRoundEndSim))]
        [HarmonyPostfix]
        public static void EepyMonkers(UnityToSimulation __instance, int round) => DozingMap.OnRoundEnd(__instance.GetAllTowers(), round);

        [HarmonyPatch(typeof(Tower), nameof(Tower.GetSaveData))]
        [HarmonyPostfix]
        public static void SaveSleepers(Tower __instance, TowerSaveDataModel __result) => DozingMap.OnTowerSave(__instance, __result);

        [HarmonyPatch(typeof(Tower), nameof(Tower.SetSaveData))]
        [HarmonyPostfix]
        public static void LoadSleepers(Tower __instance, TowerSaveDataModel towerData) => DozingMap.OnTowerLoad(__instance, towerData);

        [HarmonyPatch(typeof(Map), nameof(Map.GetSaveData))]
        [HarmonyPostfix]
        public static void SaveLamps(MapSaveDataModel mapData) => DozingMap.OnMapSave(mapData);

        [HarmonyPatch(typeof(Map), nameof(Map.SetSaveData))]
        [HarmonyPostfix]
        public static void LoadLamps(MapSaveDataModel mapData) => DozingMap.OnMapLoad(mapData);

        [HarmonyPatch(typeof(Tower), nameof(Tower.OnDestroy))]
        [HarmonyPostfix]
        public static void NoMoreEepy(Tower __instance) => DozingMap.OnTowerDestroy(__instance);

        [HarmonyPatch(typeof(UnityToSimulation), nameof(UnityToSimulation.Restart))]
        [HarmonyPostfix]
        public static void ResetSleepers() => DozingMap.OnMapReset(false);

        [HarmonyPatch(typeof(MapLoader), nameof(MapLoader.ClearMap))]
        [HarmonyPostfix]
        public static void NoMoSleepies() => DozingMap.OnMapReset(true);

        #endregion

        #region Map and assets

        [HarmonyPatch(typeof(Factory.__c__DisplayClass21_0), nameof(Factory.__c__DisplayClass21_0._CreateAsync_b__0))]
        [HarmonyPrefix]
        public static bool LoadModels(Factory.__c__DisplayClass21_0 __instance, UnityDisplayNode prototype) {
            string objectId = __instance.objectId.guidRef;
            if (!string.IsNullOrEmpty(objectId) && prototype is null) {
                return !SleepyTowers.LoadDisplay(objectId, __instance, new System.Action<UnityDisplayNode>(proto => {
                    DisplayHelpers.SetUpPrototype(proto, __instance.objectId, __instance);
                    DisplayHelpers.SetUpDisplay(proto, __instance);
                }));
            }
            return true;
        }

        [HarmonyPatch(typeof(SpriteAtlas), nameof(SpriteAtlas.GetSprite))]
        [HarmonyPrefix]
        public static bool LoadSprites(ref Sprite __result, string name) {
            if (!string.IsNullOrEmpty(name)) {
                Sprite sprite = DozingMap.LoadSprite(name);
                if (sprite is not null) {
                    __result = sprite;
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(GameData), nameof(GameData.LoadAsync))]
        [HarmonyPostfix]
        public static void AddMap(GameData __instance, Task __result) {
            __result.ContinueWith(new System.Action<Task>(t => {
                __instance.mapSet.Maps.items = __instance.mapSet.Maps.items.Append(DozingMap.Details);
            }));
        }

        [HarmonyPatch(typeof(ProfileModel), nameof(ProfileModel.Validate))]
        [HarmonyPostfix]
        public static void UnlockMap(ProfileModel __instance) => __instance.mapInfo.GetMap(DozingMap.MapId).seen = true;

        [HarmonyPatch(typeof(ModeButton), nameof(ModeButton.Initialise))]
        [HarmonyPostfix]
        public static void UnlockDifficulties(ModeButton __instance) {
            if (InGameData.Editable.selectedMap.Equals(DozingMap.MapId))
                __instance.unlockMode = "";
        }

        private static string actualCurrentMap = null;
        [HarmonyPatch(typeof(MapLoader), nameof(MapLoader.LoadScene))]
        [HarmonyPrefix]
        public static bool LoadScene(MapLoader __instance) {
            if (__instance.currentMapName.Equals(DozingMap.MapId)) {
                actualCurrentMap = DozingMap.MapId;
                __instance.currentMapName = "Blons";
                return true;
            }
            actualCurrentMap = null;
            return true;
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName) {
            if (DozingMap.MapId.Equals(sceneName)) {
                Scene dozingScene = SceneManager.GetSceneByName(DozingMap.MapId);
                DozingMap.ModifyScene(dozingScene);
            }
        }

        [HarmonyPatch(typeof(UnityToSimulation), nameof(UnityToSimulation.InitMap))]
        [HarmonyPrefix]
        public static bool LoadMap(MapModel map) {
            if (DozingMap.MapId.Equals(actualCurrentMap)) {
                map.areas = DozingMap.AreaModels;
                map.blockers = DozingMap.BlockerModels;
                map.coopAreaLayouts = DozingMap.CoopAreaLayoutModels;
                map.paths = DozingMap.PathModels;
                map.removeables = DozingMap.RemoveableModels;
                map.gizmos = DozingMap.MapGizmoModels;
                map.spawner = DozingMap.PathSpawnerModel;
                map.mapEvents = DozingMap.MapEventModels;
                map.mapWideBloonSpeed = DozingMap.MapWideBloonSpeed;
                SceneManager.LoadScene(DozingMap.ScenePath, LoadSceneMode.Additive);
                Scene scene = SceneManager.GetSceneByName("Blons");
                scene.GetRootGameObjects()[0].SetActive(false);
            }
            return true;
        }

        [HarmonyPatch(typeof(MapLoader), nameof(MapLoader.ClearMap))]
        [HarmonyPrefix]
        public static bool UnloadMap() {
            if (DozingMap.MapId.Equals(actualCurrentMap))
                SceneManager.UnloadScene(DozingMap.MapId);
            return true;
        }

        private static float lastX = 0, lastY = 0;
        private static bool run = false;

        [HarmonyPatch(typeof(InGame), "Update")]
        [HarmonyPostfix]
        public static void GetPositions() {
            Vector2 cursorPosition = InGame.instance.inputManager.cursorPositionWorld;
            if (Input.GetKeyDown(KeyCode.LeftBracket)) {
                run = !run;
                if (lastX != cursorPosition.x || lastY != cursorPosition.y) {
                    System.Console.WriteLine("new(" + System.Math.Round(cursorPosition.x, 1) + ", " + System.Math.Round(cursorPosition.y, 1) + ")");
                    lastX = cursorPosition.x;
                    lastY = cursorPosition.y;
                }
            }
        }

        #endregion
    }
}
