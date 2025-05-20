using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class HuggingFaceAPI
{
    private static readonly string API_URL = "https://api-inference.huggingface.co/models/openai/whisper-small";

    public static async Task<string> SendAudio(byte[] audioBytes, string apiKey)
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            using (var content = new ByteArrayContent(audioBytes))
            {
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("audio/wav");

                HttpResponseMessage response = await client.PostAsync(API_URL, content);
                string result = await response.Content.ReadAsStringAsync();

                return result;
            }
        }
    }
}
