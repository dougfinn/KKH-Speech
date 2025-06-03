using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Text;
using System.Text.RegularExpressions;

public class OllamaChat : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button button;
    [SerializeField] private ScrollRect scroll;
    [SerializeField] private RectTransform sent;
    [SerializeField] private RectTransform received;

    private float height;

    private List<ChatMessage> messages = new List<ChatMessage>();

    private string prompt = "Act as a random introverted student in a classroom. Do not agree everything. Do not like everything. Do not always ask question. Don't break character. Don't ever mention that you are an AI model. Include only (happy, sad, neutral, doubtful) with bracket at the end. For example, I am a human. (happy)";
    [SerializeField] private string ollamaModel = "gemma2";

    private void Start()
    {
        LoadPromptFromJson();
        button.onClick.AddListener(SendReply);
    }

    private void LoadPromptFromJson()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("AI_Prompt"); // name .json extension
        if (jsonFile != null)
        {
            PromptData promptData = JsonUtility.FromJson<PromptData>(jsonFile.text);
            prompt = promptData.prompt;
        }
        else
        {
            Debug.LogError("Prompt JSON file not found in Resources.");
        }
    }

    private void AppendMessage(ChatMessage message)
    {
        scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);

        var item = Instantiate(message.Role == "user" ? sent : received, scroll.content);
        item.GetChild(0).GetChild(0).GetComponent<Text>().text = message.Content;
        item.anchoredPosition = new Vector2(0, -height);
        LayoutRebuilder.ForceRebuildLayoutImmediate(item);
        height += item.sizeDelta.y;
        scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        scroll.verticalNormalizedPosition = 0;
    }

    private void SendReply()
    {
        var userInput = inputField.text;
        if (string.IsNullOrEmpty(userInput)) return;

        var userMessage = new ChatMessage()
        {
            Role = "user",
            Content = userInput
        };

        AppendMessage(userMessage);

        if (messages.Count == 0)
        {
            userMessage.Content = prompt + "\n" + userInput;
        }

        messages.Add(userMessage);

        button.enabled = false;
        inputField.text = "";
        inputField.enabled = false;

        StartCoroutine(SendToOllama(userMessage.Content));
    }

    IEnumerator SendToOllama(string promptText)
    {
        string json = JsonUtility.ToJson(new OllamaRequest
        {
            model = ollamaModel,
            prompt = promptText,
            stream = true
        });

        using (UnityWebRequest request = new UnityWebRequest("http://localhost:11434/api/generate", "POST"))
        {
            byte[] postData = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(postData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Ollama error: " + request.error);
            }
            else
            {
                string rawResponse = request.downloadHandler.text;

                // Each response line is a separate JSON object
                string[] lines = rawResponse.Split('\n');
                StringBuilder finalResponse = new StringBuilder();

                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    try
                    {
                        OllamaResponse res = JsonUtility.FromJson<OllamaResponse>(line);
                        finalResponse.Append(res.response);
                    }
                    catch
                    {
                        Debug.LogWarning("Skipping invalid JSON line:\n" + line);
                    }
                }

                string cleanedText = Regex.Replace(finalResponse.ToString(), @"[\(\[].*?[\)\]]", "").Trim();

                var aiMessage = new ChatMessage()
                {
                    Role = "assistant",
                    Content = cleanedText
                };

                messages.Add(aiMessage);
                AppendMessage(aiMessage);
            }

            button.enabled = true;
            inputField.enabled = true;
        }
    }

    private string ExtractResponse(string json)
    {
        // Parse response JSON
        OllamaResponse response = JsonUtility.FromJson<OllamaResponse>(json);
        return response.response;
    }

    [System.Serializable]
    public class ChatMessage
    {
        public string Role;
        public string Content;
    }

    [System.Serializable]
    public class OllamaRequest
    {
        public string model;
        public string prompt;
        public bool stream;
    }

    [System.Serializable]
    public class OllamaResponse
    {
        public string response;
        public bool done;
    }

    public class PromptData
    {
        public string prompt;
    }
}