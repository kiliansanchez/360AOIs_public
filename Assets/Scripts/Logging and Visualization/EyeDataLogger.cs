using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;

/// <summary>
/// Handles the creation of all csv output files.
/// </summary>
public class EyeDataLogger
{
    /// <summary>
    /// Writes DataSamples to csv
    /// </summary>
    /// <param name="data">List of datasamples.</param>
    /// <param name="filename_postfix">Postfix for filename. File will be called DataSamples + Current Time + postfix.</param>
    /// <returns></returns>
    public static string WriteDatasamplesToCSV(List<EyeTrackingDataSample> data, string filename_postfix = "")
    {

        if (!Directory.Exists(ProjectManager.Instance.ProjectFolderPath + @"\Results\" + EyeRecorder.Recording_ID))
        {
            Directory.CreateDirectory(ProjectManager.Instance.ProjectFolderPath + @"\Results\" + EyeRecorder.Recording_ID);
        }

        string filename = "DataSamples" + "-" + DateTime.Now.ToLongTimeString() + filename_postfix + ".csv";
        filename = filename.Replace(@"\", "-");
        filename = filename.Replace(":", "-");

        string path = ProjectManager.Instance.ProjectFolderPath + @"\Results\" + EyeRecorder.Recording_ID;
        string fullPath = path + @"\" + filename;

        Directory.CreateDirectory(path);

        using (StreamWriter writer = new StreamWriter(new FileStream(fullPath, FileMode.Create, FileAccess.Write)))
        {
            writer.WriteLine("Timestamp" + "," + "VideoFrame" + "," + "GazeDirection_x" + "," + "GazeDirection_y" + "," + "GazeDirection_z" + "," + "delta_angle" + "," + "velocity_deg/s" + "," + "AOI" + "," + "CombinedValidity");

            for (int i = 0; i < data.Count; i++)
            {
                //string gazeDirection = rawData[i]._gazeDirection.ToString().Replace(", ", "-");
                string x = data[i].GazeDirection.x.ToString();
                string y = data[i].GazeDirection.y.ToString();
                string z = data[i].GazeDirection.z.ToString();

                float delta_angle = i == 0 ? 0 : Vector3.Angle(data[i-1].GazeDirection, data[i].GazeDirection);
                float velocity = i == 0 ? 0 : delta_angle / (data[i].Timestamp - data[i-1].Timestamp) * 1000;

                //bool fixationtest = TestForFixation(rawData, i);

                var combined_validity = data[i].VerboseEyeData.left.eye_data_validata_bit_mask & data[i].VerboseEyeData.right.eye_data_validata_bit_mask;
                var combined_validity_string = Convert.ToString((long)combined_validity, 2);

                string line = data[i].Timestamp.ToString() + "," + data[i].VideoFrame + "," +  x + "," + y + "," + z + "," + delta_angle + "," + velocity + "," + data[i].AoiName + "," + combined_validity_string;
                writer.WriteLine(line);
            }

            writer.Close();
        }

        return fullPath;
    }

    /// <summary>
    /// After EventDetection is completed this method writes the AOI parameters to csv.
    /// </summary>
    /// <param name="results">Dictionary of AOIParaneters</param>
    /// <param name="filename_postfix">Postfix for filename. File will be called AoiParameters + Current Time + postfix.</param>
    public static void WriteAoiParametersToCsv(IDictionary<string, EventDetection.AoiParameters> results, string filename_postfix = "")
    {

        if (!Directory.Exists(ProjectManager.Instance.ProjectFolderPath + @"\Results\" + EyeRecorder.Recording_ID))
        {
            Directory.CreateDirectory(ProjectManager.Instance.ProjectFolderPath + @"\Results\" + EyeRecorder.Recording_ID);
        }

        string filename = "AoiParameters" + "-" + DateTime.Now.ToLongTimeString() + filename_postfix + ".csv";
        filename = filename.Replace(@"\", "-");
        filename = filename.Replace(":", "-");

        string path = ProjectManager.Instance.ProjectFolderPath + @"\Results\" + EyeRecorder.Recording_ID;
        string fullPath = path + @"\" + filename;

        Directory.CreateDirectory(path);

        using (StreamWriter writer = new StreamWriter(new FileStream(fullPath, FileMode.Create, FileAccess.Write)))
        {
            writer.WriteLine("AOI" + "," + "Dwell_Count" + "," + "Entry_Time" + "," + "First_Pass_Dwell_Time" + "," + "Total_Dwell_Time" + "," + "Proportion_Of_Total_Dwell_Time" +
                "," + "Fixation_Count" + "," + "Time_To_First_Fixation" + "," + "Duration_Of_First_Fixation" + "," + "Proportion_Of_Fixations");

            foreach (KeyValuePair<string, EventDetection.AoiParameters> entry in results)
            {
                EventDetection.AoiParameters data = entry.Value;
                string line = data.AoiName + "," + data.DwellCount + "," + data.EntryTime + "," + data.FirstPassDwellTime + "," + data.TotalDwellTime + "," + data.ProportionOfTotalDwellTime
                    + "," + data.FixationCount + "," + data.TimeToFirstFixation + "," + data.DurationOfFirstFixation + "," + data.ProportionOfFixations;
                writer.WriteLine(line);
            }
        }      
    }


    /// <summary>
    /// Used in debugrerun to write csv parameters to path chosen by user, without having a recording id.
    /// </summary>
    /// <param name="results"></param>
    /// <param name="path"></param>
    public static void WriteAoiParametersToCsvAtPath(IDictionary<string, EventDetection.AoiParameters> results, string path)
    {

        using (StreamWriter writer = new StreamWriter(new FileStream(path, FileMode.Create, FileAccess.Write)))
        {
            writer.WriteLine("AOI" + "," + "Dwell_Count" + "," + "Entry_Time" + "," + "First_Pass_Dwell_Time" + "," + "Total_Dwell_Time" + "," + "Proportion_Of_Total_Dwell_Time" +
                "," + "Fixation_Count" + "," + "Time_To_First_Fixation" + "," + "Duration_Of_First_Fixation" + "," + "Proportion_Of_Fixations");

            foreach (KeyValuePair<string, EventDetection.AoiParameters> entry in results)
            {
                EventDetection.AoiParameters data = entry.Value;
                string line = data.AoiName + "," + data.DwellCount + "," + data.EntryTime + "," + data.FirstPassDwellTime + "," + data.TotalDwellTime + "," + data.ProportionOfTotalDwellTime
                    + "," + data.FixationCount + "," + data.TimeToFirstFixation + "," + data.DurationOfFirstFixation + "," + data.ProportionOfFixations;
                writer.WriteLine(line);
            }
        }
    }


    /// <summary>
    /// Unused old function. Writes a dictionary to csv. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="parameterName"></param>
    /// <param name="dict"></param>
    public static void WriteDictionaryToCsv <T>(string parameterName, IDictionary<string, T> dict)
    {

        if (!Directory.Exists(ProjectManager.Instance.ProjectFolderPath + @"\Results\" + EyeRecorder.Recording_ID))
        {
            Directory.CreateDirectory(ProjectManager.Instance.ProjectFolderPath + @"\Results\" + EyeRecorder.Recording_ID);
        }

        string filename = parameterName + "-" + DateTime.Now.ToLongTimeString() + ".csv";
        filename = filename.Replace(@"\", "-");
        filename = filename.Replace(":", "-");

        string path = ProjectManager.Instance.ProjectFolderPath + @"\Results\" + EyeRecorder.Recording_ID;
        string fullPath = path + @"\" + filename;

        Directory.CreateDirectory(path);

        using (StreamWriter writer = new StreamWriter(new FileStream(fullPath, FileMode.Create, FileAccess.Write)))
        {
            writer.WriteLine("AOI" + "," + parameterName);

            foreach (KeyValuePair<string, T> entry in dict)
            {
                writer.WriteLine(entry.Key + "," + entry.Value.ToString());
            }
        }
    }
}
