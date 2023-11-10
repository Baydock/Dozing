using Dozing.Properties;
using Il2CppAssets.Scripts.Models.Profile;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Unity.Bridge;
using Il2CppAssets.Scripts.Unity.Display;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Utils;
using System.Collections.Generic;
using UnityEngine;
using static Il2CppAssets.Scripts.Models.Bloons.Behaviors.StunTowersInRadiusActionModel;
using NKVector2 = Il2CppAssets.Scripts.Simulation.SMath.Vector2;
using Resources = Dozing.Properties.Resources;

namespace Dozing {
    public static class SleepyTowers {
        private static readonly AssetBundle bundle = Resources.GetAssetBundle("sleepytowers");

        private const string MutatorId = "sleepyTimeMutator";
        private const string LastAwakeId = "lastAwake";
        private const string ZsId = "Z's";
        private const int RoundsAwake = 5;

        private static Dictionary<int, int> LastAwake { get; } = new();

        public static void RegisterSleeper(Tower tower) {
            if (!LastAwake.ContainsKey(tower.Id.Id))
                LastAwake[tower.Id.Id] = InGame.Bridge.GetCurrentRound();
        }

        public static void TrySnoozers(Tower tower, int round) {
            if (LastAwake.ContainsKey(tower.Id.Id)) {
                int lastAwake = LastAwake[tower.Id.Id];
                if (round >= lastAwake + RoundsAwake - 1)
                    Snoozers(tower);
            }
        }

        public static void SaveSleeper(Tower tower, TowerSaveDataModel save) {
            if (LastAwake.ContainsKey(tower.Id.Id))
                save.metaData.Add(LastAwakeId, $"{LastAwake[tower.Id.Id]}");
        }

        public static void LoadSleeper(Tower tower, TowerSaveDataModel save) {
            if (save.metaData.ContainsKey(LastAwakeId)) {
                if (!int.TryParse(save.metaData["lastAwake"], out int lastAwake))
                    return;
                LastAwake[tower.Id.Id] = lastAwake;
                TrySnoozers(tower, InGame.Bridge.GetCurrentRound());
            }
        }

        public static void RemoveSleeper(Tower tower) {
            LastAwake.Remove(tower.Id.Id);
        }

        public static void RemoveSleepers() {
            LastAwake.Clear();
        }

        private static UnityDisplayNode Zs { get; set; } = null;
        public static bool LoadDisplay(string name, Factory.__c__DisplayClass21_0 assetFactory, System.Action<UnityDisplayNode> onComplete) {
            if (name.Equals(ZsId)) {
                UnityDisplayNode udn = Zs;
                if (udn is null) {
                    GameObject zs = Object.Instantiate(bundle.GetResource<GameObject>(ZsId));
                    udn = Zs = zs.AddComponent<UnityDisplayNode>();
                }
                onComplete(udn);
                return true;
            }
            return false;
        }

        private static void Snoozers(Tower tower) {
            if (tower.IsMutatedBy(MutatorId))
                return;
            TowerFreezeMutator sleepyTime = new(new PrefabReference() { guidRef = ZsId }, false);
            sleepyTime.id = MutatorId;
            tower.AddMutator(sleepyTime);
        }

        private static void RemoveSnoozers(Tower tower) {
            if (!tower.IsMutatedBy(MutatorId))
                return;
            tower.RemoveMutatorsById(MutatorId);
        }

        public static void RemoveSnoozersAroundPoint(NKVector2 point) {
            foreach (TowerToSimulation tts in InGame.Bridge.GetAllTowers()) {
                Tower tower = tts.tower;
                if (LastAwake.ContainsKey(tower.Id.Id) && IsTowerInRangeOfPoint(tower, point)) {
                    LastAwake.Remove(tower.Id.Id);
                    RemoveSnoozers(tower);
                }
            }
        }

        public static bool IsTowerInRangeOfPoint(Tower tower, NKVector2 point) {
            return point.Distance(tower.Position.ToVector2()) <= DozingMap.lampRange;
        }
    }
}
