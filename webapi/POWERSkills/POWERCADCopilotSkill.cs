using Microsoft.SemanticKernel.SkillDefinition;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace POWEREngineers.Bucky.Skills.POWEREngPlugins;

public class CADCopilotSkill
{
    IKernel _kernel;
    public CADCopilotSkill(IKernel kernel)
    {
        _kernel = kernel;
    }

    /// <summary>
    /// Query the QMS Memory collection for documents that match the query.
    /// </summary>
    /// <param name="query">Query to match.</param>
    /// <param name="context">The SkContext.</param>
    [SKFunction, Description("Use GPT-4 to generate Lisp code to help drafters create content in drawings")]
    [SKParameter("query", "Query to match.")]
    public async Task<string> GenerateResponse(string query)
    {
        return string.Empty;
    }
}