using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using System.ComponentModel;
using System.Threading.Tasks;

#pragma warning disable IDE0130
namespace POWEREngineers.Bucky.Skills.POWEREngPlugins;
#pragma warning restore IDE0130

public class ModelIdentifierPlugin
{
    private IKernel _kernel;

    public ModelIdentifierPlugin(IKernel kernel)
    {
        this._kernel = kernel;
    }

    [SKFunction, Description("Routes the request to the appropriate data retrieval function.")]
    public async Task<string> RouteRequest(SKContext context)
    {
        return string.Empty;
        // check to see if we called this from AutoCAD and are using the AutoCAD Copilot
        if (context.Variables.TryGetValue("POWERSkillTarget", out string? value))
        {
            if (value == "LispForCAD")
            {
                var cadSkill = this._kernel.Functions.GetFunction("CADPlugin", "LispForAutoCAD");
                await cadSkill.InvokeAsync(context);
                string toReturn = context.Variables["input"].Trim();
                return $"CADCP data:{toReturn}";
            }
        }

        // Save the original user request
        string request = context.Variables["input"];

        // Retrieve the intent from the user request
        var targetModel = this._kernel.Functions.GetFunction("ModelIdentifierPlugin", "GetIntendedModel");
        await targetModel.InvokeAsync(context);
        string intent = context.Variables["input"].Trim();
        string data = string.Empty;
        // Call the appropriate function
        switch (intent)
        {
            case "PDQMS":
                var qmsSkill = new POWERQMSSkill(this._kernel);
                data = await qmsSkill.QueryQMSIndexAsync(request);
                return $"PDQMS data:{data}";
            case "POWERAUS":
                var ausSkill = new POWERAUSSkill(this._kernel);
                data = await ausSkill.QueryAUSModelAsync(request);
                return $"POWERAUS data:{data}";
            case "PDSUBKMP":
                var kmpSkill = new POWERKMPSkill(this._kernel);
                data = await kmpSkill.QueryKMPModelAsync(request);
                return $"PDSUBKMP data:{data}";
            case "UNKNOWN":
            default:
                return data;
        }
    }
}