using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Main placeholder for all AI related components
/// </summary>
public class AI_Orchestrator : MonoBehaviour
{
    [Header("Speech to Text")]
    [SerializeField] public STT_Groq_OpenAI sttGroqOpenAI;

    [Header("LLM")]
    [SerializeField] public LLM_Groq llmGroq;
    [SerializeField] public LLM_Google llmGoogle;
    [SerializeField] public LLM_Ollama llmOllama;

    [Header("Text to Speech")]
    [SerializeField] public TTS_11_Labs tts11Labs;
    [SerializeField] public TTS_Google ttsGoogle;

    [SerializeField] private string contextFilePath;
    private string initialContext;

    //[Header("Text to Mesh")]
    //[SerializeField] public TTM_Sloyd_API ttmSloyd;       //Deprecated 


    //This initializes all AI components AFTER the API keys were read from the APIKeys file
    public void Init()
    {
        LoadContext(); // Load the context from a JSON file in Resources
        if (llmGoogle) llmGoogle.Init(initialContext);
        if (llmGroq) llmGroq.Init(initialContext);
        if (llmOllama) llmOllama.Init(initialContext);

        if (sttGroqOpenAI) sttGroqOpenAI.Init();

        //if (ttmSloyd) ttmSloyd.Init();    Deprecated

        if (tts11Labs) tts11Labs.Init();
        if (ttsGoogle) ttsGoogle.Init();
    }


    //Generalized Say command - Expand here for new services!
    public void Say(string input)
    {
        if (tts11Labs) tts11Labs.Say(input);
        if (ttsGoogle) ttsGoogle.Say(input);
    }


    //Generalized TextToLLM command - Expand here for new services!
    public void TextToLLM(string input, string context)
    {
        if (llmGroq) llmGroq.TextToLLM(input, context);
        if (llmGoogle) llmGoogle.TextToLLM(input, context);
        if (llmOllama) llmOllama.TextToLLM(input, context);
    }


    /* Generalized TextToImage command - Expand here for new services!
    public void TextToImage(string input)
    {
        if (ttiHFSDXLB) ttiHFSDXLB.GetImage(input);
    }
    */


    /* Generalized TextToMesh commands - Expand here for new services!
    public void TTMCreate(string input)
    {
        //if (ttmSloyd)       ttmSloyd.Create(input);   //deprecated
    }

    public void TTMEdit(string input)
    {
        //if (ttmSloyd)       ttmSloyd.Edit(input);     //deprecated
    }

    public void TTMDelete()
    {
        //if (ttmSloyd)       ttmSloyd.Delete();        //deprecated
    }

    //Non-async call ro retrieve Context from a RAG database
    // - all RAG systems must implement a .GetContext method
    // - add new services here to ensure consistent calls via aiO.RAGGetContext
    public async Task<string> RAGGetContext(string prompt, int numberOfResults)
    {
        if (ragMariaDB)
        {
            return await ragMariaDB.GetContext(prompt, numberOfResults);
        }
        else return null;
    }

    //Check whether to use RAG or not 
    public bool RAGConfigured()
    {
        return (ragMariaDB == null ? false : true);
    }
    */


    //Event handlers for XR Interaction Toolkit Select Interactions
    // Moved from the individual STT components to the AI Orchestrator
    public void SelectEnterEventHandler(SelectEnterEventArgs eventArgs)
    {
        if (sttGroqOpenAI) sttGroqOpenAI.StartSpeaking();
    }

    public void SelectExitEventHandler(SelectExitEventArgs eventArgs)
    {
        Microphone.End(null);
    }

    private void LoadContext()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(contextFilePath); // .json file path name in Resources folder
        if (jsonFile != null)
        {
            PromptData promptData = JsonUtility.FromJson<PromptData>(jsonFile.text);
            initialContext = promptData.context;
            Debug.Log("Initial context loaded: " + initialContext);
        }
        else
        {
            Debug.LogError("Prompt JSON file not found in Resources.");
        }
    }

    public class PromptData
    {
        public string context;
    }

}
