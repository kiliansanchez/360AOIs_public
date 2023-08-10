using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Eye;
using System.Runtime.InteropServices;
using System;

/// <summary>
/// Example usage for eye tracking callback
/// Note: Callback runs on a separate thread to report at ~120hz.
/// Unity is not threadsafe and cannot call any UnityEngine api from within callback thread.
/// </summary>
public class EyeRecorder : MonoBehaviour
{

    public static EyeRecorder Recorder { get; private set; }

    [Range(0, 1)]
    public double Sensitivity = 1;

    public static EyeData CurrentEyeData { get; private set; } = new EyeData();
    public static EyeData PreviousEyeData { get; private set; } = new EyeData();

    private static bool _eye_callback_registered = false;

    private static int _timestampOffset = 0;
    private static List<EyeData> _recordedEyeData = new List<EyeData>();

    public static List<EyeTrackingDataSample> RawData { get; private set; } = new List<EyeTrackingDataSample>(); 

    public static string Recording_ID;

    private void Start()
    {
        Recorder = this;
    }

    private void Update()
    {
        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING) return;

        if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == true && _eye_callback_registered == false)
        {
            SRanipal_Eye.WrapperRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye.CallbackBasic)EyeCallback));
            _eye_callback_registered = true;
        }
        else if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == false && _eye_callback_registered == true)
        {
            SRanipal_Eye.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye.CallbackBasic)EyeCallback));
            _eye_callback_registered = false;
        }

        if (!_eye_callback_registered)
        {
            return;
        }

        var test = new EyeParameter();
        SRanipal_Eye_API.GetEyeParameter(ref test);

        if (test.gaze_ray_parameter.sensitive_factor != Sensitivity)
        {
            Debug.Log("Changing Sensitivity from " + test.gaze_ray_parameter.sensitive_factor + " to " + Sensitivity);
            test.gaze_ray_parameter.sensitive_factor = Sensitivity;
            SRanipal_Eye_API.SetEyeParameter(test);
        }
       
        //RenderCurrentGazeAsRay();
    }

   

    private void OnDisable()
    {
        Release();
    }

    void OnApplicationQuit()
    {
        Release();
    }

    /// <summary>
    /// Release callback thread when disabled or quit
    /// </summary>
    private static void Release()
    {
        if (_eye_callback_registered == true)
        {
            SRanipal_Eye.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye.CallbackBasic)EyeCallback));
            _eye_callback_registered = false;
        }
    }

    /// <summary>
    /// Required class for IL2CPP scripting backend support
    /// </summary>
    internal class MonoPInvokeCallbackAttribute : System.Attribute
    {
        public MonoPInvokeCallbackAttribute() { }
    }

    /// <summary>
    /// Eye tracking data callback thread.
    /// Reports data at ~120hz
    /// MonoPInvokeCallback attribute required for IL2CPP scripting backend
    /// </summary>
    /// <param name="eye_data">Reference to latest eye_data</param>
    [MonoPInvokeCallback]
    private static void EyeCallback(ref EyeData eye_data)
    {
        PreviousEyeData = CurrentEyeData;
        CurrentEyeData = eye_data;

        if (RecordButton.IsRecording)
        {
            eye_data.timestamp = eye_data.timestamp - _timestampOffset;
            _recordedEyeData.Add(eye_data);
        }      
    }

    public void StartRecording()
    {
        _timestampOffset = CurrentEyeData.timestamp;
        InvokeRepeating(nameof(RecordData), 0, 0.004f);
    }

    public void FinishRecording()
    {
        _timestampOffset = 0;

        CancelInvoke(nameof(RecordData));

        if (_recordedEyeData.Count > 0)
        {
            Debug.LogWarning("data processing too slow, " + _recordedEyeData.Count + " samples left to process");
        }

        //while (_recordedEyeData.Count > 0)
        //{       
        //    RecordData();
        //}

        //EyeDataLogger.WriteDatasamplesToCSV(RawData, "-raw");

        Recording_ID = DateTime.Today.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();
        Recording_ID = Recording_ID.Replace(@"\", "-");
        Recording_ID = Recording_ID.Replace(":", "-");

        EventDetection eventDetection = new(RawData);
        string filepath = EyeDataLogger.WriteDatasamplesToCSV(eventDetection.FilteredDataSamples, "-filtered");
        EyeDataLogger.WriteAoiParametersToCsv(eventDetection.Results);
        SequenceChartPythonProcess.CallSequenceChartScript(filepath, ProjectManager.ProjectFolderPath + @"\Results\" + EyeRecorder.Recording_ID);

    }

    void RecordData()
    {

        if (_recordedEyeData.Count > 0)
        {
            var item = _recordedEyeData[0];
            EyeTrackingDataSample newRow;

            if (GazeSimulator.UseSimulatedDataInstead)
            {
                newRow = GazeSimulator.GetNextFakeSample();

                if (newRow == null)
                {
                    return;
                }

                //newRow.Timestamp = item.timestamp;
                newRow.VideoFrame = VideoManager.VideoPlayer.frame;
            }
            else
            {
                Vector3 GazeOriginCombinedLocal, GazeDirectionCombinedLocal;
                SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal, item);
                Vector3 GazeDirectionCombined = Camera.main.transform.TransformDirection(GazeDirectionCombinedLocal);


                newRow = new EyeTrackingDataSample(item.timestamp, VideoManager.VideoPlayer.frame, GazeDirectionCombined,
                    item.verbose_data);
                
            }


            if (Physics.Raycast(Camera.main.transform.position, newRow.GazeDirection, out RaycastHit hit, 25f))
            {

                if (hit.collider.gameObject != null)
                {
                    // Does the ray intersect any AOIs?
                    if (hit.collider.gameObject.CompareTag("AOI"))
                    {
                        newRow.AoiName = hit.collider.gameObject.GetComponent<Listable>().Name;
                    }
                }

            }

            RawData.Add(newRow);
            _recordedEyeData.Remove(item);

        }
        
    }

}