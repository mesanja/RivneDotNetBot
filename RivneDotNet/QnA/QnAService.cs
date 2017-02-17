using System;
using System.Net;
using Newtonsoft.Json;

namespace RivneDotNet.QnA
{
    public static class QnAService
    {
        public static QnAMakerResult GetAnswer(string query)
        {
            var rawAnswer = GetRawAnswer(query);

            QnAMakerResult response;
            try
            {
                response = JsonConvert.DeserializeObject<QnAMakerResult>(rawAnswer);
            }
            catch
            {
                throw new Exception("Unable to deserialize QnA Maker response string.");
            }
            return response;
        }

        public static string GetRawAnswer(string query)
        {
            try
            {
                var knowledgebaseId = "";       // Use knowledge base id created.
                var qnamakerSubscriptionKey = "";   // Use subscription key assigned to you.

                //Build the URI
                Uri qnamakerUriBase = new Uri("https://westus.api.cognitive.microsoft.com/qnamaker/v1.0");
                var builder = new UriBuilder($"{qnamakerUriBase}/knowledgebases/{knowledgebaseId}/generateAnswer");

                //Add the question as part of the body
                var postBody = $"{{\"question\": \"{query}\"}}";

                //Send the POST request
                using (WebClient client = new WebClient())
                {
                    //Set the encoding to UTF8
                    client.Encoding = System.Text.Encoding.UTF8;

                    //Add the subscription key header
                    client.Headers.Add("Ocp-Apim-Subscription-Key", qnamakerSubscriptionKey);
                    client.Headers.Add("Content-Type", "application/json");
                    return client.UploadString(builder.Uri, postBody);
                }
            }
            catch (Exception)
            {
                // it's better to handle me
                throw;
            }
        }
    }
}