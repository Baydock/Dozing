using UnityEngine;

namespace Dozing.CustomMonoBehaviors {
    public class Lamp : MonoBehaviour {
        public System.Action<Lamp> onClicked;
        public int index;

        public Lamp(System.IntPtr ptr) : base(ptr) { }

        public void OnMouseUpAsButton() {
            onClicked?.Invoke(this);
        }
    }
}
