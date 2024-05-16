using Azure.AI.OpenAI;
using Azure;

var apiBase = "https://{YOUR_URL_IN_HERE}.openai.azure.com/";
var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
var deploymentName = "gpt3turbo"; 

var searchEndpoint = "https://rag-test-ai-search.search.windows.net";
var searchKey = Environment.GetEnvironmentVariable("SEARCH_KEY"); 
var searchIndexName = "azureblob-index"; 
var client = new OpenAIClient(new Uri(apiBase), new AzureKeyCredential(apiKey!));

Console.WriteLine("Ask something");
var prompt = Console.ReadLine();

var chatCompletionsOptions = new ChatCompletionsOptions()
{
    Messages =
    {
        new ChatRequestUserMessage(prompt)
    },
    AzureExtensionsOptions = new AzureChatExtensionsOptions()
    {
        Extensions =
        {
            new AzureCognitiveSearchChatExtensionConfiguration()
            {
                SearchEndpoint = new Uri(searchEndpoint),
                IndexName = searchIndexName,
                Key = searchKey,
                QueryType = AzureCognitiveSearchQueryType.Simple,
            },
        },
    },
    DeploymentName = deploymentName,
    MaxTokens = 800,
    Temperature = 0,
};

var response = await client.GetChatCompletionsAsync(chatCompletionsOptions);

var message = response.Value.Choices[0].Message;

Console.WriteLine($"{message.Role}: {message.Content}");

Console.WriteLine($"Citations and other information:");

foreach (var contextMessage in message.AzureExtensionsContext.Messages)
{
    // Note: citations and other extension payloads from the "tool" role are often encoded JSON documents
    // and need to be parsed as such; that step is omitted here for brevity.
    Console.WriteLine($"{contextMessage.Role}: {contextMessage.Content}");
}

