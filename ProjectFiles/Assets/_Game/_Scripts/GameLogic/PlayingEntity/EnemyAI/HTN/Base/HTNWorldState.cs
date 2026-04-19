using System;
using System.Collections.Generic;
using System.Linq;

public class HTNWorldState
{
    public Action StateChanged;

    public Dictionary<string, object> state = new();

    public void SetValue(string stateKey, object value, bool notifyStateChange = true)
    {
        if (!state.ContainsKey(stateKey))
        {
            state.Add(stateKey, value);
            //UnityEngine.Debug.Log("Added state: " + stateKey + " With value: " + value);
        }

        if (state.TryGetValue(stateKey, out var currentValue) && Equals(currentValue, value))
        {
            return;
        }

        state[stateKey] = value;
        //UnityEngine.Debug.Log("WorldStateSet state: " + stateKey + " NewValue: " + value);

        if (notifyStateChange)
        {
            StateChanged?.Invoke();
        }
    }

    public object GetValue(string stateKey, object defaultValue = null)
    {
        return state.TryGetValue(stateKey, out var value) ? value : defaultValue;
    }

    public HTNWorldState Duplicate()
    {
        HTNWorldState duplicate = (HTNWorldState)this.MemberwiseClone();
        duplicate.state = state.ToDictionary(entry => entry.Key,
                                               entry => entry.Value);

        return duplicate;
    }

    public void Clear()
    {
        state.Clear();
    }

    public bool ContainsKey(string key)
    {
        return state.ContainsKey(key);
    }
}
