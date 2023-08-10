using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Some testing code for the event detection. Generates a list EyeTrackingDataSamples, then creates an event detection object
/// and passes the list as parameter. The created csv file can be checked manually and compared to the created EyeTrackingDataSamples.
/// </summary>

public class EventDetectionTester : MonoBehaviour
{

    private class TesterSample : EyeTrackingDataSample
    {

        public static float ms_per_videoframe = 33.33333f;

        public static int fake_time = 8;

        public enum aoi_test_selector
        {
            v1,
            v2,
            v3
        }


        public TesterSample(aoi_test_selector v)
        {
            Timestamp = fake_time;
            fake_time = fake_time + 8;

            VideoFrame = (int)Mathf.Round(Timestamp / ms_per_videoframe) >= 0 ? (int)Mathf.Round(Timestamp / ms_per_videoframe) : 0;

            switch (v)
            {
                case aoi_test_selector.v1:
                    GazeDirection = new(1, 0, 0);
                    AoiName = "v1";
                    break;
                case aoi_test_selector.v2:
                    GazeDirection = new(0, 1, 0);
                    AoiName = "v2";
                    break;
                case aoi_test_selector.v3:
                    GazeDirection = new(0, 0, 1);
                    AoiName = "v3";
                    break;
                default:
                    break;
            }

            VerboseEyeData = new();
            VerboseEyeData.left.eye_data_validata_bit_mask = 31;
            VerboseEyeData.right.eye_data_validata_bit_mask = 31;

        }

    }
    void Start()
    {

        //RunTest();

    }

    public static IDictionary<string, EventDetection.AoiParameters> RunTest(bool logResults = true)
    {
        List<EyeTrackingDataSample> fake_data = new();

        for (int i = 0; i < 21; i++)
        {
            fake_data.Add(new TesterSample(TesterSample.aoi_test_selector.v1));

            if (i == 10)
            {
                fake_data.Add(new TesterSample(TesterSample.aoi_test_selector.v3));
                fake_data.Add(new TesterSample(TesterSample.aoi_test_selector.v3));
            }
        }

        for (int i = 0; i < 21; i++)
        {
            fake_data.Add(new TesterSample(TesterSample.aoi_test_selector.v2));
        }

        for (int i = 0; i < 21; i++)
        {
            fake_data.Add(new TesterSample(TesterSample.aoi_test_selector.v3));
        }

        for (int i = 0; i < 21; i++)
        {
            fake_data.Add(new TesterSample(TesterSample.aoi_test_selector.v2));
        }

        for (int i = 0; i < 3000; i++)
        {
            var tester = new TesterSample(TesterSample.aoi_test_selector.v1);

            var random_direction = new Vector3(1, Random.Range(0, .01f), 0).normalized;

            tester.GazeDirection = random_direction;

            fake_data.Add(tester);
        }


        var eventDetector = new EventDetection(fake_data);
        var results = eventDetector.Results;

        if (logResults)
        {
            EyeDataLogger.WriteAoiParametersToCsv(results, "-test");
        }

        return results;
    }

}
