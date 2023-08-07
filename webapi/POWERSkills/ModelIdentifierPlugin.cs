using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;
using System.ComponentModel;
using System.Threading.Tasks;

namespace POWEREngineers.Bucky.Skills.POWEREngPlugins;

public class ModelIdentifierPlugin
{
    IKernel _kernel;

    public ModelIdentifierPlugin(IKernel kernel)
    {
        _kernel = kernel;
    }

    [SKFunction, Description("Routes the request to the appropriate data retrieval function.")]
    public async Task<string> RouteRequest(SKContext context)
    {
        // Save the original user request
        string request = context["input"];

        // Retrieve the intent from the user request
        var targetModel = _kernel.Skills.GetFunction("ModelIdentifierPlugin", "GetIntendedModel");
        await targetModel.InvokeAsync(context);
        string intent = context["input"].Trim();
        string data = string.Empty;
        // Call the appropriate function
        switch (intent)
        {
            case "PDQMS":
                var qmsSkill = new POWERQMSSkill(_kernel);
                data = await qmsSkill.QueryQMSIndexAsync(request);
                return $"PDQMS data:{data}";
            case "POWERAUS":
                var ausSkill = new POWERAUSSkill(_kernel);
                data = await ausSkill.QueryAUSModelAsync(request);
                return $"POWERAUS data:{data}";
            case "PDSUBKMP":
                var kmpSkill = new POWERKMPSkill(_kernel);
                data = await kmpSkill.QueryKMPModelAsync(request);
                return $"PDSUBKMP data:{data}";
            case "UNKNOWN":
            default:
                return data;
        }
    }
}