using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using SFB;


public class VideoLoader : MonoBehaviour
{

    public static VideoLoader Instance;
    public UnityEvent<string> OnVideoLoaded = new();

    public GameObject VideoLoadUI;

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
    }


    public void OnChooseFileButtonClicked()
    {
        //string path = EditorUtility.OpenFilePanel("Choose mp4 video", "", "*.mp4");
        var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", "", false);

        if (paths.Length == 0)
        {
            return;
        }

        if (paths[0].Length != 0)
        {
            LoadVideo(paths[0]);
        }
    }

    public static void LoadVideo(string path)
    {
        ProjectManager.ChangeProjectSetting(ProjectManager.SettingType.VideoPath, path);
        Instance.VideoLoadUI.SetActive(false);
        Instance.OnVideoLoaded?.Invoke(path);
    }

}
