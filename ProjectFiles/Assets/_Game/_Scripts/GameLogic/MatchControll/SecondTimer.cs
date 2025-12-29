using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class SecondTimer : MonoBehaviour
{
    public static SecondTimer Instance;
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Debug.LogError("You can't have more than one second timer!");
            Destroy(this);
        }
    }

    readonly WaitForSeconds timeDelay = new(0.1f);
    public List<SecondTimerSubscriber> subscribers = new();
    bool isRunning;

    public void StartTimer(float secondsLife, out SecondTimerSubscriber subscriber, 
        Action actionOnCompleted, Action actionOnTick = null)
    {
        subscriber = new SecondTimerSubscriber(secondsLife, actionOnCompleted, actionOnTick);
        subscribers.Add(subscriber);
        if (!isRunning)
        {
            isRunning = true;
            StartCoroutine(TimerFunction());
            GameController.Instance.states.OnSetSettings += ResetTimer;
        }
    }

    IEnumerator TimerFunction()
    {      
        while (true)
        {
            if (isRunning)
                TimeCount();
            else
                yield break;

            yield return timeDelay;
        }
    }

    void TimeCount()
    {
        foreach (SecondTimerSubscriber subscriber in subscribers.ToList())
        {
            subscriber.onTick?.Invoke();
            subscriber.secondLifeLeft -= 0.1f;
            if (subscriber.secondLifeLeft <= 0)
            {
                subscribers.Remove(subscriber);
                subscriber.onComplete?.Invoke();
            }
        }
        if (subscribers.Count == 0)
            ResetTimer();
    }

    void ResetTimer()
    {
        GameController.Instance.states.OnSetSettings -= ResetTimer;
        subscribers.Clear();
        isRunning = false;
        StopCoroutine(TimerFunction());
    }
}

public class SecondTimerSubscriber
{
    public float secondLifeLeft;
    public Action onComplete;
    public Action onTick;

    public SecondTimerSubscriber(float secondLife, Action actionOnCompleted, Action onTick)
    {
        if (secondLife < -1)
        {
            Debug.LogError($"{typeof(SecondTimerSubscriber)} get negative secondLife: {secondLife}. Action: " + actionOnCompleted);
            secondLife = 1;
        }
        secondLifeLeft = secondLife;
        onComplete = actionOnCompleted;
        this.onTick = onTick;
    }

    public void AddTurnsLife(float turnsLife)
    {
        secondLifeLeft += turnsLife;
        if (secondLifeLeft <= 0)
            secondLifeLeft = 1;
    }
}
