using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace NoteBrain.Sink
{
    public class AIBrain
    {
        public async Task DoWorkAsync(string key, string[] fileNames, CancellationToken cancellationToken)
        {
            //Create a kernel with gpt-4 vision model
            var kernel = Kernel.CreateBuilder()
                        .AddOpenAIChatCompletion("gpt-4o", key)
                        .Build();

            // Get the chat service
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            string systemPrompt = "You are a friendly assistant that helps decribe images.";
            string userInput = @"
Please extract the text from the image, also do it in markdown if possible
Addionally, create a json file with the following information

- Title (key:title)
- If available date of when the notes where taken (key:date)
- List of action items (key:actionItems)
-- description of the item (key:description)
-- due date of the item (key:dueDate)
-- status of the item (key:status)
-- priority of the item (key:priority)
-- assignee of the item (key:assignee)
";

            userInput = "Tell me a joke";
            

            var collection = new ChatMessageContentItemCollection();
            collection.Add(new TextContent(userInput));
            foreach (var file in fileNames)
            {
                var bytes = File.ReadAllBytes(file);
                var type = new FileInfo(file).Extension switch
                {
                    ".png" => "image/png",
                    ".jpg" => "image/jpeg",
                    ".jpeg" => "image/jpeg",
                    ".gif" => "image/gif",
                    ".bmp" => "image/bmp",
                    ".pdf" => "application/pdf",
                    _ => "image/png"
                };
                collection.Add(new ImageContent(bytes, type));
            }

            // Add system message
            var chatHistory = new ChatHistory(systemPrompt);

            chatHistory.AddUserMessage(collection);

            var settings = new PromptExecutionSettings()
            {
                ExtensionData = new Dictionary<string, object>() {
                    { "temperature", 0.7 }
                }
            };

            var sb = new StringBuilder();

            //var reply = await chatCompletionService.GetChatMessageContentAsync(chatHistory, settings);


            //reply.Items.ToList().ForEach(item =>
            //{
            //    if (item is TextContent textContent)
            //    {
            //        Console.WriteLine(textContent.Text);
            //        sb.AppendLine(textContent.Text);
            //    }
            //});

            //reply.Metadata?.ToList().ForEach(item =>
            //{
            //    Console.WriteLine(item.Key + " : " + item.Value);
            //});

            //var items = new List<StreamingChatMessageContent>();
            //StreamingChatMessageContent? lastContent = null;
            //await foreach (var content in chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory, settings, null, cancellationToken))
            //{
            //    items.Add(content);
            //    Console.Write(content.Content);
            //    sb.Append(content.Content);
            //    Debug.WriteLineIf(content.Metadata != null && content.Metadata.ContainsKey("FinishReason"),
            //        content.Metadata["FinishReason"]);
            //    lastContent = content;
            //}

            var response = await chatCompletionService.GetChatMessageContentAsync(chatHistory, settings, null, cancellationToken);
            foreach (var item in response.Items)
            {
                if (item is TextContent textContent)
                {
                    Console.WriteLine(textContent.Text);
                    sb.AppendLine(textContent.Text);
                }
            }




            //var output = JsonSerializer.Serialize(items, new JsonSerializerOptions
            //{
            //    WriteIndented = true,
            //    ReferenceHandler = ReferenceHandler.IgnoreCycles
            //});

            //File.WriteAllText("output.json", output);

            var json = ExtractJsonFromText(sb.ToString());
        }

        public string ExtractJsonFromText(string text)
        {
            var jsonPattern = @"\{(?:[^{}]|(?<open>\{)|(?<-open>\}))+(?(open)(?!))\}";
            var match = Regex.Match(text, jsonPattern, RegexOptions.Singleline);
            return match.Success ? match.Value : string.Empty;
        }
    }
}
