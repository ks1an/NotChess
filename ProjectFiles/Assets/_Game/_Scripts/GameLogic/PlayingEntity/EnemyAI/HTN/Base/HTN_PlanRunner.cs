using System;
using System.Collections.Generic;
using UnityEngine;

public class HTN_PlanRunner : MonoBehaviour
{
    public event Action<TaskResult> PlanFinished;

    private HTNWorldState worldState;
    private List<HTN_PrimitiveTask> tasks = new();
    private object actor;

    public void Init(HTNWorldState state, object actor)
    {
        worldState = state;
        this.actor = actor;
    }

    public bool SetPlan(List<HTN_PrimitiveTask> tasks)
    {
        //Debug.Log($"New plan. Tasks count: {tasks.Count}");
        this.tasks = tasks;
        return ExecuteCurrentTask(Time.deltaTime);
    }

    void Update()
    {
        if (tasks.Count == 0)
            return;

        ExecuteCurrentTask(Time.deltaTime);
    }

    private bool ExecuteCurrentTask(float delta)
    {
        var currentTask = tasks[0];
        var result = currentTask.Execute(delta, actor, worldState);

        switch (result)
        {
            case TaskResult.RUNNING:
                return true;
            case TaskResult.FAILURE:
                //Debug.Log($"Task FAILURE: {currentTask.GetType().FullName}");
                FinishPlan(TaskResult.FAILURE);
                return false;
            case TaskResult.SUCCESS:
                //Debug.Log($"Task SUCCESS: {currentTask.GetType().FullName}");
                NextTask(delta);
                return true;
        }
        return false;
    }

    /// <summary>
    /// Переход к следующей задаче.
    /// </summary>
    private void NextTask(float delta)
    {
        tasks.RemoveAt(0); // Удаляем выполненную задачу

        if (tasks.Count == 0)
        {
            FinishPlan(TaskResult.SUCCESS);
            return;
        }

        var newTask = tasks[0];

        if (!newTask.IsAvailable(worldState))
        {
            //Debug.Log($"Error. Task is FAILURE: {newTask.GetType().FullName}");
            FinishPlan(TaskResult.FAILURE);
            return;
        }

        //Debug.Log($"Start next task: {newTask.GetType().FullName}");
        ExecuteCurrentTask(delta);
    }

    private void FinishPlan(TaskResult result)
    {
        tasks.Clear();
        PlanFinished?.Invoke(result);
    }
}
