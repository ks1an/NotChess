
using System.Collections.Generic;

public abstract class HTNCompoundTask : HTNTask
{
    public class MethodDefinition
    {
        public Dictionary<string, object> PreConditions { get; set; } = new();

        public List<HTNTask> Tasks { get; set; } = new();
    }

    public virtual IList<MethodDefinition> GetMethods()
    {
        return new List<MethodDefinition>();
    }
}
