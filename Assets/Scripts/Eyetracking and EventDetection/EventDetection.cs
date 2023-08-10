using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EventDetection
{

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
    public class AoiParameters
    {
        public string AoiName;

        public int DwellCount = 0;
        public int TimeToFirstDwell;
        public int DurationOfFirstDwell;
        public int TotalDwellDuration=0;
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


    // fixation detection parameters
    public static int FixationDurationThresholdInMs = 150;
    public static int FixationTimeBetweenSamplesLimitInMs = 32;
    public static int FixationDispersionThresholdInDegrees = 1;
    public static int FixationVelocityThresholdInDegPerSecond = 20;

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

        DetectDwells();
        DetectFixations();
    }

    public void FilterInvalidData()
    {
        for (int i = 0; i < RawDataSamples.Count; i++)
        {
            var current_sample = RawDataSamples[i];
            ulong flag = 2; //flag for gaze direction validity bit

            //if sample invalid
            if (!((flag & (current_sample.VerboseEyeData.left.eye_data_validata_bit_mask & current_sample.VerboseEyeData.right.eye_data_validata_bit_mask)) == flag))
            {
                // replaces invalid sample with mean of valid samples to the left and right,
                // unfortunately creates fake fixations during blinks, since samples are invalid for a long time

                //var index_left = FindIndexOfNextValidSample(i, false, flag);
                //var index_right = FindIndexOfNextValidSample(i, true, flag);

                //if (index_left != -1 && index_right != -1)
                //{
                //    var mean_direction = (RawData[index_left].GazeDirection + RawData[index_right].GazeDirection).normalized;
                //    current_sample.GazeDirection = mean_direction;
                //    FilteredData.Add(current_sample);
                //}
                //else if (index_left != -1)
                //{
                //    current_sample.GazeDirection = RawData[index_left].GazeDirection;
                //    FilteredData.Add(current_sample);
                //}
                //else if (index_right != -1)
                //{
                //    current_sample.GazeDirection = RawData[index_right].GazeDirection;
                //    FilteredData.Add(current_sample);
                //}
            }

            // if sample valid
            else
            {
                FilteredDataSamples.Add(current_sample);
            }
        }
    }

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

                // if next sample is the last sample, manually wrap up dwells
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
                Results[aoi].TimeToFirstDwell = firstdwell.StartTimestamp;
                Results[aoi].DurationOfFirstDwell = firstdwell.Duration;
                Results[aoi].TotalDwellDuration = Dwells.Where(dwell => dwell.AoiName == aoi).Sum(dwell => dwell.Duration);
            }         
        }


        foreach (var aoi in UniqueAoiNames())
        {
            Results[aoi].ProportionOfTotalDwellTime = EventDurationProportion(aoi, Event.EventType.Dwell);
        }

        return true;
    }

    public bool DetectFixations()
    {
        if (FilteredDataSamples.Count < 1)
        {
            return false;
        }

        Fixations = new();

        Vector3 fixationStartDirection = FilteredDataSamples[0].GazeDirection;
        int fixationStartTime = FilteredDataSamples[0].Timestamp;
        int fixationDuration = 0;

        List<string> aoiHitsDuringFixation = new();

        for (int i = 1; i < FilteredDataSamples.Count; i++)
        {

            bool adjacent_samples_are_within_fixation_criteria = false;
            EyeTrackingDataSample previous_sample = null;
            EyeTrackingDataSample current_sample = null;

            int delta_time = 0;

            for (int offset = 0; offset < 2; offset++)
            {

                if (i + offset > FilteredDataSamples.Count - 1)
                {
                    break;
                }

                previous_sample = FilteredDataSamples[i + offset - 1];
                current_sample = FilteredDataSamples[i + offset];

                var angle = Vector3.Angle(current_sample.GazeDirection, fixationStartDirection);
                delta_time = current_sample.Timestamp - previous_sample.Timestamp;

                var velocity = Vector3.Angle(current_sample.GazeDirection, previous_sample.GazeDirection)
                                / delta_time * 1000;

                adjacent_samples_are_within_fixation_criteria = adjacent_samples_are_within_fixation_criteria ||
                (angle <= FixationDispersionThresholdInDegrees && velocity <= FixationVelocityThresholdInDegPerSecond && delta_time <= FixationTimeBetweenSamplesLimitInMs);           

            }

            previous_sample = FilteredDataSamples[i - 1];
            current_sample = FilteredDataSamples[i];

            if (adjacent_samples_are_within_fixation_criteria)
            {
                fixationDuration += delta_time;
                aoiHitsDuringFixation.Add(current_sample.AoiName);

                // special condition of edge case where current_sample is last sample -> manually check and create fixation
                if (i == FilteredDataSamples.Count - 1 && fixationDuration >= FixationDurationThresholdInMs)
                {
                    Fixations.Add(new Event(Event.EventType.Fixation, fixationDuration, fixationStartTime, previous_sample.Timestamp, Helper.MostCommon(aoiHitsDuringFixation)));
                    Results[Helper.MostCommon(aoiHitsDuringFixation)].FixationCount++;
                }

            }
            else
            {
                if (fixationDuration >= FixationDurationThresholdInMs)
                {
                    Fixations.Add(new Event(Event.EventType.Fixation, fixationDuration, fixationStartTime, previous_sample.Timestamp, Helper.MostCommon(aoiHitsDuringFixation)));
                    Results[Helper.MostCommon(aoiHitsDuringFixation)].FixationCount++;
                }

                fixationDuration = 0;
                fixationStartDirection = current_sample.GazeDirection;
                fixationStartTime = current_sample.Timestamp;
                aoiHitsDuringFixation = new();
            }
        }

        // first fixation and proportion parameters after all fixations have been detected
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



    public float EventDurationProportion(string aoi_name, Event.EventType eventType)
    {
        List<Event> events = eventType == Event.EventType.Dwell ? Dwells : Fixations;

        float total_duration_of_all_events = events.Sum(item => item.Duration);
        float total_duration_of_events_for_aoi = events.Where(item => item.AoiName == aoi_name).Sum(item => item.Duration);

        float proportion = total_duration_of_events_for_aoi / total_duration_of_all_events;

        return proportion;
    }

    public float EventCountProportion(string aoi_name, Event.EventType eventType)
    {
        List<Event> events = eventType == Event.EventType.Dwell ? Dwells : Fixations;

        float total_count = events.Count;
        float count_of_events_for_aoi = events.Where(item => item.AoiName == aoi_name).Count();

        float proportion = count_of_events_for_aoi / total_count;

        return proportion;
    }

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
    public static T MostCommon<T>(this IEnumerable<T> list)
    {
        return (from i in list
                group i by i into grp
                orderby grp.Count() descending
                select grp.Key).First();
    }
}
