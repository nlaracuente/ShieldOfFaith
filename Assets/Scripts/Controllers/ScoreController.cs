using System.Collections.Generic;
using UnityEngine;

public class ScoreController : Singleton<ScoreController>
{
    [SerializeField, Tooltip("How many digits the score should be so that we can append zero to keep it consistent")]
    int leadingZeros = 4;

    [SerializeField, Tooltip("Default score to awared the player when we cannot determine how much to give for a bee")]
    int defaultScore = 0;

    [SerializeField, Tooltip("Keeps track of the current score")]
    int score;
    public int Score { get { return score; } }
    public string ScoreText { get { return score.ToString($"D{leadingZeros}"); } }

    [SerializeField, Tooltip("Tracks the current multiplier")]
    float multiplier = 1;

    [SerializeField, Tooltip("How much to increase the multiplier by when the bee is killed by fire")]
    float fireMultiplier = 1f;

    [SerializeField, Tooltip("Total bees to kill in a row before getting a new heart")]
    int totalBeesForHealthPickup = 5;

    [SerializeField, Tooltip("How many points to award for killing a bee of a specific color")]
    List<BeeScore> beeScores;
    Dictionary<BeeColor, int> beeScoreMapping;

    Shield Shield { get { return GameManager.instance.Shield; } }

    private void Start()
    {
        BeeEnemyController.instance.RegisterOnBeeKilled(OnBeeKilled);
        beeScoreMapping = new Dictionary<BeeColor, int>();
        foreach (var beeScore in beeScores)
            beeScoreMapping[beeScore.beeColor] = beeScore.score;
    }

    private void Update()
    {
        // Reset multiplier when the player picks up the shield
        if (Shield.IsAttached)
            multiplier = 1;       
    }

    public void SetScore(int _score) => score = _score;
    Player Player { get { return GameManager.instance.Player; } }

    /// <summary>
    /// Adds points to the total score based on the type of bee, current multiplier, and how the bee was killed
    /// </summary>
    /// <param name="bee"></param>
    public void OnBeeKilled(Bee bee)
    {
        var total = beeScoreMapping.ContainsKey(bee.BeeColor) ? beeScoreMapping[bee.BeeColor] : defaultScore;

        // Add multiplier for fire
        var multiple = multiplier;
        if (Shield.IsOnFire)
            multiple += fireMultiplier;

        // Calculate points
        var points = (int)(total * multiple);
        score += points;
        bee.ShowKillPoints(points);

        // Combo sound goes up in pitch with each bee (1.2, 1.3, 1.4)
        if (multiplier > 1)
        {
            var comboSound = AudioManager.instance.Play(SFXLibrary.instance.score);
            comboSound.Play();
            comboSound.pitch = 1 + multiplier / 10;
        }

        // Get health if this is the -Nth- bee killed before picking up the shield again
        if (multiplier >= totalBeesForHealthPickup)
            Player.IncreaseFaith();

        // Increase the multiplier if the bee was killed while the shield is not attached
        else if (!Shield.IsAttached)
            multiplier++;
    }
}
