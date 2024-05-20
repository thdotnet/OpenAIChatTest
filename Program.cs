using Azure;
using Azure.AI.OpenAI;

var apiBase = "https://{YOUR_URL_IN_HERE}.openai.azure.com/";
var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
var deploymentName = "gpt3turbo";

var searchEndpoint = "https://{{YOUR_SEARCH_SERVICE_IN_HERE}}.search.windows.net";
var searchKey = Environment.GetEnvironmentVariable("SEARCH_KEY"); 
var searchIndexName = "azureblob-index"; 
var client = new OpenAIClient(new Uri(apiBase), new AzureKeyCredential(apiKey!));

var interactionHistory = new List<Interaction>();

while (true)
{
    Console.WriteLine("Ask something");
    var prompt = Console.ReadLine();

    interactionHistory.Add(new Interaction { Role = "user", Content = prompt });

    var chatMessages = new List<ChatRequestUserMessage>();
    foreach (var interaction in interactionHistory)
    {
        chatMessages.Add(new ChatRequestUserMessage(interaction.Content)
        {
            Role = interaction.Role,
        });
    }

    var chatCompletionsOptions = new ChatCompletionsOptions()
    {
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

    foreach (var interaction in interactionHistory)
    {
        chatCompletionsOptions.Messages.Add(new ChatRequestUserMessage(interaction.Content)
        {
            Role = interaction.Role,
        });
    }

    var response = await client.GetChatCompletionsAsync(chatCompletionsOptions);

    var message = response.Value.Choices[0].Message;

    interactionHistory.Add(new Interaction { Role = "system", Content = message.Content });

   
    Console.WriteLine($"{message.Role}: {message.Content}");

    Console.WriteLine($"Citations and other information:");

    foreach (var contextMessage in message.AzureExtensionsContext.Messages)
    {
        Console.WriteLine($"{contextMessage.Role}: {contextMessage.Content}");
    }
    //line breaks
    Console.WriteLine("");
    Console.WriteLine("");
}

public class Interaction
{
    public string Role { get; set; }
    public string Content { get; set; }
}