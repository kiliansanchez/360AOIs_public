using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogMessagesManager : MonoBehaviour
{
    public GameObject LogPanel;

    public GameObject LogEntryPrefab;

    public UnityEngine.UI.ScrollRect ScrollRect;

    public Transform ContentContainer;

    void Awake()
    {
        Application.logMessageReceived += HandleException;
        //DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            LogPanel.SetActive(!LogPanel.activeSelf);
        }
    }

    void HandleException(string logString, string stackTrace, LogType type)
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

        text_label.text = logString;
        text_label.color = color;
        //parent the item to the content container
        itemInstance.transform.SetParent(ContentContainer);
        //reset the item's scale -- this can get munged with UI prefabs
        itemInstance.transform.localScale = Vector2.one;
    }

}
