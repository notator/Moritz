using Multimedia.Midi;

namespace Moritz.AssistantPerformer
{
    /// <summary>
    /// Called by the AssistantPerformerRuntime to report the performer's position to 
    /// the AssistantPerformerMainForm during an assisted performance.
    /// </summary>
    public delegate void ReportPositionDelegate(int msPosition);
    public delegate void CheckRepeatDelegate(PerformanceState performanceState);
    internal delegate void SaveSequenceAsMidiFileDelegate(Sequence sequence, string defaultFilename);
    /// <summary>
    /// called by the AssistantPerformerRuntime to report that the performance has completed.
    /// </summary>
    internal delegate void NotifyCompletionDelegate();
}
