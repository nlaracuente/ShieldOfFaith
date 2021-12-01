using System;
using System.Collections.Generic;

[Serializable]
public struct LevelSaveData
{
    public bool allBeesKilled;
    public bool allScripturesCollected;
    public bool levelCompleted;
    public bool[] scripturesCollected;
    public int highScore;
}

[Serializable]
public struct CheckpointData
{
    public bool hasData;
    public string checkpointId;
    public List<string> beesKilled;
    public bool[] scripturesCollected;
    public int totalScore;

    public CheckpointData(string id, List<string> bees, bool[] scriptures, int _totalScore = 0)
    {
        hasData = true;
        checkpointId = id;

        // We make a copy of the following two so that we don't lose the reference on scene reload
        beesKilled = new List<string>(bees);
        scripturesCollected = new bool[scriptures.Length];
        Array.Copy(scriptures, scripturesCollected, scriptures.Length);
        totalScore = _totalScore;
    }

    public override string ToString()
    {
        return $"[CheckpointData] hadData: {hasData}, id: {checkpointId}, bees: {beesKilled}, Score: {totalScore}";
    }
}

[Serializable]
public struct BeeScore
{
    public BeeColor beeColor;
    public int score;
}