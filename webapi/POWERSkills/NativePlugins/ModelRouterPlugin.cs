using Microsoft.SemanticKernel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

#pragma warning disable IDE0130
namespace POWEREngineers.Bucky.POWEREngPlugins;
#pragma warning restore IDE0130

public class ModelRouterPlugin
{
    private const string MODELIDPROMPT = @"These are available data models and tools that a user might want to query information from or use:

    Power Delivery Quality Management System model (alias = PDQMS):
        The POWER Power Delivery Quality Management System (PDQMS) is a comprehensive framework designed to ensure the quality and effectiveness of Power Delivery projects. The PDQMS process consists of several high-level processes, including project initiation, design, QAQC (Quality Assurance/Quality Control), closeout, audit, reporting, and unlisted. The PDQMS process involves:
            1. Selecting a relevant high-level process from the list provided.
            2. Identifying QMS documents, processes, tools, workflows, and training that apply to the chosen process.
            3. Providing a description or name for an existing process that may need to be reviewed, refreshed, or removed.
            4. Summarizing the request by giving a brief summary of the process and any issues involved.
            5. Offering ideas for improvement or replacement of the mentioned processes.
            6. Including a Process Document URL that links to the current standard process documentation in a document management system.
        The PDQMS covers various aspects such as QAQC processes for different project types (e.g., distribution, transmission lines, substation), performance evaluation and customer satisfaction monitoring measures.  Use this model when a user requires information from POWER's PDQMS process and/or if they mention specific keywords related to quality management systems or ask about Power Delivery project processes and procedures related to quality assurance/control or customer satisfaction evaluations.

    POWER AUS proposal model (alias = POWERAUS):
        this model is meant to provide examples of previously written proposals, scopes, project information, and implementation details of work performed by POWER Engineer's AUS group for utility clients.  This work could be Utility Network (UN) Implementations, GIS and/or ESRI implementations and configurations, DERMS, DEERs and other distribution and ADMS system installation and configurations.  The AUS model will help users create new proposal and project documentation from these examples.

    POWER Substation KMP model (alias = PDSUBKMP):
        The POWER Engineers, Inc. (POWER) Knowledge Management Program (KMP), is a company-wide activity to promote and facilitate the sharing of technical information related to substation engineering and electrical engineering among all POWER staff. Knowledge management is 'An integrated, systematic approach to identifying, acquiring, transforming, developing, disseminating, using, sharing, and preserving knowledge, relevant to achieving specified objectives.' The KMP describes processes to perform substation design and electrical engineering for various clients, using design tools like AutoCad, Microstation, and many BIM tools like ACADE (AutoCad Electrical), Inventor, Bentley iTwin, Autodesk construction cloud, Autodesk Vault, and also explains how our clients share and define our CAD(computer Aided Drafting) and CAE(Computer aided engineering) standards and processes.The goal is to provide POWER users with a consistent vision to facilitate the following main objectives with the intended goal of increasing project efficiencies and quality:
        · Capture and share valuable project experiences and developments.This is also referred to as providing “feedback of value”, and is critical to the success of the KMP.
        · Transfer and share knowledge
        · Facilitate development of knowledgeable staff
        · Identify an efficient means to make our most knowledgeable staff available to clients when needed
        · Provide known tools to quickly locate reference information
        · Provide for cross-training and coordination opportunities between different Business Units and Divisions for common knowledge subjects
        Based on these objectives, the KMP includes three distinct components and describes the necessary interactions between each component. These components include:
        · Existing Knowledge about substation design and substation electrical engineering
        · Knowledge Transfer about substation design and substation electrical engineering
        · Project Support about substation design and substation electrical engineering
        A common database will serve as a repository for sharing information developed by each of the components noted above. The database consists of three components, the Library, the Wiki and the Forum.The Library is currently configured independently by Business Unit or Division and utilizes both ProjectWise and SharePoint. The goal is to ultimately condense our libraries into a single platform.The Wiki is a collection of open source websites maintained by each Division or Business Unit.The forum is a tool included in the new Portal tool(Interact) and will facilitate collaboration across KMP participants.

    DALL-E-3 Image generation(alias DALL-E):
        DALL-E is an image creation model hosted within our Azure OpenAI resource.If the user is requesting that an image or picture or JPEG or illustration or any other image format based on the provided inputs, this is the model to return.
    =========================

    Which model is this query asking for? If none match or you cannot determine the model, respond with UNKNOWN.  Otherwise, respond with the alias of the model.  Only respond with the chosen alias or UNKNOWN
    
    {{$intent}}

    Model: ";

    private readonly Kernel _kernel;

    public ModelRouterPlugin(Kernel kernel)
    {
        this._kernel = kernel;
    }

    public ModelRouterPlugin() { }

    [KernelFunction, Description("Routes the request to the appropriate data retrieval function.")]
    public async Task<string> RouteRequest(KernelArguments context)
    {
        string data = string.Empty;

        // TODO: this needs to be redone due to changes upstream regarding semantic functions
        // I reimpleneted this to deal with the routing below that was a semantic function previously
        // we should probably do something similar with the CADCopilotPlugin
        // check to see if we called this from AutoCAD and are using the AutoCAD Copilot
        /*
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
        */

        // Save the original user request
        var request = context["message"] as string;

        if (!string.IsNullOrEmpty(request))
        {
            // Retrieve the intent from the user request
            var modelIdentifier = this._kernel.CreateFunctionFromPrompt(
                new PromptTemplateConfig()
                {
                    Name = "ModelIdentifier",
                    Description = "Identifies the model the user is interested in based on the message in the request",
                    Template = MODELIDPROMPT,
                    TemplateFormat = "semantic-kernel",
                    InputVariables = new List<InputVariable>()
                    {
                        new() {Name = "intent", Description = "The users request.", IsRequired = true },
                    },
                }
            );
            var intent = await this._kernel.InvokeAsync(
                    modelIdentifier,
                    new() { { "intent", request } }
                );

            string model = intent.ToString();
            // Call the appropriate function
            switch (model)
            {
                case "PDQMS":
                    var qmsSkill = new QMSPlugin(this._kernel);
                    data = await qmsSkill.QueryQMSIndexAsync(request);
                    return $"PDQMS data: {data}";
                case "POWERAUS":
                    var ausSkill = new AUSPlugin(this._kernel);
                    data = await ausSkill.QueryAUSModelAsync(request);
                    return $"POWERAUS data: {data}";
                case "PDSUBKMP":
                    var kmpSkill = new KMPPlugin(this._kernel);
                    data = await kmpSkill.QueryKMPModelAsync(request);
                    return $"PDSUBKMP data: {data}";
                case "DALL-E":
                    var imageGenerationPlugin = new DALLEImageCreationPlugin(this._kernel);
                    data = await imageGenerationPlugin.CreateImage(request);
                    return $"DALL-E image data: {data}";
                case "UNKNOWN":
                default:
                    break;
            }
        }
        return data;
    }
}