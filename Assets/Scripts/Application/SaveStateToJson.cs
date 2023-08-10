using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public class SaveStateToJson : MonoBehaviour
{
    [System.Serializable]
    public class AOISaveData
    {
        [SerializeField]
        public string Name;
        [SerializeField]
        public List<Keyframable.FrameData> Keyframes;
    }

    [System.Serializable]
    public class Datalist
    {
        [SerializeField]
        public AOISaveData[] list;
    }

    public static Datalist datalist = new();

    public void Save()
    {

        // Save file
        var path = SFB.StandaloneFileBrowser.SaveFilePanel("Save File", ProjectManager.ProjectFolderPath, "360AOIsave", "json");

        GameObject[] aois1 = GameObject.FindGameObjectsWithTag("AOI");
        GameObject[] aois2 = GameObject.FindGameObjectsWithTag("AOI_Invisible");
        GameObject[] aois = aois1.Concat(aois2).ToArray();

        string json;
        datalist.list = new AOISaveData[aois.Length];

        for (int i = 0; i < aois.Length; i++)
        {
            var item = aois[i];
            AOISaveData data = new();
            data.Name = item.GetComponent<Listable>().Name;
            data.Keyframes = item.GetComponent<Keyframable>().Keyframes;

            datalist.list[i] = data;
        }

        json = JsonUtility.ToJson(datalist);

        File.WriteAllText(path, json);

        ProjectManager.ChangeProjectSetting(ProjectManager.SettingType.AoiJsonFile, path);
    }



    public void OnLoadAOIButtonDown()
    {
        // Open file
        var paths = SFB.StandaloneFileBrowser.OpenFilePanel("Open File", ProjectManager.ProjectFolderPath, "", false);

        if (paths.Length == 0)
        {
            return;
        }

        if (paths[0].Length != 0)
        {
            LoadAois(paths[0]);
        }

            
    }

    public static void LoadAois(string path)
    {
        string fileContents;

        if (File.Exists(path))
        {
            // Read the entire file and save its contents.
            fileContents = File.ReadAllText(path);

            datalist = JsonUtility.FromJson<Datalist>(fileContents);

            foreach (AOISaveData item in datalist.list)
            {
                GameObject aoi = AOIManager.Instance.SpawnAoi(item.Keyframes[0].Position);
                aoi.GetComponent<Keyframable>().LoadKeyframes(item.Keyframes);
                aoi.GetComponent<Listable>().SetName(item.Name);
            }
        }

        ProjectManager.ChangeProjectSetting(ProjectManager.SettingType.AoiJsonFile, path);
        
    }
}
