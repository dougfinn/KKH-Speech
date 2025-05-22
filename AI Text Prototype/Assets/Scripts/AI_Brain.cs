using OpenAI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class AI_Brain : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button button;
    [SerializeField] private ScrollRect scroll;

    [SerializeField] private RectTransform sent;
    [SerializeField] private RectTransform received;

    private float height;
    private OpenAIApi openai = new OpenAIApi();

    private List<ChatMessage> messages = new List<ChatMessage>();
    [TextArea(5, 10)] [SerializeField] private string prompt = "You are a 12 year old student in a school in Singapore. You are talking with your friend, Sally, who is also a 12 year old and autistic. You are talking about whether you should go for lunch or not and where. The options are the school canteen, or to go a coffee shop. Sometimes, sally finds it hard to respond and you may not understand what she says. You are supportive and helpful. Speak in an appropriate style for a young school girl. Sometimes, you may hesitate thinking about what to say, using Ah, Um, Yeh, etc. Basically, you are a young person talking to your friend. Be naturalistic and concise. Do not answer with more than 10 words.";

    public AI_TextToSpeech speech;
    public AI_SpeechToText speechToText;
    private void Start()
    {
        button.onClick.AddListener(SendReply);
        speech = GetComponent<AI_TextToSpeech>();
        speechToText = GetComponent<AI_SpeechToText>();
    }

    public void UseConvertedText()
    {
        string userSpeech = speechToText.ConvertedText;

        if (!string.IsNullOrEmpty(userSpeech))
        {
            inputField.text = userSpeech;
            SendReply();
        }
        else
        {
            Debug.LogWarning("Converted text is empty or null.");
        }
    }
    public void ChangeToText()
    {
        var newMessage = new ChatMessage()
        {
            Role = "user",
            Content = speechToText.ConvertedText
        };
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

            speech.Speak(message.Content);
        }
        else
        {
            Debug.LogWarning("No text was generated from this prompt.");
        }

        button.enabled = true;
        inputField.enabled = true;
    }
}
