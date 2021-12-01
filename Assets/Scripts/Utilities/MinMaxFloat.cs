[System.Serializable]
public struct MinMaxFloat
{
    public float min;
    public float max;

    public MinMaxFloat(float _min, float _max)
    {
        min = _min;
        max = _max;
    }
}