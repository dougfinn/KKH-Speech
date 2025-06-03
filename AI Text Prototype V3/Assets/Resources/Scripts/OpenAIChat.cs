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
    [SerializeField] private Button sendButton;
    [SerializeField] private Button recordButton;
    [SerializeField] private ScrollRect scroll;

    [SerializeField] private RectTransform sent;
    [SerializeField] private RectTransform received;

    private float height;
    private OpenAIApi openai = new OpenAIApi();

    private List<ChatMessage> chatHistory = new List<ChatMessage>();

    public AI_TextToSpeech speech;

    private string prompt;

    public static OpenAIChat Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        LoadPromptFromJson();
        sendButton.onClick.AddListener(SendMessage);
        inputField.onSubmit.AddListener(CallMethod);
        speech = GetComponent<AI_TextToSpeech>();
    }

    private void LoadPromptFromJson()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("AI_Prompts/AI_Prompt"); // .json file path name in Resources folder
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


    private void CallMethod(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        SendMessage();
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


    private void SendMessage()
    {
        if (string.IsNullOrEmpty(inputField.text)) return;

        // Display original user message
        AppendMessage(new ChatMessage()
        {
            Role = "user",
            Content = inputField.text
        });

        // Prepare message for OpenAI (prepend prompt only once)
        var actualContent = chatHistory.Count == 0 ? prompt + "\n" + inputField.text : inputField.text;

        chatHistory.Add(new ChatMessage()
        {
            Role = "user",
            Content = actualContent
        });
        Debug.Log($"ChatHistory Count: {chatHistory.Count}");

        sendButton.interactable = false;
        inputField.text = "";
        inputField.interactable = false;
        recordButton.interactable = false;

        HandleReply();
    }


    public void SendFromMicrophone(string transcribedText)
    {
        if (string.IsNullOrWhiteSpace(transcribedText)) return;
        Debug.Log("Send From Microphone");
        // Display original transcribed message
        AppendMessage(new ChatMessage()
        {
            Role = "user",
            Content = transcribedText
        });

        // Prepare message for OpenAI (prepend prompt only once)
        var actualContent = chatHistory.Count == 0 ? prompt + "\n" + transcribedText : transcribedText;

        chatHistory.Add(new ChatMessage()
        {
            Role = "user",
            Content = actualContent
        });
        Debug.Log($"ChatHistory Count: {chatHistory.Count}");

        sendButton.interactable = false;
        inputField.text = "";
        inputField.interactable = false;
        recordButton.interactable = false;

        HandleReply();
    }


    private async void HandleReply()
    {
        // Complete the instruction
        var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
        {
            Model = "gpt-4o-mini",
            Messages = chatHistory            // Add the chat history to the request
        });

        if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
        {
            var message = completionResponse.Choices[0].Message;
            string displayMessage = Regex.Replace(message.Content, @"[\(\[].*?[\)\]]", "");    //remove the text in brackets
            message.Content = displayMessage.Trim();

            chatHistory.Add(message);
            Debug.Log($"ChatHistory Count: {chatHistory.Count}");
            AppendMessage(message);
            speech.Speak(message.Content); // Call the Speak method to convert text to speech
        }
        else
        {
            Debug.LogWarning("No text was generated from this prompt.");
        }

        ReplyCompleted();
    }


    private void ReplyCompleted()
    {
        sendButton.interactable = true;
        inputField.interactable = true;
        recordButton.interactable = true;
    }


    public class PromptData
    {
        public string prompt;
    }
}
