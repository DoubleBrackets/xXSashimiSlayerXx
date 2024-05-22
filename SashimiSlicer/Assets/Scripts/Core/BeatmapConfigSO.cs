using System;
using UnityEngine;
using UnityEngine.Timeline;

[CreateAssetMenu(fileName = "BeatmapConfig", menuName = "BeatmapConfigSO")]
public class BeatmapConfigSo : ScriptableObject
{
    [field: Header("Song Timing")]

    [field: SerializeField]
    [field: TextArea]
    public string BeatmapName { get; private set; }

    [field: SerializeField]
    public double Bpm { get; private set; }

    [field: SerializeField]
    public int Subdivisions { get; private set; }

    [field: SerializeField]
    public double StartTime { get; private set; }

    [field: SerializeField]
    public int BeatsPerMeasure { get; private set; }

    [field: Header("Beatmap Data")]

    [field: SerializeField]
    public TimelineAsset BeatmapTimeline { get; private set; }

    [field: Header("Gameplay Data")]

    [field: SerializeField]
    public float BossHealth { get; private set; }

    /// <summary>
    ///     Take a time and snap it to the nearest subdivision
    /// </summary>
    /// <param name="rawTime"></param>
    /// <returns></returns>
    public double QuantizeTime(double rawTime)
    {
        double startTime = StartTime;
        double bpm = Bpm;

        int subdivisions = Subdivisions;
        subdivisions = subdivisions == 0 ? 1 : subdivisions;

        double subdivDuration = 60 / bpm / subdivisions;

        double beatTime = (rawTime - startTime) / subdivDuration;

        double quantizedBeatTime = Math.Round(beatTime);

        rawTime = startTime + quantizedBeatTime * subdivDuration;

        return rawTime;
    }
}