using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace POWEREngineers.Bucky.POWEREngPlugins;

public class QMSPlugin
{
    private Kernel _kernel;
    public QMSPlugin(Kernel kernel)
    {
        this._kernel = kernel;
    }

    public QMSPlugin() { }

    /// <summary>
    /// Query the QMS Memory collection for documents that match the query.
    /// </summary>
    /// <param name="query">Query to match.</param>
    /// <param name="context">The SkContext.</param>
    [KernelFunction, Description("Call the POWER QMS Data prompt-flow endpoint to get information")]
    //[SKParameter("query", "Query to match.")]
    public async Task<string> QueryQMSIndexAsync(string query)
    {
        //short-circuit this for now till the backend gets fixed
        //return string.Empty;
        string qmsResult = string.Empty;
        var handler = new HttpClientHandler()
        {
            ClientCertificateOptions = ClientCertificateOption.Manual,
            ServerCertificateCustomValidationCallback =
                    (_, cert, cetChain, policyErrors) => true
        };
        using (var client = new HttpClient(handler))
        {
            // More information can be found here:
            // https://docs.microsoft.com/azure/machine-learning/how-to-deploy-advanced-entry-script
            var requestBody = $"{{{query}}}";

            // Replace this with the primary/secondary key or AMLToken for the endpoint
            const string apiKey = "YFvib3wRerkjFbmYK0Nell2z8A416c0C";

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            client.BaseAddress = new Uri("https://pd-qms.eastus.inference.ml.azure.com/score");

            var content = new StringContent(requestBody);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // This header will force the request to go to a specific deployment.
            // Remove this line to have the request observe the endpoint traffic rules
            content.Headers.Add("azureml-model-deployment", "blue");

            HttpResponseMessage response = await client.PostAsync("", content);

            if (response.IsSuccessStatusCode)
            {
                qmsResult = await response.Content.ReadAsStringAsync();
            }
        }
        return $"{qmsResult}";
    }
}