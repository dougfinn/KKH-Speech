using OpenAI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class OpenAIChat : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button button;
    [SerializeField] private ScrollRect scroll;

    [SerializeField] private RectTransform sent;
    [SerializeField] private RectTransform received;

    private float height;
    private OpenAIApi openai = new OpenAIApi();

    private List<ChatMessage> messages = new List<ChatMessage>();
    private string prompt = "Act as a random introverted student in a classroom. Do not agree everything. Do not like everything. Do not always ask question. Don't break character. Don't ever mention that you are an AI model. Include only (happy, sad, neutral, doubtful) with bracket at the end. For example, I am a human. (happy)";

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

    private async void SendReply()
    {
        var newMessage = new ChatMessage()
        {
            Role = "user",
            Content = inputField.text
        };

        AppendMessage(newMessage);

        if (messages.Count == 0) newMessage.Content = prompt + "\n" + inputField.text;

        messages.Add(newMessage);

        button.enabled = false;
        inputField.text = "";
        inputField.enabled = false;

        // Complete the instruction
        var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
        {
            Model = "gpt-4o-mini",
            Messages = messages
        });

        if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
        {
            var message = completionResponse.Choices[0].Message;
            string displayMessage = Regex.Replace(message.Content, @"[\(\[].*?[\)\]]", "");
            message.Content = displayMessage.Trim();

            messages.Add(message);
            AppendMessage(message);
        }
        else
        {
            Debug.LogWarning("No text was generated from this prompt.");
        }

        button.enabled = true;
        inputField.enabled = true;
    }

    public class PromptData
    {
        public string prompt;
    }
}
