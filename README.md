# :speech_balloon: Dialog Node Based System (Unity Asset Tool) :speech_balloon:

**Last Updated:** 07/09/2025

**Publisher: cherrydev (Oleg Vishnivetsky)**

> 💡 **About**: The **Nodes-based Dialog System** is a Unity asset tool that enables game developers to create branching dialog trees for game characters with ease. It provides a visual editor to create and connect nodes, building dynamic conversations. Each node represents a piece of dialog, and connections between nodes determine the conversation flow.  
> The **Node Editor** presents an editable graph, displaying nodes and their connections, allowing you to create simple sentence nodes or answer nodes to craft your dialogs.

You can check the video tutorial on YouTube: 
[Basics](https://www.youtube.com/watch?v=oFOvop46eic&t=16s&ab_channel=cherrydev),
[New](https://youtu.be/881lzd-p9eg).

## 📋 Navigation

- [1. Node Editor 🗂️](#1️⃣-node-editor)
- [2. Variables 🧠](#2️⃣-variables)
- [3. Sentence Node 🗣️](#3️⃣-sentence-node)
- [4. Answer Node ✍️](#4️⃣-answer-node)
- [5. Modify Variable Node 🔧](#5️⃣-modify-variable-node)
- [6. Variable Condition Node 🔍](#6️⃣-variable-condition-node)
- [7. How to Use and Technical Part 💻](#7️⃣-how-to-use-and-technical-part)
- [8. Localization Integration 🌐](#8️⃣-localization-integration)
- [9. Timeline Integration ⏱️](#9️⃣-timeline-integration)
- [10. Tool Bar Navigation 🧭](#🔟-tool-bar-navigation)

## 1️⃣ Node Editor

### STEP 1: How to Open Node Editor Window

1. Right-click in the **Assets** folder.
2. Navigate to **Create > ScriptableObjects > Node Graph > Node Graph**.
3. Double-click your new **DialogNodeGraph** asset to open the editor.
4. Alternatively, go to **Window > DialogNodeBasedEditor**, but you still need to create and select a **NodeGraph** ScriptableObject.

### STEP 2: Orientation

#### Mouse Controls
1. **Left Mouse Button**:
   - On node: Select and move nodes.
   - On empty space + drag: Create a selection rectangle to select multiple nodes.
   - On empty space + click: Deselect all nodes.
2. **Middle Mouse Button**:
   - On empty space + drag: Pan/move around the editor view.
   - On node + drag: Create connections between nodes.
3. **Right Mouse Button**:
   - Click: Open context menu with options like "Create Sentence Node", "Create Answer Node", "Select All Nodes", "Remove Selected Nodes", and "Remove Connections".

## 2️⃣ Variables

The variable system allows dialogs to dynamically react to game state, player choices, or external triggers by reading and modifying values during conversations.

<img src="https://github.com/user-attachments/assets/fb7286f1-af70-470e-8c8a-1ee20e495136" alt="VariableConfig" width="300">

### Supported Variable Types
- **Bool**
- **Int**
- **Float**
- **String**

Variables are stored in a **Variable Config** (ScriptableObject). Create one and attach it to the **DialogNodeGraph**, or it will be automatically created when you add a variable node.

> 💡 Variables can be marked as "Save to Prefs" to persist changes in **PlayerPrefs**.

### Usage in Sentence/Answer Nodes
Variables can be embedded in dialog text using placeholders:
- Example: `"You have {coinCount} coins!"` — Use the variable name inside `{}`.
- Handled by `DialogTextProcessor.ProcessText(text, handler)` for automatic replacement during dialog display.

### Accessing/Getting Variables in Code
```csharp
int coins = dialogBehaviour.GetVariableValue<int>("coinCount");

dialogBehaviour.SetVariableValue("coinCount", 15);
dialogBehaviour.SetVariableValue("isDoorOpen", true);
```

## 3️⃣ Sentence Node

1. A **Sentence Node** connects to **one** parent and **one** child node.
2. Parameters:
   - **Name**
   - **Sentence**
   - **Sprite** (optional, can be left **null**).
3. Click the "**Add External Function**" button to add a field for a previously bound function (see [How to Use and Technical Part](#7️⃣-how-to-use-and-technical-part)).
4. Click the "**Remove External Function**" button to remove it.

## 4️⃣ Answer Node

1. An **Answer Node** can have **infinite** parent nodes, but the number of child nodes depends on the **number of answer options**. (Answer nodes cannot connect to other answer nodes.)
2. Click the "**Add Answer**" button to add another answer option (unlimited).
3. Delete the **last answer** option using the corresponding button.

## 5️⃣ Modify Variable Node

Used to **change** a variable's value during the dialog.

1. Select the target variable from the dropdown menu.
2. Choose the target action:
   - For **Boolean** variables:
     - **Set**: Directly set to true or false.
     - **Toggle**: Flip the current value.
   - For **Int/Float** variables:
     - **Set**: Set to a specific value.
     - **Increase**: Add a specified amount.
     - **Decrease**: Subtract a specified amount.
3. Enter the value or set the boolean status.

## 6️⃣ Variable Condition Node

Used to check variable values and branch dialog flow based on conditions.

1. Select the variable to check from the dropdown menu.
2. Set the comparison type:
   - For **Boolean**: `==`, `≠`.
   - For **Int/Float**: `==`, `≠`, `>`, `<`, `≥`, `≤`.
3. Enter the value to compare against.
4. Connect to different nodes for **TRUE** and **FALSE** outcomes.

## 7️⃣ How to Use and Technical Part

1. Drag and drop the **Dialog Prefab** from the **Prefab folder** and call the **StartDialog** method from the **DialogBehaviour** script attached to the prefab.

> 💡 It's recommended to call this method from another script, as shown in the demo script:

```csharp
public class TestDialogStarter : MonoBehaviour
{
    [SerializeField] private DialogBehaviour dialogBehaviour;
    [SerializeField] private DialogNodeGraph dialogGraph;

    private void Start()
    {
        dialogBehaviour.StartDialog(dialogGraph);
    }
}
```

**StartDialog** method from the **DialogBehaviour** script:
```csharp
public void StartDialog(DialogNodeGraph dialogNodeGraph)
{
    isDialogStarted = true;

    if (dialogNodeGraph.nodesList == null)
    {
        Debug.LogWarning("Dialog Graph's node list is empty");
        return;
    }

    onDialogStarted?.Invoke();

    currentNodeGraph = dialogNodeGraph;

    DefineFirstNode(dialogNodeGraph);
    CalculateMaxAmountOfAnswerButtons();
    HandleDialogGraphCurrentNode(currentNode);
}
```

2. Bind **external functions** for use in **Sentence Nodes** with the **BindExternalFunction** method, which takes a function name and the function itself.

> 💡 Bind external functions before calling them.

![External Function Example](https://github.com/OlegVishnivetsky/node-based-dialog-system/assets/98222611/ca9faeb7-23c2-4734-8bda-2de7a3417ab6)

```csharp
public void BindExternalFunction(string funcName, Action function);

public class TestDialogStarter : MonoBehaviour
{
    [SerializeField] private DialogBehaviour dialogBehaviour;
    [SerializeField] private DialogNodeGraph dialogGraph;

    private void Start()
    {
        dialogBehaviour.BindExternalFunction("Test", DebugExternal);
        dialogBehaviour.StartDialog(dialogGraph);
    }

    private void DebugExternal()
    {
        Debug.Log("External function works!");
    }
}
```

3. Configure the following **parameters** in the Inspector:
   - **Dialog Char Delay** (float): Delay before printing characters (text printing speed).
   - **Next Sentence Key Codes** (List<enum>): Keys to load the next sentence.
   - **Is Can Skipping Text** (bool): If true, pressing keys instantly prints the current sentence.
   - **OnDialogStarted** and **OnDialogEnded** events.

> 💡 These parameters can also be set in code.

![Inspector Parameters](https://github.com/OlegVishnivetsky/node-based-dialog-system/assets/98222611/7ec4fb1b-3a24-466e-b58c-976738d9eb18)

## 8️⃣ Localization Integration

This asset integrates with Unity's Localization system for multi-language support.

> 💡 The Unity Localization package is a dependency. If not needed, you can remove it.

### Setting Up Localization
1. In **Player Settings**:
   - Create **Locale Settings**.
   - Set up your desired **Locales**.
2. In the **Dialog Node Editor**, click **Localization > Set Up Localization**:
   - Creates a **Localization** folder in **Assets**.
   - Stores all localization data for each graph.
   - Automatically generates a **Localization Table Collection** with pre-configured entries.

### Edit Your Translations
- Add text for other languages in the generated **table collection**.
- The system uses **auto-generated keys**, but you can customize them.

### Managing Localization Keys
- Click the **Edit Table Keys** button to toggle key editing mode.
- Auto-generated keys may be random and less readable.
- Edit keys to be more descriptive, then click **Localization > Update Keys** to apply changes.

> ✅ Auto-generated keys work fine if you don't want to customize them.

## 9️⃣ Timeline Integration

Integrate **Dialog Behaviour** with Unity's Timeline system for cutscenes and scripted sequences.

### Setting Up Dialog in Timeline
1. **Create Dialog Behavior Track**: Add a new track in your Timeline and assign the **Dialog Behavior** component.
2. **Add Sentence Clips**: Create sentence clips on the track and assign sentence ScriptableObjects.
3. **Control Typing Speed**: The character typing speed is determined by the clip length (longer clips = slower typing).

### External Functions in Timeline
- Use **Call External Function Clip** to trigger bound functions at specific points.
- Ensure methods are **bound** before using them in timeline clips.
- Use the **exact method name** used during binding.

## 🔟 Tool Bar Navigation

The editor toolbar enhances your workflow with quick-access functions.

![Toolbar](https://github.com/user-attachments/assets/97524a86-aac9-4c89-b274-3218e1f759c7)

### 🔽 Nodes Dropdown
- Lists all nodes in your graph, prefixed with:
  - `S:` for **Sentence** nodes (shows the first part of the dialog text).
  - `A:` for **Answer** nodes (shows the first answer option).
- Click a node to **center** and **select** it in the graph.

### 🔍 Search Functionality
- Type text into the **search field** to locate nodes containing that text.
- Matching nodes are **automatically highlighted** as you type.
- Click the **Clear (×)** button to reset the search.

### 🧲 Find My Nodes
- Automatically centers the view on all nodes in your graph.
- Useful when nodes are **scattered** across the canvas.

### 🌐 Localization Tools
- **Edit Table Keys**: Toggle editing mode to customize localization keys. After editing, click **Localization > Update Keys**.
- **Localization**: Access all localization options from the dropdown.

---

:star::star::star::star::star: Feel free to edit the code to suit your needs. If you find bugs or have questions, contact me via email, GitHub, or Unity Asset Store reviews. I'd also love for you to visit my itch.io page! 😄

- **Email**: [olegmroleg@gmail.com](mailto:olegmroleg@gmail.com)
- **GitHub**: [OlegVishnivetsky](https://github.com/OlegVishnivetsky)
- **Itch.io**: [oleg-vishnivetsky.itch.io](https://oleg-vishnivetsky.itch.io/)
- **Unity Asset Store**: [Node-based Dialog System](https://assetstore.unity.com/packages/tools/game-toolkits/node-based-dialog-system-249962)

This README will be updated over time. Suggestions are welcome!
