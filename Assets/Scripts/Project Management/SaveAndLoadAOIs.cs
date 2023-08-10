using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System;

/// <summary>
/// Handles saving and loading AOIs to and from a json file.
/// </summary>
public class SaveAndLoadAOIs : MonoBehaviour
{
    [System.Serializable]
    public class AOISaveData
    {
        [SerializeField]
        public string Name;
        [SerializeField]
        public List<Animation.FrameData> Keyframes;
    }

    [System.Serializable]
    public class Datalist
    {
        [SerializeField]
        public AOISaveData[] list;
    }

    public static Datalist datalist = new();


    /// <summary>
    /// Callback for Save Button OnButtonDown Event. Prompts user to select an existing file to overwrite or to declare new filename
    /// for AOI-save file. Converts all AOIs in current scene to json and stores information in selected file.
    /// </summary>
    public void Save()
    {

        // Save file
        var path = SFB.StandaloneFileBrowser.SaveFilePanel("Save File", ProjectManager.Instance.ProjectFolderPath, "360AOIsave", "json");

        GameObject[] aois1 = GameObject.FindGameObjectsWithTag("AOI");
        GameObject[] aois2 = GameObject.FindGameObjectsWithTag("AOI_Invisible");
        GameObject[] aois = aois1.Concat(aois2).ToArray();

        
        datalist.list = new AOISaveData[aois.Length];

        for (int i = 0; i < aois.Length; i++)
        {
            var item = aois[i];
            AOISaveData data = new();
            data.Name = item.GetComponent<Listable>().Name;
            data.Keyframes = item.GetComponent<Animation>().Keyframes;

            datalist.list[i] = data;
        }

        string json = JsonUtility.ToJson(datalist);

        File.WriteAllText(path, json);

        ProjectManager.ChangeProjectSetting(ProjectManager.SettingType.AoiJsonFile, Path.GetRelativePath(ProjectManager.Instance.ProjectFolderPath, path));
    }


    /// <summary>
    /// Callback for ButtonDown Event of Load AOIs Button. Prompts user to select a file from which to load AOIs.
    /// </summary>
    public void OnLoadAOIButtonDown()
    {
        // Open file
        var extensions = new[] {
        new SFB.ExtensionFilter("AOI File", "json"),
        };
        var paths = SFB.StandaloneFileBrowser.OpenFilePanel("Open File", ProjectManager.Instance.ProjectFolderPath, extensions, false);

        if (paths.Length == 0)
        {
            return;
        }

        if (paths[0].Length != 0)
        {
            LoadAois(paths[0]);
        }

            
    }

    /// <summary>
    /// Loads AOIs from json file specified as parameter.
    /// </summary>
    /// <param name="path">Path for json file to load AOIs from.</param>
    public static void LoadAois(string path)
    {
        string fileContents;

        if (File.Exists(path))
        {
            try
            {
                // Read the entire file and save its contents.
                fileContents = File.ReadAllText(path);

                datalist = JsonUtility.FromJson<Datalist>(fileContents);

                foreach (AOISaveData item in datalist.list)
                {
                    GameObject aoi = AOIManager.Instance.SpawnAoi(item.Keyframes[0].Position);
                    aoi.GetComponent<Animation>().LoadKeyframes(item.Keyframes);
                    aoi.GetComponent<Listable>().SetName(item.Name);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Couldn't load AOI save file.");
                Debug.LogError(e.Message);
                LogMessagesManager.Instance.OpenLogPanel();
                return;
            }

        }
        else
        {
            Debug.LogError("Couldn't load AOI save file. File at specified path " + path + " doesn't exist.");
            LogMessagesManager.Instance.OpenLogPanel();
            return;
        }

        ProjectManager.ChangeProjectSetting(ProjectManager.SettingType.AoiJsonFile, Path.GetRelativePath(ProjectManager.Instance.ProjectFolderPath, path));
        
    }
}
