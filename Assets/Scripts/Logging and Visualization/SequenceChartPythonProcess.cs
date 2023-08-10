using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

/// <summary>
/// Calls python process that creates sequence chart from filteredData csv file.
/// </summary>
public class SequenceChartPythonProcess : MonoBehaviour
{
    /// <summary>
    /// Calls python process that creates sequence chart from filteredData csv file.
    /// </summary>
    /// <param name="pathToDataCsv">File location of filtered data csv file.</param>
    /// <param name="savePath">Folder where output graph should be stored.</param>
    public static void CallSequenceChartScript(string pathToDataCsv, string savePath)
    {
        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = "python.exe";//cmd is full path to python.exe
        start.Arguments = "\"" + Application.dataPath + @"\StreamingAssets\Python\visualization_test.py" + "\" " + "-dp \"" + pathToDataCsv + "\" -sp \"" + savePath + "\"";//args is path to .py file and any cmd line args
        start.UseShellExecute = false;
        start.RedirectStandardOutput = true;
        Process.Start(start);
        using (Process process = Process.Start(start))
        {
            using (StreamReader reader = process.StandardOutput)
            {
                string result = reader.ReadToEnd();
                UnityEngine.Debug.Log("[CallSequenceChartScript] " + result);
            }
        }
    }
}
