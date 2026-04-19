using System;
using System.Collections.Generic;
using UnityEngine;

public class HTN_CardPlanner
{
    public event Action<TaskResult> OnPlanFinished;
    private EnemyAI actor;

    private HTNTask rootTask;
    private HTN_PlanRunner planRunner;

    #region Setup
    public void Init(HTN_PlanRunner runner, EnemyAI actor)
    {
        this.actor = actor;

        SetupWorldState();
        SetupDomain();
        SetupPlanRunner(runner);
    }

    private void SetupWorldState()
    {
        actor.sensor.GetWorldState().StateChanged += OnWorldStateChanged;
    }

    private void SetupDomain()
    {
        rootTask = new HTN_CardComproudTask();
    }

    private void SetupPlanRunner(HTN_PlanRunner runner)
    {
        if (actor == null)
        {
            Debug.LogError("Actor not defined");
            return;
        }

        planRunner = runner;
        planRunner.Init(actor.sensor.GetWorldState(), actor);
        planRunner.PlanFinished += OnPlanExecutionFinished;
    }

    #endregion

    public bool GetPlan()
    {
        //Debug.Log("StartFindingPlan");

        List<HTN_PrimitiveTask> plan = null;
        if (rootTask is HTNCompoundTask compound)
        {
            //Debug.Log("CompundTask");
            plan = DecomposeCompoundTask(compound);
        }
        else if(rootTask is HTN_PrimitiveTask primitive)
        {
            //Debug.Log("PRIMITIVE");
            if (primitive.IsAvailable(actor.sensor.GetWorldState()))
            {
                plan = new List<HTN_PrimitiveTask>() { primitive };
                primitive.ApplyEffects(actor.sensor.GetWorldState());
            }
        }

        if (plan != null && plan.Count > 0)
        {
            //Debug.Log("New plan");
            /*foreach (var task in plan)
            {
                Debug.Log(task.GetType().FullName);
            }*/
            return planRunner.SetPlan(plan);
        }
        //Debug.Log("EndPlanning");
        return false;
    }

    private List<HTN_PrimitiveTask> DecomposeCompoundTask(HTNCompoundTask task)
    {
        var methods = task.GetMethods();
        var currentState = actor.sensor.GetWorldState();

        foreach (var method in methods)
        {
            if (CheckConditions(method.PreConditions, currentState))
            {
                var decomposedTasks = DecomposeTasks(method.Tasks);

                if (decomposedTasks.Count == 0)
                {
                    continue;
                }
                return decomposedTasks;
            }
        }
        actor.sensor.SetNewWorldState(currentState);
        if (actor.sensor.GetWorldState() == null) Debug.LogError("WorldState is null");
        return new List<HTN_PrimitiveTask>();
    }

    private List<HTN_PrimitiveTask> DecomposeTasks(List<HTNTask> tasks)
    {
        var decomposedTasks = new List<HTN_PrimitiveTask>();
        var currentState = actor.sensor.GetWorldState();

        foreach (var childTask in tasks)
        {
            if (childTask is HTNCompoundTask compoundTask)
            {
                var t = DecomposeCompoundTask(compoundTask);
                if (t.Count == 0) return new List<HTN_PrimitiveTask>(); // Неудача
                decomposedTasks.AddRange(t);
            }
            else if (childTask is HTN_PrimitiveTask primitiveTask)
            {
                if (primitiveTask.IsAvailable(currentState))
                {
                    primitiveTask.ApplyEffects(currentState);
                    decomposedTasks.Add(primitiveTask);
                }
                else
                {
                    return new List<HTN_PrimitiveTask>(); // Неудача
                }
            }
            else
                return new List<HTN_PrimitiveTask>();
        }
        actor.sensor.SetNewWorldState(currentState);
        if (actor.sensor.GetWorldState() == null) Debug.LogError("WorldState is null");
        return decomposedTasks;
    }

    private void OnWorldStateChanged()
    {
        //Debug.Log("World state changed");
        //GetPlan();
    }

    private void OnPlanExecutionFinished(TaskResult result)
    {
        //Debug.Log("Plan finished.");
        //GetPlan();
        OnPlanFinished?.Invoke(result);
    }

    private bool CheckConditions(Dictionary<string, object> conditions, HTNWorldState worldState)
    {
        foreach (var key in conditions.Keys)
        {
            if (!object.Equals(worldState.GetValue(key), conditions[key]))
                return false;
        }
        return true;
    }

    public void KillPlanner()
    {
        planRunner.PlanFinished -= OnPlanExecutionFinished;
        actor.sensor.GetWorldState().StateChanged -= OnWorldStateChanged;
    }
}
