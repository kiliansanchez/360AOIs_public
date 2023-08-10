using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using UnityEngine.UI;
using SFB;

/// <summary>
/// This class is responsible for loading a video from storage into the project.
/// </summary>
public class VideoLoader : MonoBehaviour
{

    public static VideoLoader Instance;
    public UnityEvent<string> OnVideoLoaded { get; private set; } = new();

    public GameObject VideoLoadUI;
    public Toggle VideoCopyToggle;

    public string video_path;

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Callback for OnButtonDown Event of ChooseFile Button. Prompts user to select a video file. If flag is set by user
    /// copies the selected video file into project folder.
    /// </summary>
    public void OnChooseFileButtonClicked()
    {
        //string path = EditorUtility.OpenFilePanel("Choose mp4 video", "", "*.mp4");
        // Open file with filter
        var extensions = new[] {
        new ExtensionFilter("Video Files", "asf", "avu", "dv", "m4v", "mov", "mp4", "mpg", "mpeg", ".ogv", "vp8", "webm", "wmv" ),
        };
        var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false);

        if (paths.Length == 0)
        {
            return;
        }

        if (paths[0].Length != 0)
        {
            string path = paths[0];

            if (Instance.VideoCopyToggle.isOn)
            {
                if (File.Exists(path))
                {
                    string filename = Path.GetFileName(path);
                    Instance.video_path = ProjectManager.Instance.ProjectFolderPath + @"\" + filename;

                    if (!File.Exists(Instance.video_path))
                    {
                        File.Copy(path, Instance.video_path);
                    }
                    
                    ProjectManager.ChangeProjectSetting(ProjectManager.SettingType.VideoPath, Path.GetRelativePath(ProjectManager.Instance.ProjectFolderPath, Instance.video_path));
                }
            }
            else
            {
                Instance.video_path = path;
                ProjectManager.ChangeProjectSetting(ProjectManager.SettingType.VideoPath, Path.GetRelativePath(ProjectManager.Instance.ProjectFolderPath, Instance.video_path));
            }

            LoadVideo(Instance.video_path);
        }
    }

    /// <summary>
    /// Tiny little function to signal to VideoManager that path has been selected and that video can be loaded.
    /// </summary>
    /// <param name="path">Path of the video.</param>
    public static void LoadVideo(string path)
    {
             
        Instance.VideoLoadUI.SetActive(false);
        Instance.OnVideoLoaded?.Invoke(path);
    }

}
