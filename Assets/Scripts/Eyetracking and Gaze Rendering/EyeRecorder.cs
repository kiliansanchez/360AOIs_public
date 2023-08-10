using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ViveSR.anipal.Eye;
using System.Runtime.InteropServices;
using System;

/// <summary>
/// Derived from SRanipal example script.
/// This script is responsible for getting the eyedata from the VR-Headset, storing the data during recording as well as
/// carrying out the raycast to check whether or not the gaze is hitting a AOI.
/// Note: Callback runs on a separate thread to report at ~120hz.
/// Unity is not threadsafe and cannot call any UnityEngine api from within callback thread.
/// </summary>
public class EyeRecorder : MonoBehaviour
{

    public static EyeRecorder Instance { get; private set; }

    public static bool IsRecording { get; private set; } = false;
    public static UnityEvent<bool> OnRecordingToggled { get; private set; } = new();
    private void SetIsRecordingAndNotify(bool new_status)
    {
        IsRecording = new_status;
        OnRecordingToggled?.Invoke(IsRecording);
    }

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
        Instance = this;
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

        var sensitivity_checker = new EyeParameter();
        SRanipal_Eye_API.GetEyeParameter(ref sensitivity_checker);

        if (sensitivity_checker.gaze_ray_parameter.sensitive_factor != Sensitivity)
        {
            Debug.Log("Changing Sensitivity from " + sensitivity_checker.gaze_ray_parameter.sensitive_factor + " to " + Sensitivity);
            sensitivity_checker.gaze_ray_parameter.sensitive_factor = Sensitivity;
            SRanipal_Eye_API.SetEyeParameter(sensitivity_checker);
        }

        //when using the program with only one person it helps to be able to start recording from keyboard rather than having to click
        //record button in UI. 
        if (Input.GetKeyDown(KeyCode.F) && VRManager.CurrentVRStatus == VRManager.VRStatus.Active)
        {
            ToggleRecording();
        }

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
        if (IsRecording)
        {
            if(_timestampOffset == -1){

                _timestampOffset = eye_data.timestamp;

            }

            eye_data.timestamp = eye_data.timestamp - _timestampOffset;
            _recordedEyeData.Add(eye_data);
        }

    }


    /// <summary>
    /// Callback for Recording button at top right corner of UI. Toggles recording on or off.
    /// </summary>
    public void ToggleRecording()
    {
        if (!IsRecording)
        {
            StartRecording();
        }
        else
        {
            FinishRecording();
        }
         
    }

    /// <summary>
    /// Starts the recording by mainly invoking the RecordData method with InvokeRepeating.
    /// </summary>
    public void StartRecording()
    {
        AOIManager.Instance.ToggleAOIColliders(true);
        _timestampOffset = -1;
        SetIsRecordingAndNotify(true);
        InvokeRepeating(nameof(RecordData), 0, 0.004f);
    }

    /// <summary>
    /// stops the recording, creates event detection and logs data. starts python process for visualizing data. 
    /// </summary>
    public void FinishRecording()
    {

        SetIsRecordingAndNotify(false);

        CancelInvoke(nameof(RecordData));

        if (_recordedEyeData.Count > 0)
        {
            Debug.LogWarning("[EyeRecorder] data processing too slow, " + _recordedEyeData.Count + " samples left to process");
        }


        Recording_ID = DateTime.Today.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();
        Recording_ID = Recording_ID.Replace(@"\", "-");
        Recording_ID = Recording_ID.Replace(":", "-");


        EyeDataLogger.WriteDatasamplesToCSV(RawData, "-raw");
        EventDetection eventDetection = new(RawData);
        string filepath = EyeDataLogger.WriteDatasamplesToCSV(eventDetection.FilteredDataSamples, "-filtered");
        EyeDataLogger.WriteAoiParametersToCsv(eventDetection.Results);
        SequenceChartPythonProcess.CallSequenceChartScript(filepath, ProjectManager.Instance.ProjectFolderPath + @"\Results\" + EyeRecorder.Recording_ID);

        //reset recorded data for new recording
	    _recordedEyeData = new List<EyeData>();
    	RawData = new List<EyeTrackingDataSample>();      
    }

    /// <summary>
    /// Method for recording the eyedata. Checks if new eyedata is available from headset (or simulation) and gets gazedirection
    /// from eyedata. Carrys out raycast with gazedirection and checks for any AOI-Hits. Tags datasample with first AOI hit.
    /// Note: Can't handle overlapping AOIs yet.
    /// </summary>
    void RecordData()
    {

        if (_recordedEyeData.Count > 0)
        {
            var item = _recordedEyeData[0];
            EyeTrackingDataSample newSample;

            if (GazeSimulator.UseSimulatedDataInstead)
            {
                newSample = GazeSimulator.GetNextFakeSample();

                if (newSample == null)
                {
                    return;
                }

                //newRow.Timestamp = item.timestamp;
                newSample.VideoFrame = VideoManager.VideoPlayer.frame;
            }
            else
            {
                Vector3 GazeOriginCombinedLocal, GazeDirectionCombinedLocal;
                SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal, item);
                Vector3 GazeDirectionCombined = Camera.main.transform.TransformDirection(GazeDirectionCombinedLocal);


                newSample = new EyeTrackingDataSample(item.timestamp, VideoManager.VideoPlayer.frame, GazeDirectionCombined,
                    item.verbose_data);
                
            }


            if (Physics.Raycast(Camera.main.transform.position, newSample.GazeDirection, out RaycastHit hit, 25f))
            {

                if (hit.collider.gameObject != null)
                {
                    // Does the ray intersect any AOIs?
                    if (hit.collider.gameObject.CompareTag("AOI"))
                    {
                        newSample.AoiName = hit.collider.gameObject.GetComponent<Listable>().Name;
                    }
                }

            }

            RawData.Add(newSample);
            _recordedEyeData.Remove(item);

        }
        
    }

}