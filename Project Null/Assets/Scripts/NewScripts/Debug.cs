using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugMenu : MonoBehaviour
{
    [Header("Scenes")]
    [Tooltip("List of scene names to switch between (must match Build Settings exactly)")]
    public List<string> sceneNames = new List<string>();

    private bool _menuOpen = false;
    private Rect _windowRect = new Rect(400, 400, 400, 120);
    private bool _stylesInit = false;
    private GUIStyle _windowStyle;
    private GUIStyle _buttonStyle;
    private GUIStyle _labelStyle;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            _menuOpen = !_menuOpen;
    }

    private void OnGUI()
    {
        if (!_menuOpen) return;

        InitStyles();

        // Dynamically resize window height based on number of scenes
        _windowRect.height = 50 + (sceneNames.Count * 36);
        _windowRect = GUI.Window(0, _windowRect, DrawWindow, "  🛠  Scene Switcher  [F1]", _windowStyle);
    }

    private void DrawWindow(int id)
    {
        GUILayout.Space(5);

        if (sceneNames.Count == 0)
        {
            GUILayout.Label("No scenes added. Fill 'Scene Names' in the Inspector.", _labelStyle);
        }
        else
        {
            foreach (string sceneName in sceneNames)
            {
                if (GUILayout.Button(sceneName, _buttonStyle))
                {
                    _menuOpen = false;
                    SceneManager.LoadScene(sceneName);
                }
            }
        }

        GUI.DragWindow();
    }

    private void InitStyles()
    {
        if (_stylesInit) return;

        _windowStyle = new GUIStyle(GUI.skin.window)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold
        };

        _buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize =36,
            fixedHeight = 45,
            margin = new RectOffset(5, 5, 3, 3)
        };

        _labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11,
            normal = { textColor = Color.yellow }
        };

        _stylesInit = true;
    }
}