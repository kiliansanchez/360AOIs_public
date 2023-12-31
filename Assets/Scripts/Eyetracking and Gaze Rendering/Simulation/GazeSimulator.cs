using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Another test-script. The code generates fixations at a specified Vector3. Instead of creating an eventdetection object here
/// the fake_data is used in the eyerecorder class when the UseSimulatedDataInstead flag is set to true.
/// </summary>
public class GazeSimulator : MonoBehaviour
{

    private static List<EyeTrackingDataSample> fake_data;

    private static int fake_time_in_ms = 8;

    public static bool UseSimulatedDataInstead = false;

    public Vector3 TestRayDirection;


    void Start()
    {
        fake_data = new();
        TestRayDirection = new(0, 0, 0);

        // test data 

        // x
        GenerateFixation(new(1,0,0), 160);

        // y
        GenerateFixation(new(0, 1, 0), 200);

        // z
        GenerateFixation(new(0, 0, 1), 184);

        // y
        GenerateFixation(new(0, 1, 0), 256);
        //GenerateFixation(new(1, 0, 0), 320);

        // z
        GenerateFixation(new(0.01f, 0, 0.24f),176);

        // x
        GenerateFixation(new(1, 0, 0), 800, 100, 5, 700);
    }


    /// <summary>
    /// Debug code used to draw a vector in the scene view based on TestRayDirection. Allows to visually check where a given vector is pointing to.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (EditorCamera.Camera != null && TestRayDirection != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(EditorCamera.Camera.transform.position, TestRayDirection.normalized * 25);

            if (Physics.Raycast(EditorCamera.Camera.transform.position, TestRayDirection.normalized * 25, out RaycastHit hit, 25f))
            {

                if (hit.collider.gameObject != null)
                {
                    Gizmos.DrawSphere(hit.point, .1f);
                }

            }

        }     
    }
    /// <summary>
    /// Generates a fixation (or dwell) based on the provided parameters.
    /// </summary>
    /// <param name="center">Center point of generated datasamples</param>
    /// <param name="duration">Duration of Fixation/Dwell</param>
    /// <param name="validityPercentge">Percentage of valid datasamples.</param>
    /// <param name="dispersionLimit">Dispersion limit for generated datasamples</param>
    /// <param name="velocityLimit">Velocity limit for generated datasamples</param>
    void GenerateFixation(Vector3 center, int duration = 150, int validityPercentge = 100, float dispersionLimit = -1, float velocityLimit = -1)
    {
        if (dispersionLimit == -1)
        {
            dispersionLimit = EventDetection.FixationDispersionThresholdInDegrees;
        }

        if (velocityLimit == -1)
        {
            velocityLimit = EventDetection.FixationVelocityThresholdInDegPerSecond;
        }


        int no_of_samples = (int)Mathf.Ceil(duration / 8);

        for (int i = 0; i <= no_of_samples; i++)
        {

            // generate gaze direction around center within dispersion and velocity limit
            Vector3 sample_direction = Vector3.zero;

            bool withinDispersionLimit = false;
            bool withinVelocityLimit = false;

            while (!(withinDispersionLimit && withinVelocityLimit))
            {

                float offset = 0.05f;

                var x = center.x + Random.Range(dispersionLimit * -offset, dispersionLimit * offset);
                var y = center.y + Random.Range(dispersionLimit * -offset, dispersionLimit * offset);
                var z = center.z + Random.Range(dispersionLimit * -offset, dispersionLimit * offset);

                sample_direction = new Vector3(x, y, z).normalized;

                withinDispersionLimit = Vector3.Angle(center, sample_direction) <= dispersionLimit;

                if (i == 0)
                {
                    withinVelocityLimit = true;
                }
                else
                {
                    var previous_sample = fake_data.Last();

                    var velocity = Vector3.Angle(sample_direction, previous_sample.GazeDirection) / 8 * 1000;
                    withinVelocityLimit = velocity <= velocityLimit;
                }
            }


            // create EyeTrackingDataSample
            var sample = new EyeTrackingDataSample();

            // set gaze direction of sample to generated gaze direction
            sample.GazeDirection = sample_direction;

            // set timestamp of sample       

            sample.Timestamp = fake_time_in_ms;
            fake_time_in_ms += 8;


            // set validity bit of verbose data of eye tracking data sample
            ViveSR.anipal.Eye.VerboseData verbosedata = new();

            var random = Random.Range(0, 100);

            if (random > validityPercentge)
            {
                verbosedata.left.eye_data_validata_bit_mask = 0;
                verbosedata.right.eye_data_validata_bit_mask = 0;
            }
            else
            {
                verbosedata.left.eye_data_validata_bit_mask = 31;
                verbosedata.right.eye_data_validata_bit_mask = 31;
            }         

            sample.VerboseEyeData = verbosedata;

            // add data to a list of data that is feed to recorder instead of raw EyeData 
            fake_data.Add(sample);
        }

    }

    /// <summary>
    /// Used by Eyerecorder to get next generated datasample during recording when UseSimulatedDataInstead flag is set to true.
    /// </summary>
    /// <returns>Returns next generated DataSample.</returns>
    public static EyeTrackingDataSample GetNextFakeSample()
    {
        if (fake_data.Any()) //prevent IndexOutOfRangeException for empty list
        {
            var sample = fake_data[0];
            fake_data.RemoveAt(0);
            return sample;
        }

        else
        {
            return null;
        }
    }
}
