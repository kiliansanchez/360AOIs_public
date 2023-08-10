using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// This script contains all the event-detection algorithms for detecting dwells and fixations with their respective
/// parameters like duration etc.
/// </summary>

public class EventDetection
{
    //class representing a single dwell of fixation
    public class Event
    {
        public enum EventType
        {
            Dwell,
            Fixation
        }

        public EventType Type;
        public int Duration;
        public int StartTimestamp;
        public int EndTimestamp;
        public string AoiName;

        public Event(EventType type, int duration, int startTimestamp, int endTimestamp = -1, string aoiName = "null")
        {
            Type = type;
            Duration = duration;
            StartTimestamp = startTimestamp;

            if (endTimestamp == -1)
            {
                EndTimestamp = StartTimestamp + Duration;
            }
            else
            {
                EndTimestamp = endTimestamp;
            }
            
            AoiName = aoiName;
        }
    }

    //dataclass holding the different eyetracking parameters for a single AOI
    public class AoiParameters
    {
        public string AoiName;

        public int DwellCount = 0;
        public int EntryTime;
        public int FirstPassDwellTime;
        public int TotalDwellTime=0;
        public float ProportionOfTotalDwellTime;

        public int FixationCount = 0;
        public int TimeToFirstFixation;
        public int DurationOfFirstFixation;
        public float ProportionOfFixations;

        public AoiParameters(string aoiName)
        {
            AoiName = aoiName;
        }
    }

    public List<EyeTrackingDataSample> RawDataSamples;
    public List<EyeTrackingDataSample> FilteredDataSamples;

    public bool[] Fixationsample_flags;

    // fixation detection parameters
    public static int FixationDurationThresholdInMs = 150;
    public static int FixationTimeBetweenSamplesLimitInMs = 32;
    public static float FixationDispersionThresholdInDegrees = 1;
    public static float FixationVelocityThresholdInDegPerSecond = 35;

    // dwell count parameters
    // how far apart do AOIhit-timestamps need to be (in milliseconds) to be considered a new dwell -> DwellToleranceInMs
    // needs to be large enough to account for blinks (intervalls of invalid data which creates gap in timestamps) but small enough to reasonable tell different dwells apart
    public static int DwellToleranceInMs = 100;

    public List<Event> Dwells { get; private set; }
    public List<Event> Fixations { get; private set; }

    public IDictionary<string, AoiParameters> Results = new Dictionary<string, AoiParameters>();

  

    public EventDetection(List<EyeTrackingDataSample> rawDataSamples)
    {
        RawDataSamples = rawDataSamples;
        FilteredDataSamples = new();
        FilterInvalidData();

        if (FilteredDataSamples.Count == 0)
        {
            //object invalid
            Debug.LogError("Eventdetection has no valid data to work with");
            return;
        }

        var aois = UniqueAoiNames();
        foreach (var aoi in aois)
        {
            Results.Add(aoi, new AoiParameters(aoi));
        }

        //for testing purposes, not needed for program
        Fixationsample_flags = new bool[FilteredDataSamples.Count];
        for (int i = 0; i < Fixationsample_flags.Length; i++)
        {
            Fixationsample_flags[i] = false;
        }

        DetectDwells();
        DetectFixations();
    }

    /// <summary>
    /// Filters all datasamples that don't have a valid gaze direction.
    /// </summary>
    public void FilterInvalidData()
    {
        for (int i = 0; i < RawDataSamples.Count; i++)
        {
            var current_sample = RawDataSamples[i];
            ulong flag = 2; //flag for gaze direction validity bit

            //if sample invalid
            if (!((flag & (current_sample.VerboseEyeData.left.eye_data_validata_bit_mask & current_sample.VerboseEyeData.right.eye_data_validata_bit_mask)) == flag))
            {             
            }

            // if sample valid
            else
            {
                FilteredDataSamples.Add(current_sample);
            }
        }
    }

