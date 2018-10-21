namespace niacop.Native.WindowManagers {
    public class KeyboardEvent {
        public bool pressed;
        public string key;

        public override string ToString() => $"{key} {pressed}";
    }
}