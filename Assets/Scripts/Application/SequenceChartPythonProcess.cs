using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class SequenceChartPythonProcess : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //string dp = @"C:\Users\Kilian\AppData\LocalLow\DefaultCompany\360AOIs\RawData\DataSamples-13-37-34-raw.csv";
        //string sp = @"C:\Users\Kilian\AppData\LocalLow\DefaultCompany\360AOIs\RawData";

        //CallSequenceChartScript(dp, sp);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void CallSequenceChartScript(string pathToDataCsv, string savePath)
    {
        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = "python.exe";//cmd is full path to python.exe
        start.Arguments = "\"" + Application.dataPath + @"\StreamingAssets\Python\visualization_test.py" + "\" " + "-dp \"" + pathToDataCsv + "\" -sp \"" + savePath + "\"";//args is path to .py file and any cmd line args
        start.UseShellExecute = false;
        start.RedirectStandardOutput = true;
        Process.Start(start);
        //using (Process process = Process.Start(start))
        //{
        //    using (StreamReader reader = process.StandardOutput)
        //    {
        //        string result = reader.ReadToEnd();
        //        UnityEngine.Debug.Log(result);
        //    }
        //}
    }
}
