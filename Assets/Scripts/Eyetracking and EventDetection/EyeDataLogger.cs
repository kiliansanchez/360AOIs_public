using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;

public class EyeDataLogger
{
    public static string WriteDatasamplesToCSV(List<EyeTrackingDataSample> rawData, string filename_postfix = "")
    {

        if (!Directory.Exists(ProjectManager.ProjectFolderPath + @"\Results\" + EyeRecorder.Recording_ID))
        {
            Directory.CreateDirectory(ProjectManager.ProjectFolderPath + @"\Results\" + EyeRecorder.Recording_ID);
        }

        string filename = "DataSamples" + "-" + DateTime.Now.ToLongTimeString() + filename_postfix + ".csv";
        filename = filename.Replace(@"\", "-");
        filename = filename.Replace(":", "-");

        string path = ProjectManager.ProjectFolderPath + @"\Results\" + EyeRecorder.Recording_ID;
        string fullPath = path + @"\" + filename;

        Directory.CreateDirectory(path);

        using (StreamWriter writer = new StreamWriter(new FileStream(fullPath, FileMode.Create, FileAccess.Write)))
        {
            writer.WriteLine("Timestamp" + "," + "VideoFrame" + "," + "GazeDirection_x" + "," + "GazeDirection_y" + "," + "GazeDirection_z" + "," + "delta_angle" + "," + "velocity_deg/s" + "," + "AOI" + "," + "CombinedValidity");

            for (int i = 0; i < rawData.Count; i++)
            {
                //string gazeDirection = rawData[i]._gazeDirection.ToString().Replace(", ", "-");
                string x = rawData[i].GazeDirection.x.ToString();
                string y = rawData[i].GazeDirection.y.ToString();
                string z = rawData[i].GazeDirection.z.ToString();

                float delta_angle = i == 0 ? 0 : Vector3.Angle(rawData[i-1].GazeDirection, rawData[i].GazeDirection);
                float velocity = i == 0 ? 0 : delta_angle / (rawData[i].Timestamp - rawData[i-1].Timestamp) * 1000;

                //bool fixationtest = TestForFixation(rawData, i);

                var combined_validity = rawData[i].VerboseEyeData.left.eye_data_validata_bit_mask & rawData[i].VerboseEyeData.right.eye_data_validata_bit_mask;
                var combined_validity_string = Convert.ToString((long)combined_validity, 2);

                string line = rawData[i].Timestamp.ToString() + "," + rawData[i].VideoFrame + "," +  x + "," + y + "," + z + "," + delta_angle + "," + velocity + "," + rawData[i].AoiName + "," + combined_validity_string;
                writer.WriteLine(line);
            }

            writer.Close();
        }

        return fullPath;
    }


   public static void WriteAoiParametersToCsv(IDictionary<string, EventDetection.AoiParameters> results, string filename_postfix = "")
    {

        if (!Directory.Exists(ProjectManager.ProjectFolderPath + @"\Results\" + EyeRecorder.Recording_ID))
        {
            Directory.CreateDirectory(ProjectManager.ProjectFolderPath + @"\Results\" + EyeRecorder.Recording_ID);
        }

        string filename = "AoiParameters" + "-" + DateTime.Now.ToLongTimeString() + filename_postfix + ".csv";
        filename = filename.Replace(@"\", "-");
        filename = filename.Replace(":", "-");

        string path = ProjectManager.ProjectFolderPath + @"\Results\" + EyeRecorder.Recording_ID;
        string fullPath = path + @"\" + filename;

        Directory.CreateDirectory(path);

        using (StreamWriter writer = new StreamWriter(new FileStream(fullPath, FileMode.Create, FileAccess.Write)))
        {
            writer.WriteLine("AOI" + "," + "Dwell_Count" + "," + "Time_to_first_Dwell" + "," + "Duration_of_first_Dwell" + "," + "Total_Dwell_Duration" + "," + "Proportion_of_total_dwell_time" +
                "," + "Fixation_Count" + "," + "Time_To_First_Fixation" + "," + "Duration_of_first_Fixation" + "," + "Proportion_of_fixations");

            foreach (KeyValuePair<string, EventDetection.AoiParameters> entry in results)
            {
                EventDetection.AoiParameters data = entry.Value;
                string line = data.AoiName + "," + data.DwellCount + "," + data.TimeToFirstDwell + "," + data.DurationOfFirstDwell + "," + data.TotalDwellDuration + "," + data.ProportionOfTotalDwellTime
                    + "," + data.FixationCount + "," + data.TimeToFirstFixation + "," + data.DurationOfFirstFixation + "," + data.ProportionOfFixations;
                writer.WriteLine(line);
            }
        }      
    }

    public static void WriteDictionaryToCsv <T>(string parameterName, IDictionary<string, T> dict)
    {

        if (!Directory.Exists(ProjectManager.ProjectFolderPath + @"\Results\" + EyeRecorder.Recording_ID))
        {
            Directory.CreateDirectory(ProjectManager.ProjectFolderPath + @"\Results\" + EyeRecorder.Recording_ID);
        }

        string filename = parameterName + "-" + DateTime.Now.ToLongTimeString() + ".csv";
        filename = filename.Replace(@"\", "-");
        filename = filename.Replace(":", "-");

        string path = ProjectManager.ProjectFolderPath + @"\Results\" + EyeRecorder.Recording_ID;
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
