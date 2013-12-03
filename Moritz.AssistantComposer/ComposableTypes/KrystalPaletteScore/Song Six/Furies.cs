using System.Collections.Generic;
using System.Diagnostics;
using System;

using Moritz.Score;
using Moritz.Score.Midi;
using Krystals4ObjectLibrary;

namespace Moritz.AssistantComposer
{
    /// <summary>
    /// The Furies "constructors" (part of the Song Six algorithm).
    /// </summary>
    internal partial class SongSixAlgorithm : MidiCompositionAlgorithm
    {

        private VoiceDef GetFury4(int firstRestMsDuration, Clytemnestra clytemnestra, PaletteDef palette)
        {
            List<LocalMidiDurationDef> snores = new List<LocalMidiDurationDef>();
            int msPosition = 0;

            LocalMidiDurationDef firstRest = new LocalMidiDurationDef(msPosition, firstRestMsDuration);
            snores.Add(firstRest);
            msPosition += firstRestMsDuration;

            #region prelude + verse1
            for(int i = 0; i < 7; ++i)
            {
                LocalMidiDurationDef snore = new LocalMidiDurationDef(palette[i]);
                snore.MsPosition = msPosition;
                msPosition += snore.MsDuration;
                snores.Add(snore);

                LocalMidiDurationDef rest = new LocalMidiDurationDef(msPosition,2500);
                msPosition += rest.MsDuration;
                snores.Add(rest);
            }
            #endregion

            int finalRestMsDuration = clytemnestra.EndMsPosition - msPosition;
            LocalMidiDurationDef finalRest = new LocalMidiDurationDef(msPosition, finalRestMsDuration);
            snores.Add(finalRest);

            VoiceDef fury4 = new VoiceDef(snores);

            #region alignments in Verse 1
            fury4.AlignObjectAtIndex(7, 8, 9, clytemnestra[3].MsPosition);
            fury4.AlignObjectAtIndex(8, 9, 10, clytemnestra[7].MsPosition);
            fury4.AlignObjectAtIndex(9, 10, 11, clytemnestra[16].MsPosition);
            fury4.AlignObjectAtIndex(10, 11, 12, clytemnestra[24].MsPosition);
            fury4.AlignObjectAtIndex(11, 12, 13, clytemnestra[39].MsPosition);
            fury4.AlignObjectAtIndex(12, 13, 14, clytemnestra[42].MsPosition);
            fury4.AlignObjectAtIndex(13, 14, 15, (clytemnestra[56].MsPosition + clytemnestra[57].MsPosition) / 2);
            #endregion

            fury4.RemoveScorePitchWheelCommands(0, 11);

            return fury4;
        }
    }
}
