using Il2CppAssets.Scripts.Unity.Display;
using Il2CppAssets.Scripts.Utils;
using UnityEngine;

namespace Dozing.Utils {
    internal class DisplayHelpers {
        public static void SetUpPrototype(UnityDisplayNode proto, PrefabReference protoRef, Factory.__c__DisplayClass21_0 assetFactory) {
            proto.transform.parent = assetFactory.__4__this.PrototypeRoot;
            proto.Active = false;
            proto.gameObject.transform.position = new Vector3(-3000, 0, 0);
            proto.gameObject.transform.eulerAngles = Vector3.zero;
            proto.cloneOf = protoRef;
        }
        public static void SetUpDisplay(UnityDisplayNode proto, Factory.__c__DisplayClass21_0 assetFactory) {
            UnityDisplayNode display = Object.Instantiate(proto.gameObject, assetFactory.__4__this.DisplayRoot).GetComponent<UnityDisplayNode>();

            display.transform.parent = assetFactory.__4__this.DisplayRoot;
            display.Active = true;
            display.cloneOf = proto.cloneOf;
            assetFactory.__4__this.active.Add(display);
            assetFactory.onComplete?.Invoke(display);
        }
    }
}
