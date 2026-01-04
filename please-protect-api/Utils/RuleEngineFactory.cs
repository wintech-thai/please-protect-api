using RulesEngine.Models;

public class RuleEngineFactory
{
    public static RulesEngine.RulesEngine CreateEngineFromJSON(string jsonText)
    {
        // แปลงจาก JSON → Workflow[]
        var workflows = Newtonsoft.Json.JsonConvert.DeserializeObject<Workflow[]>(jsonText);

        // สร้าง RulesEngine instance
        return new RulesEngine.RulesEngine(workflows, null);
    }

    public static List<Workflow>? CreateWorkflowFromJSON(string jsonText)
    {
        try
        {
            // แปลงจาก JSON → Workflow[]
            var workflows = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Workflow>>(jsonText);
            return workflows;            
        }
        catch
        {
            return null;
        }
    }
}
