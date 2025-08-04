using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.Net;

public class TTS_Google : MonoBehaviour
{
    [System.Serializable]
    private class ApiKeyData
    {
        public string apiKey;
    }

    [System.Serializable]
    private class TTSRequest
    {
        public TTSInput input;
        public TTSVoice voice;
        public TTSAudioConfig audioConfig;
    }

    [System.Serializable]
    private class TTSInput
    {
        public string text;
    }

    [System.Serializable]
    private class TTSVoice
    {
        public string languageCode;
        public string name;
    }

    [System.Serializable]
    private class TTSAudioConfig
    {
        public string audioEncoding;
    }

    [System.Serializable]
    private class TTSResponse
    {
        public string audioContent;
    }

    API_Keys api_Keys;
    private string TTS_Google_ApiKey;
    private string ttsUrl => $"https://texttospeech.googleapis.com/v1/text:synthesize?key={TTS_Google_ApiKey}";

    private AudioSource audioSource;

    public void Init()
    {   
        audioSource = gameObject.GetComponent<AudioSource>();
        //We first retrieve the API keys from the API Key component
        api_Keys = GetComponent<API_Keys>();
        if (!api_Keys)
            Debug.LogError("TTS_Google: Cannot find the API Keys component, please check the Inspector!");
        else TTS_Google_ApiKey = api_Keys.GetAPIKey("Google_API_Key_TTS");

        if(TTS_Google_ApiKey == null)
            Debug.LogWarning("TTS_Google: Warning: TTS API key is empty, check Inspector!");
    }

    public void Say(string inputText)
    {   
        Debug.Log("Speak() called with input: " + inputText);
        StartCoroutine(SendTTSRequest(inputText));
    }

    IEnumerator SendTTSRequest(string inputText)
    {
        var jsonRequest = new TTSRequest
        {
            input = new TTSInput { text = inputText },
            voice = new TTSVoice
            {
                languageCode = "en-US",
                name = "en-US-Wavenet-F" +
            ""
            },
            audioConfig = new TTSAudioConfig { audioEncoding = "LINEAR16" }
        };

        string jsonData = JsonUtility.ToJson(jsonRequest);
        Debug.Log("TTS JSON Payload: " + jsonData);

        UnityWebRequest www = new UnityWebRequest(ttsUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        Debug.Log("Sending TTS request...");
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("TTS request successful!");
            var response = JsonUtility.FromJson<TTSResponse>(www.downloadHandler.text);
            byte[] audioBytes = System.Convert.FromBase64String(response.audioContent);
            Debug.Log("Decoded audio bytes length: " + audioBytes.Length);

            var audioClip = WavUtility.ToAudioClip(audioBytes, "TTS");
            if (audioClip == null)
            {
                Debug.LogError("AudioClip is null! WavUtility might not be working.");
            }
            else
            {
                audioSource.clip = audioClip;
                audioSource.volume = 1.0f;
                audioSource.Play();

                StartCoroutine(RemoveClipAfterPlay(audioClip.length));
                Debug.Log("Playing audio via custom AudioSource...");
            }
        }
        else
        {
            Debug.LogError($"TTS Error: {www.error}");
            Debug.LogError($"TTS Full Error: {www.downloadHandler.text}");
        }
    }

    IEnumerator RemoveClipAfterPlay(float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSource.clip = null;
        Debug.Log("AudioClip removed from AudioSource.");
    }
}
