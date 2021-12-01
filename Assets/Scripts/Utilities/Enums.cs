public enum PathType
{
    Empty,
    Horizontal,
    Vertical,
    TopLeftCorner,
    BottomLeftCorner,
    TopRightCorner,
    BottomRightCorner,
    Cross
}

public enum ButtonState
{
    Hidden,
    Next,
    Close,
}

public enum LevelMode
{
    Edit,       // Player entered edit mode but as is not actively drawing a path
    Drawing,    // Player is actively drawing path
    Playing,    // Player has switched to making the shepherds move
}

public enum EntranceState
{
    Opened,
    Closed,
}

public enum BeeState
{
    Idle,
    Move,
    Attack,
    Bonk,
    Dead,
}

public enum ShieldState
{
    Disabled,
    Attached,
    Thrown,
    Throwing,
    Recalled,
    Attaching,
}

public enum ShieldAttribute
{
    Normal,
    Fire,
}

public enum Direction
{
    North,
    West,
    South,
    East,
}

public enum ShieldParticle
{ 
    Fire,
    Poof,
    Smoke,
}

public enum BeeColor
{
    Yellow,
    Red,
    Black,
    Blue,
}

public enum LevelState
{
    Loading,
    Transitioning,
    Playing,
    GameOver,
    Completed,
    Paused,
}