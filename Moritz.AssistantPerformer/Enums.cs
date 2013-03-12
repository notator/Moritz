
using System.ComponentModel;

namespace Moritz.AssistantPerformer
{
    public enum MoritzPlayer
    {
        [Description("notPlayed")]
        None,
        [Description("assistant")]
        Assistant,
        [Description("livePerformer")]
        LivePerformer,
    }

    public enum KeyType
    {
        [Description("0")]
        Silent,
        [Description("1")]
        Assisted,
        [Description("2")]
        Solo_AssistantHearsNothing,
        [Description("3")]
        Solo_AssistantHearsFirst,
    }

    public enum PerformersPitchesType
    {
        [Description("asNotated")]
        AsNotated,	// All pitches in the score are played as notated.
        [Description("asPerformed")]
        AsPerformed, // Performer's pitches override the notated ones. Assistant's pitches as notated.
    }

    public enum PerformersDynamicsType
    {
        [Description("asNotated")]
        AsNotated,	// All dynamics in the score are played as notated.
        [Description("asPerformed")]
        AsPerformed, // The performer's dynamics override the notated ones. Assistant's pitches as notated.
        [Description("silent")]
        Silent, // The performer's staff is silent (Conductor option) Overrides any dynamics options.
    }

    public enum AssistantsDurationsType
    {
        // Assistant's durations are calculated from the logical symbol widths notated in the score.
        // The performer's previously performed durations are ignored. 
        [Description("symbolsAbsolute")]
        SymbolsAbsolute,
        // Assistant's durations are calculated from the logical symbol widths notated in the score, and the performer's
        // previously performed duration.
        [Description("symbolsRelative")]
        SymbolsRelative,
    }
}
