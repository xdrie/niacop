namespace niacop.Native.WindowManagers {
    public class KeyboardEvent {
        public bool pressed;
        public char keyChar;
        public int keycode;

        public override string ToString() => $"{keyChar} {pressed}";
    }
}