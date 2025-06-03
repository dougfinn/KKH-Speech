using LLMUnity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class LLMUnityChat : MonoBehaviour
{
    [Header("LLM Character")]
    public LLMCharacter llmCharacter;

    [Header("UI Elements")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private ScrollRect scroll;

    [SerializeField] private RectTransform sent;
    [SerializeField] private RectTransform received;

    private float height;
    private List<string> chatHistory = new List<string>();

    private string systemPrompt; 

    private void Start()
    {
        LoadPromptFromJson();
        sendButton.onClick.AddListener(SendReply);
    }

    private void LoadPromptFromJson()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("AI_Prompt"); // name .json extension
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

    private void SendReply()
    {
        string userMessage = inputField.text.Trim();
        if (string.IsNullOrEmpty(userMessage)) return;

        AppendMessage(userMessage, true);
        chatHistory.Add("user: " + userMessage);

        sendButton.interactable = false;
        inputField.text = "";
        inputField.interactable = false;

        // Send message to LLMCharacter
        _ = llmCharacter.Chat(userMessage, HandleReply, ReplyCompleted);
    }

    private void HandleReply(string reply)
    {
        string displayMessage = Regex.Replace(reply, @"[\(\[].*?[\)\]]", "").Trim();
        AppendMessage(displayMessage, false);
        chatHistory.Add("assistant: " + displayMessage);
    }

    private void ReplyCompleted()
    {
        Debug.Log("Reply generation completed.");
        sendButton.interactable = true;
        inputField.interactable = true;
    }

    public class PromptData
    {
        public string prompt;
    }
}
