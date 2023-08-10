using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class EyeTrackingDataSample
{

    public int Timestamp;
    public long VideoFrame;
    public Vector3 GazeDirection;
    public string AoiName;
    public ViveSR.anipal.Eye.VerboseData VerboseEyeData;

    public EyeTrackingDataSample(int timestamp, long frame, Vector3 direction, ViveSR.anipal.Eye.VerboseData verboseData, string aoiName = "null")
    {
        Timestamp = timestamp;
        VideoFrame = frame;
        GazeDirection = direction;
        VerboseEyeData = verboseData;
        AoiName = aoiName;
    }

    // only for tester class
    public EyeTrackingDataSample()
    {
        AoiName = "null";
    }

}
