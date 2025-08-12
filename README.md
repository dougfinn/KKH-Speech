# Social Skill Practice with VR

## Project Description

This project is developed in collaboration with **KK Women’s and Children’s Hospital** to support autistic children in overcoming socialization challenges.  
It provides an interactive, game-based environment where children can practice conversations and social interactions in a safe and comfortable space before applying these skills in real-life situations.

By using virtual characters and AI-driven dialogue, the system helps build confidence, reduce anxiety, and encourage gradual engagement with real people.  
Our ultimate goal is to bridge the gap between virtual practice and real-world social interactions, empowering autistic children to communicate more comfortably.

## Steps to get started
1. **Pull or Fork the Branch**  
   Clone the repository or fork it to your own GitHub account.

2. **Open the Project in Unity**  
   - Launch Unity Editor  
   - Go to **File → Open Scene → Classroom**

3. **Set Up NPC AI Keys**  
   If you want to talk with the NPC, obtain API keys from the following providers:  
   - **GroqCloud** (Free)  
   - **Google LLM** (Free)  
   - **Google TTS** (Free)  
   - **ElevenLabs** (Partially Free)

4. **Create API Keys File**  
   - Create a new folder: `Assets/Resources/Secure`  
   - Inside, create a file named `APIKeys.txt` with the following structure:  
     ```
     Google_API_Key:yourkeyhere
     Google_API_Key_TTS:yourkeyhere
     Groq_API_Key:yourkeyhere
     ElevenLabs_API_Key:yourkeyhere
     ```

5. **Register and Generate Keys**  
   Register with the AI services you plan to use and generate API keys.  
   At a minimum, you will need:  
   - Speech-to-Text provider  
   - LLM provider  
   - Text-to-Speech provider  

6. **Configure the AI Orchestrator**  
   - Select the **AI** GameObject in the **Hierarchy** panel  
   - In the **AI Orchestrator** component:  
     - Configure all AI components you want to use  
     - **Important:** Enable **only one** service per category (e.g., only one TTS provider to avoid overlapping voices)  

7. **Add or Remove AI Components (Optional)**  
   - All components are located in `Assets/Resources/Scripts/AI`  
   - Add or remove as needed for your project  

8. **Fix UI Event System**  
   - Remove any existing **Event System** in the scene; otherwise, UI may not work properly

9. **Edit Prompts**  
    - Modify prompt `.json` files as desired  
    - These are located under: `Assets/Resources/AI_Prompts`

10. **Reference the Prompt Path in LLM Components**  
    - In the **AI** GameObject, ensure all LLM components have the correct prompt file path set.

11. **Apply the Same Setup to the McDonald's Scene**  
   - Repeat steps above for the McDonald's scene to ensure consistent setup.

##During Play
- Press Grab Button to turn on microphone and interact with the AI.


