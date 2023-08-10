using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script managing the creation of logs in the error/debug log console. Console can be opened and closed by pressing 'F1'.
/// </summary>
public class LogMessagesManager : MonoBehaviour
{

    public static LogMessagesManager Instance { get; private set; }

    public GameObject LogPanel;

    public GameObject LogEntryPrefab;

    public UnityEngine.UI.ScrollRect ScrollRect;

    public Transform ContentContainer;

    void Awake()
    {
        Instance = this;
        Application.logMessageReceived += OnLogMessageReceived;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            LogPanel.SetActive(!LogPanel.activeSelf);
        }
    }

    public void OpenLogPanel()
    {
        LogPanel.SetActive(true);
    }

    public void CloseLogPanel()
    {
        LogPanel.SetActive(false);
    }

    /// <summary>
    /// Callback for unity's Application.logMessageReceived event. Creates log entry from prefab and displays it in
    /// log console.
    /// </summary>
    /// <param name="logString">String of the log-message.</param>
    /// <param name="stackTrace"></param>
    /// <param name="type">Type of log-message, whether its e.g. an error, a warning, just a debug log etc...</param>
    void OnLogMessageReceived(string logString, string stackTrace, LogType type)
    {
        var color = new Color(1,1,1);
        switch (type)
        {
            case LogType.Error:
                color = new Color(1, 0, 0);
                break;
            case LogType.Assert:
                break;
            case LogType.Warning:
                color = new Color(1, 1, 0);
                break;
            case LogType.Log:
                break;
            case LogType.Exception:
                color = new Color(1, 0, 0);
                break;
            default:
                break;
        }

        var itemInstance = Instantiate(LogEntryPrefab);

        var text_label = itemInstance.GetComponent<TMPro.TMP_Text>();

        text_label.text = "[" + DateTime.Now.ToString("h:mm:ss tt") + "] " + logString;
        text_label.color = color;
        //parent the item to the content container
        itemInstance.transform.SetParent(ContentContainer);
        //reset the item's scale -- this can get munged with UI prefabs
        itemInstance.transform.localScale = Vector2.one;

        if (type == LogType.Error || type == LogType.Exception)
        {
            OpenLogPanel();
        }
    }

}
