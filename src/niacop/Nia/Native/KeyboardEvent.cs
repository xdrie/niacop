namespace Nia.Native {
    public class KeyboardEvent {
        public string key;
        public bool pressed;

        public KeyboardEvent(string key, bool pressed) {
            this.key = key;
            this.pressed = pressed;
        }

        public override string ToString() => $"{key} {pressed}";
    }
}