using System.Collections.Generic;

public class HTN_CardComproudTask : HTNCompoundTask
{
    public override IList<MethodDefinition> GetMethods()
    {
        var methods = new List<MethodDefinition>();

        var method0 = new MethodDefinition
        {
            PreConditions = new Dictionary<string, object> { },
            Tasks = new List<HTNTask>
            {
                new HTN_PlayDistantRelativeCard(),
            }
        };
        methods.Add(method0);

        var method1 = new MethodDefinition
        {
            PreConditions = new Dictionary<string, object> { },
            Tasks = new List<HTNTask>
            {
                new HTN_PlayLightingBoltCard()
            }
        };
        methods.Add(method1);

        var method2 = new MethodDefinition
        {
            PreConditions = new Dictionary<string, object> { },
            Tasks = new List<HTNTask>
            {
                new HTN_PlayMeteorRainCard(),
            }
        };
        methods.Add(method2);

        return methods;
    }

    protected override Dictionary<string, object> PreConditions()
    {
        return base.PreConditions();
    }

    public override TaskResult Execute(float delta, object actor, HTNWorldState worldState)
    {
        throw new System.NotImplementedException();
    }

    protected override Dictionary<string, object> Effects(HTNWorldState worldState)
    {
        return base.Effects(worldState);
    }
}