    /// <summary>
    /// helper function that finds next valid data sample closest to specified start index. Not used anymore.
    /// </summary>
    /// <param name="startIndex"></param>
    /// <param name="lookAhead">if true, looks right of start index, if false looks left</param>
    /// <param name="validity_flag">flag in bitmask to be checked</param>
    /// <returns></returns>
    public int FindIndexOfNextValidSample(int startIndex, bool lookAhead, ulong validity_flag)
    {
        int direction = lookAhead ? 1 : -1;

        if (startIndex + direction > RawDataSamples.Count || startIndex + direction < 0)
        {
            return -1;
        }

        int break_condition = lookAhead ? RawDataSamples.Count : 0;

        for (int i = startIndex + direction; i != break_condition; i+= direction)
        {
            var current_sample = RawDataSamples[i];

            if ((validity_flag & (current_sample.VerboseEyeData.left.eye_data_validata_bit_mask & current_sample.VerboseEyeData.right.eye_data_validata_bit_mask)) == validity_flag)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Algorithm for detecting dwells.
    /// </summary>
    /// <returns>Returns true of detection had valid data samples to work with.</returns>
    public bool DetectDwells()
    {
        if (FilteredDataSamples.Count < 1)
        {
            return false;
        }

        var unique_aoi_names = UniqueAoiNames();

        Dwells = new();

        foreach (var aoi in unique_aoi_names)
        {

            var dataForAOI = FilteredDataSamples.Where(x => x.AoiName == aoi).ToList();

            int dwellDuration = 0;
            int dwellStartTime = dataForAOI[0].Timestamp;

            for (int i = 0; i < dataForAOI.Count - 1; i++)
            {


                var current_sample = dataForAOI[i];
                var next_sample = dataForAOI[i + 1];

                var delta_time = next_sample.Timestamp - current_sample.Timestamp;

                if (delta_time <= DwellToleranceInMs)
                {
                    dwellDuration += delta_time;
                }             

                if (delta_time > DwellToleranceInMs)
                {

                    var dwellEndTime = current_sample.Timestamp;

                    var dwell = new Event(Event.EventType.Dwell, dwellDuration, dwellStartTime, dwellEndTime, aoi);
                    Dwells.Add(dwell);

                    Results[aoi].DwellCount++;

                    dwellDuration = 0;
                    dwellStartTime = next_sample.Timestamp;

                }

                // if next sample is the last sample, manually wrap up dwell
                if (next_sample == dataForAOI.Last())
                {
                    var dwell = new Event(Event.EventType.Dwell, dwellDuration, dwellStartTime, next_sample.Timestamp, aoi);
                    Dwells.Add(dwell);
                    Results[aoi].DwellCount++;
                }

            }

            if (Dwells.Count > 0)
            {
                var firstdwell = Dwells.Where(dwell => dwell.AoiName == aoi).First();
                Results[aoi].EntryTime = firstdwell.StartTimestamp;
                Results[aoi].FirstPassDwellTime = firstdwell.Duration;
                Results[aoi].TotalDwellTime = Dwells.Where(dwell => dwell.AoiName == aoi).Sum(dwell => dwell.Duration);
            }         
        }


        foreach (var aoi in UniqueAoiNames())
        {
            Results[aoi].ProportionOfTotalDwellTime = EventDurationProportion(aoi, Event.EventType.Dwell);
        }

        return true;
    }

    /// <summary>
    /// Helper Function that takes a start index and outputs the index if the first data sample that is FixationDurationThresholdInMs milliseconds
    /// away from start index. It basically spans a window of 150ms (or whatever value the user chooses) to be used in the fixation detection.
    /// </summary>
    /// <param name="samples">Samples to check</param>
    /// <param name="startindex">Start index to check from</param>
    /// <returns>Index at position of FixationDurationThresholdInMs away from start index</returns>
    private int GetSampleIndexAtMinFixationDuration(List<EyeTrackingDataSample> samples, int startindex = 0)
    {
        int time = 0;

        if (startindex > samples.Count-2)
        {
            return -1;
        }

        for (int i = startindex + 1; i < samples.Count; i++)
        {
            int delta_time = samples[i].Timestamp - samples[i-1].Timestamp;
            time += delta_time;

            if (time >= FixationDurationThresholdInMs)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// I-DVT algorithm that detects fixations. Based on lanes-Jurado et al. (2020)
    /// https://pubmed.ncbi.nlm.nih.gov/32883026/
    /// </summary>
    /// <returns>Returns true when fixation detection was able to run without errors.</returns>
    public bool DetectFixations()
    {
        if (FilteredDataSamples.Count < 1)
        {
            return false;
        }

        Fixations = new();

        List<EyeTrackingDataSample> samples = FilteredDataSamples;

        int window_left_index = 0;

        while (window_left_index < samples.Count - 2)
        {
            int window_right_index = GetSampleIndexAtMinFixationDuration(samples, window_left_index);
            if (window_right_index == -1)
            {
                break;
            }

            bool loop_condition = true;
            bool fixationDetected = false;       

            while (loop_condition)
            {

                bool allSamplesWithinThreshold = true;

                if (window_left_index > samples.Count - 2)
                {
                    break;
                }

                //check dispersion by comparing angle pairwise between all points
                for (int i = window_left_index; i < window_right_index; i++)
                {
                    for (int j = i + 1; j <= window_right_index; j++)
                    {

                        float angle = Vector3.Angle(samples[i].GazeDirection, samples[j].GazeDirection);
                
                        if (angle > FixationDispersionThresholdInDegrees)
                        {
                            allSamplesWithinThreshold = false;
                            break;
                        }
                    }

                    if (!allSamplesWithinThreshold)
                    {
                        break;
                    }
                }

                // check velocity and frequency by comparing each sample to its previous sample
                // except the first one. first sample of fixation may be coming out of a saccade
                // and is allowed to have high velocity
                for (int i = window_left_index + 1; i <= window_right_index; i++)
                {

                    int delta_time = samples[i].Timestamp - samples[i - 1].Timestamp;
                    float velocity = Vector3.Angle(samples[i].GazeDirection, samples[i - 1].GazeDirection)
                        / delta_time * 1000;
                    if (velocity > FixationVelocityThresholdInDegPerSecond || delta_time > FixationTimeBetweenSamplesLimitInMs)
                    {
                        allSamplesWithinThreshold = false;
                    }
                    
                }

                if (allSamplesWithinThreshold)
                {
                    // All vectors are within the desired dispersion threshold
                    fixationDetected = true;
                    window_right_index++;

                    if (window_right_index < samples.Count - 1)
                    {
                        continue;
                    }             
                }

                 // At least one vector is outside the desired dispersion, velocity and frequency thresholds
                 // or last datasample has been reached

                if (fixationDetected)
                {
                    //create fixation from window_left_index to window_right_index - 1
                    int fixationDuration =
                        samples[window_right_index - 1].Timestamp - samples[window_left_index].Timestamp;

                    List<string> aoiHitsDuringFixation = new();
                    for (int i = window_left_index; i < window_right_index; i++)
                    {
                        aoiHitsDuringFixation.Add(samples[i].AoiName);
                        Fixationsample_flags[i] = true;
                    }

                    Fixations.Add(new Event(Event.EventType.Fixation, fixationDuration, 
                        samples[window_left_index].Timestamp,
                        samples[window_right_index - 1].Timestamp, 
                        Helper.MostCommon(aoiHitsDuringFixation)));
                    Results[Helper.MostCommon(aoiHitsDuringFixation)].FixationCount++;

                    //remove entire window except last sample of fixation.
                    window_left_index = window_right_index - 1;                       
                }
                else
                {
                    //remove sample at window_left_index
                    window_left_index++;
                }

                loop_condition = false;
                fixationDetected = false;           
            }

        }

        //first fixation and proportion parameters after all fixations have been detected
        if (Fixations.Count > 0)
        {
            var aois = UniqueAoiNames();
            foreach (var aoi in aois)
            {
                if (Fixations.Any(fixation => fixation.AoiName == aoi))
                {
                    var firstfixation = Fixations.Where(fixation => fixation.AoiName == aoi).First();
                    Results[aoi].TimeToFirstFixation = firstfixation.StartTimestamp;
                    Results[aoi].DurationOfFirstFixation = firstfixation.Duration;

                    Results[aoi].ProportionOfFixations = EventCountProportion(aoi, Event.EventType.Fixation);
                }
            }
        }

        return true;
    }


    /// <summary>
    /// Calculates the proportion of duration for dwells or fixations for a given AOI.
    /// </summary>
    /// <param name="aoi_name">Name of AOI for which to calculate duration proportion</param>
    /// <param name="eventType">Dwells or Fixations</param>
    /// <returns></returns>
    public float EventDurationProportion(string aoi_name, Event.EventType eventType)
    {
        List<Event> events = eventType == Event.EventType.Dwell ? Dwells : Fixations;

        float total_duration_of_all_events = events.Sum(item => item.Duration);
        float total_duration_of_events_for_aoi = events.Where(item => item.AoiName == aoi_name).Sum(item => item.Duration);

        float proportion = total_duration_of_events_for_aoi / total_duration_of_all_events;

        return proportion;
    }

    /// <summary>
    /// Calculates the proportion of occurrences for dwells or fixations for a given AOI.
    /// </summary>
    /// <param name="aoi_name">Name of AOI for which to calculate occurrences-proportion</param>
    /// <param name="eventType">Dwells or Fixations</param>
    /// <returns></returns>
    public float EventCountProportion(string aoi_name, Event.EventType eventType)
    {
        List<Event> events = eventType == Event.EventType.Dwell ? Dwells : Fixations;

        float total_count = events.Count;
        float count_of_events_for_aoi = events.Where(item => item.AoiName == aoi_name).Count();

        float proportion = count_of_events_for_aoi / total_count;

        return proportion;
    }

    /// <summary>
    /// Little helper that returns a list of the unique AOIs that are found in the filtered data.
    /// </summary>
    /// <returns>List of unique AOI names.</returns>
    public List<string> UniqueAoiNames()
    {
        List<string> unique_aoi_names = new();
        foreach (var item in FilteredDataSamples)
        {
            unique_aoi_names.Add(item.AoiName);
        }
        unique_aoi_names = unique_aoi_names.Distinct().ToList();
        return unique_aoi_names;
    }

   

}

public static class Helper
{
    /// <summary>
    /// Used to determine which AOI a fixation should be assigned to.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns>Returns the valua that has the most occurrences within a list</returns>
    public static T MostCommon<T>(this IEnumerable<T> list)
    {
        return (from i in list
                group i by i into grp
                orderby grp.Count() descending
                select grp.Key).First();
    }
}
