using LLMUnity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;


public class LLMChat : MonoBehaviour
{
    [Header("LLM Character")]
    public LLMCharacter llmCharacter;

    [Header("UI Elements")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private ScrollRect scroll;
    [SerializeField] private Button recordButton;

    [SerializeField] private RectTransform sent;
    [SerializeField] private RectTransform received;

    private float height;
    private List<string> chatHistory = new List<string>();

    public AI_TextToSpeech speech;

    private string systemPrompt; 

    private void Start()
    {
        LoadPromptFromJson();
        sendButton.onClick.AddListener(SendMessage);
        inputField.onSubmit.AddListener(CallMethod);
        speech = GetComponent<AI_TextToSpeech>();
    }


    private void LoadPromptFromJson()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("AI_Prompts/AI_Prompt"); // .json file path in Resources folder
        if (jsonFile != null)
        {
            PromptData promptData = JsonUtility.FromJson<PromptData>(jsonFile.text);
            systemPrompt = promptData.prompt;
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


    private void AppendMessage(string content, bool isUser)
    {
        scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);

        var item = Instantiate(isUser ? sent : received, scroll.content);
        item.GetChild(0).GetChild(0).GetComponent<Text>().text = content;
        item.anchoredPosition = new Vector2(0, -height);
        LayoutRebuilder.ForceRebuildLayoutImmediate(item);
        height += item.sizeDelta.y;
        scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        scroll.verticalNormalizedPosition = 0;
    }

    private void SendMessage()
    {
        string userMessage = inputField.text.Trim();
        if (string.IsNullOrEmpty(userMessage)) return;

        AppendMessage(userMessage, true);

        if (chatHistory.Count == 0) userMessage = systemPrompt + "\n" + inputField.text;
        chatHistory.Add("user: " + userMessage);

        sendButton.interactable = false;
        inputField.text = "";
        inputField.interactable = false;
        recordButton.interactable = false;

        // Send message to LLMCharacter
        _ = llmCharacter.Chat(userMessage, HandleReply, ReplyCompleted);
    }

    public void SendFromMicrophone(string transcribedText)
    {
        if (string.IsNullOrWhiteSpace(transcribedText)) return;

        AppendMessage(transcribedText, true);

        if (chatHistory.Count == 0) transcribedText = systemPrompt + "\n" + transcribedText;
        chatHistory.Add("user: " + transcribedText);

        sendButton.interactable = false;
        inputField.text = "";
        inputField.interactable = false;
        recordButton.interactable = false;

        _ = llmCharacter.Chat(transcribedText, HandleReply, ReplyCompleted);
    }

    private void HandleReply(string reply)
    {
        string displayMessage = Regex.Replace(reply, @"[\(\[].*?[\)\]]", "").Trim();      // Remove text in parentheses or brackets
        AppendMessage(displayMessage, false);
        chatHistory.Add("assistant: " + displayMessage);
        speech.Speak(displayMessage);
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
