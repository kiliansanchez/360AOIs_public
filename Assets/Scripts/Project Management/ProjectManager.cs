using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

/// <summary>
/// Handles the creation of new projects as well as loading new projects. Stores relevant file paths in a file called
/// project_settings.txt. Very simple implementation, in the future this should probably be handled using xml or json.
/// </summary>
public class ProjectManager : MonoBehaviour
{

    public static ProjectManager Instance;

    public GameObject StartupPanel;

    public string ProjectFolderPath = "";

    public string ProjectSettingsFileName = "project_settings.txt";

    static string _aoiJsonFileString = "aoi_json_file:";
    static string _360VideoFilePathString = "video_path:";

    public enum SettingType
    {
        AoiJsonFile,
        VideoPath
    }


    void Start()
    {
        Instance = this;    
    }

  /// <summary>
  /// Callback for OnButtonDown of New Project Button. Prompts user to select a folder for the project.
  /// </summary>
    public void OnNewProjectButtonDown()
    {
        var paths = SFB.StandaloneFileBrowser.OpenFolderPanel("Please select a new folder for your project", "C:\\",false);

        if (paths.Length == 0)
        {
            return;
        }

        ProjectFolderPath = paths[0];
        CreateProjectSettingsFile(ProjectFolderPath);

        StartupPanel.SetActive(false);
    }


    /// <summary>
    /// Callback for Load Project Button OnButtonDown Event. Promts user to select existing project folder.
    /// Reads project settings file and starts loading of video and AOIs based on paths specified in project settings file.
    /// </summary>
    public void OnLoadProjectButtonDown()
    {
        var paths = SFB.StandaloneFileBrowser.OpenFolderPanel("Please select the folder of your existing project", "C:\\", false);
        
        if (paths.Length == 0)
        {
            return;
        }

        ProjectFolderPath = paths[0];

        if (!File.Exists(ProjectFolderPath + @"\" + ProjectSettingsFileName))
        {
            Debug.LogError("Selected folder doesn't contain a project settings file called \"" + ProjectSettingsFileName + "\". " + "" +
                "Please select a different folder, or create a new project.");
            LogMessagesManager.Instance.OpenLogPanel();
            return;
        }

        //load video
        var videopath = Path.GetFullPath(GetSettingFromProjectSettings(SettingType.VideoPath), ProjectFolderPath);
        VideoLoader.LoadVideo(videopath);

        //load aois
        var aoi_path = Path.GetFullPath(GetSettingFromProjectSettings(SettingType.AoiJsonFile), ProjectFolderPath);
        SaveAndLoadAOIs.LoadAois(aoi_path);

        StartupPanel.SetActive(false);
    }

    /// <summary>
    /// Creates project setting file at specified path.
    /// </summary>
    /// <param name="path">Path for creation of project settings file.</param>
    public void CreateProjectSettingsFile(string path)
    {
        var filestream = File.Create(path + @"\" + ProjectSettingsFileName);

        using (StreamWriter writer = new StreamWriter(filestream))
        {

            writer.WriteLine("creation_date: " + DateTime.Today.ToShortDateString() + " at " + DateTime.Now.ToLongTimeString());
            writer.WriteLine(_aoiJsonFileString);
            writer.WriteLine(_360VideoFilePathString);
            writer.Close();
        }

        filestream.Close();
    }

    /// <summary>
    /// Getter function that returns the path to the project settings file.
    /// </summary>
    /// <returns>Path to the project settings file</returns>
    public static string GetFullPathToProjectSettingsFile()
    {
        return Instance.ProjectFolderPath + @"\" + Instance.ProjectSettingsFileName;
    }

    /// <summary>
    /// Changes specified setting to new value.
    /// </summary>
    /// <param name="type">Selection of which setting should be changed.</param>
    /// <param name="new_value">New value for selected setting.</param>
    public static void ChangeProjectSetting(SettingType type, string new_value)
    {
        string setting = "";
        switch (type)
        {
            case SettingType.AoiJsonFile:
                setting = _aoiJsonFileString;
                break;
            case SettingType.VideoPath:
                setting = _360VideoFilePathString;
                break;
            default:
                break;
        }

        new_value = setting + " " + new_value;

        int index = GetLineIndexFromString(setting);

        if (index == -1)
        {
            return;
        }

        string[] arrLine = File.ReadAllLines(GetFullPathToProjectSettingsFile());
        arrLine[index] = new_value;
        File.WriteAllLines(GetFullPathToProjectSettingsFile(), arrLine);
    }

    /// <summary>
    /// Getter function for setting specified in projects settings file.
    /// </summary>
    /// <param name="setting">Name of setting for which to return value.</param>
    /// <returns>Returns setting value as string. Returns "error" if setting has no value.</returns>
    public static string GetSettingFromProjectSettings(SettingType setting)
    {
        string value = "";
        string setting_identifier = "";
        switch (setting)
        {
            case SettingType.AoiJsonFile:
                setting_identifier = _aoiJsonFileString;
                break;
            case SettingType.VideoPath:
                setting_identifier = _360VideoFilePathString;
                break;
            default:
                break;
        }

        using (var reader = new StreamReader(GetFullPathToProjectSettingsFile()))
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();

                if (line.StartsWith(setting_identifier))
                {
                    var strings = line.Split(": ");
                    if (strings.Length < 2)
                    {
                        Debug.LogError("Setting has no value");
                        return "error";
                    }
                    value = strings[1];
                }
              
            }

            reader.Close();
        }

        return value;
    }

    /// <summary>
    /// Helper function to locate specific setting in project settings text file.
    /// </summary>
    /// <param name="str">String to find location of.</param>
    /// <returns>line index of string that starts with passed value.</returns>
    public static int GetLineIndexFromString(string str)
    {
        int line_index = -1;
        int i = 0;

        using (var reader = new StreamReader(GetFullPathToProjectSettingsFile()))
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();

                if (line.StartsWith(str))
                {
                    line_index = i;
                }

                i++;
            }

            reader.Close();
        }

        return line_index;
    }
}
