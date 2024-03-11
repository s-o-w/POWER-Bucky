using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Threading.Tasks;

#pragma warning disable IDE0130
namespace POWEREngineers.Bucky.Skills.POWEREngPlugins;
#pragma warning restore IDE0130

public class ModelIdentifierPlugin
{
    private readonly Kernel _kernel;

    public ModelIdentifierPlugin(Kernel kernel)
    {
        this._kernel = kernel;
    }

    public ModelIdentifierPlugin() { }

    [KernelFunction, Description("Routes the request to the appropriate data retrieval function.")]
    public async Task<string> RouteRequest(KernelArguments context)
    {
        string data = string.Empty;
        // check to see if we called this from AutoCAD and are using the AutoCAD Copilot
        if (context.TryGetValue("POWERSkillTarget", out object? value))
        {
            if (value as string == "LispForCAD")
            {
                var cadSkill = this._kernel.Plugins.GetFunction("CADPlugin", "LispForAutoCAD");
                await cadSkill.InvokeAsync(this._kernel, context);
                if (context.TryGetValue("input", out object? input))
                {
                    var val = input as string;
                    return $"CADCP data:{val}";
                }
                return data;
            }
            return data;
        }

        // Save the original user request
        var request = context["input"] as string;

        if (!string.IsNullOrEmpty(request))
        {
            // Retrieve the intent from the user request
            var targetModel = this._kernel.Plugins.GetFunction("ModelIdentifierPlugin", "GetIntendedModel");
            await targetModel.InvokeAsync(this._kernel, context);
            string intent = request.Trim();
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
                    break;
            }
        }
        return data;
    }
}