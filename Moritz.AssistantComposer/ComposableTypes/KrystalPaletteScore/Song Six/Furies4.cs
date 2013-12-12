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

        private VoiceDef GetFuries4(int firstRestMsDuration, Clytemnestra clytemnestra, VoiceDef wind1, PaletteDef palette)
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

            double factor;
            double msDuration;
            double restDuration;
            for(int i = 10; i >= 0; --i)
            {
                LocalMidiDurationDef snore = new LocalMidiDurationDef(palette[i/2]);

                snore.MsPosition = msPosition;
                factor = Math.Pow(1.07, (double) i);
                msDuration = snore.MsDuration * factor;
                snore.MsDuration = (int)msDuration;
                msPosition += snore.MsDuration;
                snore.Transpose(i - 10);
                UniqueMidiChordDef umcd = snore.UniqueMidiDurationDef as UniqueMidiChordDef;
                umcd.MidiVelocity = (byte)((double)umcd.MidiVelocity / factor);
                snores.Add(snore);

                restDuration = 2500 * factor;
                LocalMidiDurationDef rest = new LocalMidiDurationDef(msPosition, (int)restDuration);
                msPosition += rest.MsDuration;
                snores.Add(rest);

            }

            snores[snores.Count - 1].MsDuration = clytemnestra.EndMsPosition - snores[snores.Count - 1].MsPosition;

            VoiceDef furies4 = new VoiceDef(snores);

            #region alignments in Verse 1
            furies4.AlignObjectAtIndex(7, 8, 9, clytemnestra[3].MsPosition);
            furies4.AlignObjectAtIndex(8, 9, 10, clytemnestra[7].MsPosition);
            furies4.AlignObjectAtIndex(9, 10, 11, clytemnestra[16].MsPosition);
            furies4.AlignObjectAtIndex(10, 11, 12, clytemnestra[24].MsPosition);
            furies4.AlignObjectAtIndex(11, 12, 13, clytemnestra[39].MsPosition);
            furies4.AlignObjectAtIndex(12, 13, 14, clytemnestra[42].MsPosition);
            furies4.AlignObjectAtIndex(13, 14, 15, (clytemnestra[56].MsPosition + clytemnestra[57].MsPosition) / 2);
            furies4.AlignObjectAtIndex(15, 23, 37, wind1[28].MsPosition);
            #endregion

            furies4.RemoveScorePitchWheelCommands(0, 12);

            return furies4;
        }
    }
}
