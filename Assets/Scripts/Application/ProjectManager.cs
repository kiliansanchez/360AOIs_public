using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class ProjectManager : MonoBehaviour
{

    public GameObject StartupPanel;

    public static string ProjectFolderPath = "";

    public static string ProjectSettingsFileName = "project_settings.txt";

    static string _aoiJsonFileString = "aoi_json_file:";
    static string _360VideoFilePathString = "video_path:";

    public enum SettingType
    {
        AoiJsonFile,
        VideoPath
    }

    // Start is called before the first frame update
    void Start()
    {
        
        
    }

    // Update is called once per frame
    void Update()
    {
       
    }


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



    public void OnLoadProjectButtonDown()
    {
        var paths = SFB.StandaloneFileBrowser.OpenFolderPanel("Please select the folder of your existing project", "C:\\", false);
        
        if (paths.Length == 0)
        {
            return;
        }

        ProjectFolderPath = paths[0];

        //load video
        VideoLoader.LoadVideo(GetSettingFromProjectSettings(SettingType.VideoPath));

        //load aois
        SaveStateToJson.LoadAois(GetSettingFromProjectSettings(SettingType.AoiJsonFile));

        StartupPanel.SetActive(false);
    }

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

    public static string GetFullPathToProjectSettingsFile()
    {
        return ProjectFolderPath + @"\" + ProjectSettingsFileName;
    }

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

        Debug.Log(value);
        return value;
    }

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
