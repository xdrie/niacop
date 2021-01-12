namespace Nia.Native {
    public struct KeyboardEvent {
        public string key;
        public bool pressed;

        public KeyboardEvent(string key, bool pressed) {
            this.key = key;
            this.pressed = pressed;
        }

        public override string ToString() => $"{key} {pressed}";
    }
}