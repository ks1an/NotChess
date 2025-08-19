using System.Collections.Generic;

public sealed class MathOperations
{
    #region Singleton
    MathOperations() { }
    static MathOperations _instance;
    static readonly object _lock = new();

    public static MathOperations GetInstance()
    {
        if(_instance == null)
            lock (_lock)
                _instance ??= new MathOperations();

        return _instance;
    }
    #endregion

    readonly System.Random rnd = new();

    public void ShuffleList<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rnd.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    /// <summary>
    /// min inclusive, max inclusive. Auto swap if max is not greater
    /// </summary>
    public float GetSafeRandom(float min, float max)
    {
        if (min == max)
            return min;

        float mi = min;
        if (mi > max)
        {
            min = max;
            max = mi;
        }

        return UnityEngine.Random.Range(min, max);
    }
}
