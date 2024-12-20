using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace NoteBrain.Sink
{
    public class AIBrain
    {
        public async Task DoWorkAsync(string key, string fileName, CancellationToken cancellationToken)
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


            var bytes = File.ReadAllBytes(fileName);

            // Add system message
            var chatHistory = new ChatHistory(systemPrompt);

            chatHistory.AddUserMessage(new ChatMessageContentItemCollection
            {
                new TextContent(userInput),
                new ImageContent(bytes, "image/png")
            });

            var settings = new OpenAIPromptExecutionSettings()
            {
                Temperature = 0d
            };

            var reply = await chatCompletionService.GetChatMessageContentAsync(chatHistory, settings);


            var sb = new StringBuilder();
            reply.Items.ToList().ForEach(item =>
            {
                if (item is TextContent textContent)
                {
                    Console.WriteLine(textContent.Text);
                    sb.AppendLine(textContent.Text);
                }
            });

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
