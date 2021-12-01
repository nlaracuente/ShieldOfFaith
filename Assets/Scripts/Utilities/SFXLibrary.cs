using UnityEngine;

/// <summary>
/// A collection of the sound effects available 
/// It is suggested that the names of the properties match
/// the names of the audio clip but they can be called whatever you want
/// </summary>
public class SFXLibrary : Singleton<SFXLibrary>
{
    [Tooltip("When hovering over any button")]
    public SoundEffect hubButtonHover;

    [Tooltip("When hitting play on main menu")]
    public SoundEffect hubTransition;

    [Tooltip("When moving next to a level in hub")]
    public SoundEffect hubLevelHover;

    [Tooltip("When selecting a level to play")]
    public SoundEffect hubLevelPlay;

    [Tooltip("When the player throws the shield")]
    public SoundEffect shieldThrow;

    [Tooltip("When the shield is in motion - LOOP")]
    public SoundEffect shieldSpin;

    [Tooltip("When the shield hits a wall or enemy")]
    public SoundEffect shieldBounce;

    [Tooltip("When the shield starts to return to the player")]
    public SoundEffect shieldRecall;

    [Tooltip("When the player catches a moving shield")]
    public SoundEffect shieldCaught;

    [Tooltip("When the player picks up the shield")]
    public SoundEffect shieldPickup;

    [Tooltip("When the shield lights on fire")]
    public SoundEffect shieldFire;

    [Tooltip("When the shield goes from being on fire to normal")]
    public SoundEffect shieldFireOut;

    [Tooltip("When the shield kills a bee")]
    public SoundEffect shieldHit;

    [Tooltip("When the shield defends a stinger")]
    public SoundEffect shieldRicochet;

    [Tooltip("When a button is activated")]
    public SoundEffect buttonOn;

    [Tooltip("When spikes are activated")]
    public SoundEffect spikes;

    [Tooltip("When a door opens")]
    public SoundEffect door;

    [Tooltip("When the shield hits a bumper")]
    public SoundEffect bumper;

    [Tooltip("When a bee attacks")]
    public SoundEffect beeAttack;

    [Tooltip("When a bee dies")]
    public SoundEffect beeDie;

    [Tooltip("When a hive shoots stingers")]
    public SoundEffect hiveAttack;

    [Tooltip("When a hive gets hit and doesn't get destroyed")]
    public SoundEffect hiveHit;

    [Tooltip("When a hive gets destroyed")]
    public SoundEffect hiveDestroy;

    [Tooltip("When a crate gets destroyed")]
    public SoundEffect crateDestroy;

    [Tooltip("When a metal crate gets destroyed")]
    public SoundEffect crateMetal;

    [Tooltip("When an ice block gets destroyed")]
    public SoundEffect crateIce;

    [Tooltip("When the player gets hurt")]
    public SoundEffect playerHurt;

    [Tooltip("When the player dies")]
    public SoundEffect playerDie;

    [Tooltip("When the player animator frames are taking a step")]
    public SoundEffect playerWalk;

    [Tooltip("When the player activates a checkpoint")]
    public SoundEffect checkpoint;

    [Tooltip("When the player or shield picks up a scripture")]
    public SoundEffect scripture;

    [Tooltip("When the score goes up hitting bees")]
    public SoundEffect score;

    [Tooltip("When the score gets you an extra heart")]
    public SoundEffect scoreHeart;

    [Tooltip("When you earn a badge")]
    public SoundEffect badge;

    [Tooltip("NOT THE BEES")]
    public SoundEffect nickCage;
}
