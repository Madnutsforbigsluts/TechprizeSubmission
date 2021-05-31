namespace Glyph
{

    public enum ScreenUpdateState {
        None, 
        LocationUpdate,
        TransitionUpdate,
        BattleUpdate
    }; 

    public static class ScreenState 
    {
        public static ScreenUpdateState screenState { get; set; }
    }
}
