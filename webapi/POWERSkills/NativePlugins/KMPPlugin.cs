using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace POWEREngineers.Bucky.POWEREngPlugins;

public class KMPPlugin
{
    private Kernel _kernel;
    public KMPPlugin(Kernel kernel)
    {
        this._kernel = kernel;
    }

    public KMPPlugin() { }

    /// <summary>
    /// Query the AUS search index for documents that match the query.
    /// </summary>
    /// <param name="query">Query to match.</param>
    /// <param name="context">The SkContext.</param>
    [KernelFunction, Description("Call the POWER Substation KMP ML Model to get information per the users context")]
    //[SKParameter("query", "Query to match.")]
    public async Task<string> QueryKMPModelAsync(string query)
    {
        string kmpResult = string.Empty;
        var handler = new HttpClientHandler()
        {
            ClientCertificateOptions = ClientCertificateOption.Manual,
            ServerCertificateCustomValidationCallback =
                    (httpRequestMessage, cert, cetChain, policyErrors) => { return true; }
        };

        using (var client = new HttpClient(handler))
        {
            var requestBody = $"{{{query}}}";

            // Replace this with the primary/secondary key or AMLToken for the endpoint
            const string apiKey = "9OfW8valhIdMfuurFVthKPc5JzbvSgBC";
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("A key should be provided to invoke the endpoint");
            }
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            client.BaseAddress = new Uri("https://pd-substation-kmp.eastus.inference.ml.azure.com/score");

            var content = new StringContent(requestBody);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            content.Headers.Add("azureml-model-deployment", "blue");

            HttpResponseMessage response = await client.PostAsync("", content);

            if (response.IsSuccessStatusCode)
            {
                kmpResult = await response.Content.ReadAsStringAsync();
            }
        }
        return $"{kmpResult}";
    }
}