using System.Threading.Tasks;
using System;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using Azure.AI.OpenAI;
using Azure;

namespace POWEREngineers.Bucky.POWEREngPlugins;

public class DALLEImageCreationPlugin
{
    private readonly Kernel _kernel;
    public DALLEImageCreationPlugin(Kernel kernel)
    {
        this._kernel = kernel;
    }

    public DALLEImageCreationPlugin() { }

    /// <summary>
    /// Query the AUS search index for documents that match the query.
    /// </summary>
    /// <param name="query">Query to match.</param>
    /// <param name="context">The SkContext.</param>
    [KernelFunction, Description("Call DALL-E to generate an image based on the provided request/prompt")]
    //[SKParameter("query", "Query to match.")]
    public async Task<string> CreateImage(string request)
    {
        string endpoint = "https://bucky-oai.openai.azure.com/";
        string key = "5e024c98d45641d49567b790add37410";

        OpenAIClient client = new(new Uri(endpoint), new AzureKeyCredential(key));

        Response<ImageGenerations> imageGenerations = await client.GetImageGenerationsAsync(
            new ImageGenerationOptions()
            {
                Prompt = request,
                DeploymentName = "dall-e-3"
            }
        );

        // Image Generations responses provide URLs you can use to retrieve requested images
        Uri imageUri = imageGenerations.Value.Data[0].Url;
        return imageUri.ToString();
    }
}