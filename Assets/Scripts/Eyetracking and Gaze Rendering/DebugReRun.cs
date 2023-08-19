using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Prototypical implementation of an offline analysis. Allows user to upload a filtered-data-samples csv file and outputs new aoi parameter file.
/// The parameters are calculated using the thresholds defined in the settings menu at time of rerun. 
/// </summary>
public class DebugReRun : MonoBehaviour
{
    /// <summary>
    /// Callback for DebugRerun Button in UI.
    /// </summary>
    public void OnReRun()
    {
        // Open file
        var extensions = new[] {
        new SFB.ExtensionFilter("Data File", "csv"),
        };
        var paths = SFB.StandaloneFileBrowser.OpenFilePanel("Open File", ProjectManager.Instance.ProjectFolderPath, extensions, false);

        if (paths.Length == 0)
        {
            return;
        }

        if (paths[0].Length != 0)
        {
            var data = CSVtoDataSampleList(paths[0]);
            EventDetection eventDetection = new(data);

            // Save file
            var path = SFB.StandaloneFileBrowser.SaveFilePanel("Save File", ProjectManager.Instance.ProjectFolderPath, "Rerun", "csv");

            EyeDataLogger.WriteAoiParametersToCsvAtPath(eventDetection.Results, path);
            DebugFixationSampleFlagsToCsv(eventDetection.Fixationsample_flags, path);
        }
    }

    /// <summary>
    /// Creates Data Samples from csv file.
    /// </summary>
    /// <param name="filepath"></param>
    /// <returns></returns>
    List<EyeTrackingDataSample> CSVtoDataSampleList(string filepath)
    {
        List<EyeTrackingDataSample> samples = new();

        int i = 0;

        using (var reader = new StreamReader(filepath))
        {
            
            while (!reader.EndOfStream)
            {

                var line = reader.ReadLine();
                var values = line.Split(',');

                if (values[0] == "Timestamp")
                {
                    continue;
                }

                int timestamp = 0;
                long frame = 0;
                decimal x, y, z = 0;
                string aoi = "null";

                ViveSR.anipal.Eye.VerboseData verboseData = new();
                verboseData.left.eye_data_validata_bit_mask = 18446744073709551615;
                verboseData.right.eye_data_validata_bit_mask = 18446744073709551615;

                int.TryParse(values[0], out timestamp);
                long.TryParse(values[1], out frame);

                decimal.TryParse(values[2], out x);
                decimal.TryParse(values[3], out y);
                decimal.TryParse(values[4], out z);

                aoi = values[7];

                EyeTrackingDataSample sample = new(timestamp, frame, new((float)x, (float)y, (float)z), verboseData, aoi);
                samples.Add(sample);
            }
        }


        return samples;
    }

    /// <summary>
    /// Outputs as csv file containing fixation flags for every data sample. Not used in program. Was used for comparison of
    /// fixation detection algorithm with vr-idt python package.
    /// </summary>
    /// <param name="Fixationsample_flags"></param>
    /// <param name="path"></param>
    private void DebugFixationSampleFlagsToCsv(bool[] Fixationsample_flags, string path)
    {

        path = Path.GetDirectoryName(path) + @"\fixationflags.csv";
        using (StreamWriter writer = new StreamWriter(new FileStream(path, FileMode.Create, FileAccess.Write)))
        {
            writer.WriteLine("flags");

            foreach (bool flag in Fixationsample_flags)
            {
                string line = flag ? "1" : "0";
                writer.WriteLine(line);
            }
        }
        
    }
}
