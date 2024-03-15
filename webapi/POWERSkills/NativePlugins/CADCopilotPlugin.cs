using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace POWEREngineers.Bucky.POWEREngPlugins;

public class CADCopilotPlugin
{
    private Kernel _kernel;
    public CADCopilotPlugin(Kernel kernel)
    {
        this._kernel = kernel;
    }

    public CADCopilotPlugin() { }

    /// <summary>
    /// Query the QMS Memory collection for documents that match the query.
    /// </summary>
    /// <param name="query">Query to match.</param>
    /// <param name="context">The SkContext.</param>
    [KernelFunction, Description("Use GPT-4 to generate Lisp code to help drafters create content in drawings")]
    //[SKParameter("query", "Query to match.")]
    public async Task<string> GenerateResponse(string query)
    {
        return string.Empty;
    }
}