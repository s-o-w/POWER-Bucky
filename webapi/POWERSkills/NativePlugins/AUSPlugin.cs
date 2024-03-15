using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System;
using System.Text;
using Azure;
using Azure.Search.Documents;
using Azure.AI.OpenAI;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace POWEREngineers.Bucky.POWEREngPlugins;

public class AUSPlugin
{
    private readonly Kernel _kernel;
    public AUSPlugin(Kernel kernel)
    {
        this._kernel = kernel;
    }

    public AUSPlugin() { }

    /// <summary>
    /// Query the AUS search index for documents that match the query.
    /// </summary>
    /// <param name="query">Query to match.</param>
    /// <param name="context">The SkContext.</param>
    [KernelFunction, Description("Call the POWER AUS Vector index to get information per the users context")]
    //[SKParameter("query", "Query to match.")]
    public async Task<string> QueryAUSIndexAsync(string query)
    {
        //return String.Empty;
        StringBuilder ausResult = new();

        // Initialize OpenAI client  
        var credential = new AzureKeyCredential("cafc0262e9d046739da7784b6e91f6c7");
        var openAIClient = new OpenAIClient(new Uri("https://pe-oai.openai.azure.com/"), credential);

        // Initialize Azure Cognitive Search clients  
        var searchCredential = new AzureKeyCredential("1rTVexZVHwqU375AM9wEbcliBBU6fdRUsO278bRcDNAzSeA1lDxH");
        var indexClient = new SearchIndexClient(new Uri("https://pe-cognativesearch.search.windows.net"), searchCredential);
        var searchClient = indexClient.GetSearchClient("power-aus-e");

        try
        {
            //var embeddingResponse = await openAIClient.GetEmbeddingsAsync("text-embedding-ada-002", new EmbeddingsOptions(query));
            //var embedding = embeddingResponse.Value.Data.First().Embedding.ToArray();
            // Perform the vector similarity search    
            //var vector = new SearchQueryVector { K = 3, Fields = "TextVector", Value = embedding };
            var searchOptions = new SearchOptions
            {
                //vector = vector,
                Size = 10,
                QueryType = SearchQueryType.Semantic,
                Select = { "id", "Description", "Text", "ExternalSourceName" },
            };

            SearchResults<SearchDocument> response = await searchClient.SearchAsync<SearchDocument>(query, searchOptions);

            await foreach (SearchResult<SearchDocument> result in response.GetResultsAsync())
            {
                ausResult.AppendLine($"id: {result.Document["id"]}");
                ausResult.AppendLine($"Description: {result.Document["Description"]}");
                ausResult.AppendLine($"Text: {result.Document["Text"]}");
                ausResult.AppendLine($"Source: {result.Document["ExternalSourceName"]}");
            }
        }
        catch (NullReferenceException)
        {
            Console.WriteLine("Total AUS Results: 0");
        }
        return $"{ausResult.ToString()}";
    }

    /// <summary>
    /// Query the AUS search index for documents that match the query.
    /// </summary>
    /// <param name="query">Query to match.</param>
    /// <param name="context">The SkContext.</param>
    [KernelFunction, Description("Call the POWER AUS ML Model to get information per the users context")]
    //[SKParameter("query", "Query to match.")]
    public async Task<string> QueryAUSModelAsync(string query)
    {
        string ausResult = string.Empty;
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
            const string apiKey = "yu3OGl3p0tVhiabMJBEniVSFU1yeSA8h";
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("A key should be provided to invoke the endpoint");
            }
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            client.BaseAddress = new Uri("https://power-aus-gold.eastus.inference.ml.azure.com/score");

            var content = new StringContent(requestBody);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            content.Headers.Add("azureml-model-deployment", "blue");

            HttpResponseMessage response = await client.PostAsync("", content);

            if (response.IsSuccessStatusCode)
            {
                ausResult = await response.Content.ReadAsStringAsync();
            }
        }
        return $"{ausResult}";
    }
}