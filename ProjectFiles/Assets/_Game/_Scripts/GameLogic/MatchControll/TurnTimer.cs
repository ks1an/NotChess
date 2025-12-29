using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class TurnTimer
{
    #region Singleton
    TurnTimer() { }
    static TurnTimer _instance;
    static readonly object _lock = new();

    public static TurnTimer GetInstance()
    {
        if (_instance == null)
            lock (_lock)
                _instance ??= new TurnTimer();

        return _instance;
    }
    #endregion

    public List<TurnTimerSubscriber> subscribers = new();
    bool isRunning;
    public void StartTimer(int turnsLife, Action actionOnCompleted, Action actionOnTick,
        out TurnTimerSubscriber subscriber)
    {
        subscriber = new TurnTimerSubscriber(turnsLife, actionOnCompleted, actionOnTick);
        subscribers.Add(subscriber);
        if (!isRunning)
        {
            isRunning = true;
            GameController.Instance.states.OnSetSettings += Reset;
            GameController.Instance.states.OnTurnEnded += OnTurn;
        }
    }

    void OnTurn(int x, int y, Team team)
    {
        foreach (TurnTimerSubscriber subscriber in subscribers.ToList())
        {
            subscriber.onTick?.Invoke();

            subscriber.turnsLifeLeft--;
            if (subscriber.turnsLifeLeft == 0)
            {
                subscribers.Remove(subscriber);
                subscriber.onComplete?.Invoke();
            }
        }
        if (subscribers.Count == 0)
            Reset();
    }

    void Reset()
    {
        subscribers.Clear();
        isRunning = false;
        GameController.Instance.states.OnSetSettings -= Reset;
        GameController.Instance.states.OnTurnEnded -= OnTurn;
    }
}

public class TurnTimerSubscriber
{
    public int turnsLifeLeft;
    public Action onComplete;
    public Action onTick;

    public TurnTimerSubscriber(int turnsLife, Action actionOnCompleted, Action onTick)
    {
        if (turnsLife < -1)
        {
            Debug.LogError($"TurnTimerSubscriber get negative turnsLife: {turnsLife}. Action: " + actionOnCompleted);
            turnsLife = 1;
        }
        turnsLifeLeft = turnsLife;
        onComplete = actionOnCompleted;
        this.onTick = onTick;
    }

    public void AddTurnsLife(int turnsLife)
    {
        turnsLifeLeft += turnsLife;
        if (turnsLifeLeft <= 0)
            turnsLifeLeft = 1;
    }
}
