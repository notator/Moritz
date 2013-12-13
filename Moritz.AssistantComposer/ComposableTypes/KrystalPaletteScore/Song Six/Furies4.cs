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
            int[] transpositions1 = { 0,0,0,0,0,1,0 };
            for(int i = 0; i < 7; ++i)
            {
                LocalMidiDurationDef snore = new LocalMidiDurationDef(palette[i]);
                snore.MsPosition = msPosition;
                msPosition += snore.MsDuration;
                snore.Transpose(transpositions1[i]);
                snores.Add(snore);

                LocalMidiDurationDef rest = new LocalMidiDurationDef(msPosition,2500);
                msPosition += rest.MsDuration;
                snores.Add(rest);
            }
            #endregion

            double factor;
            double msDuration;
            double restDuration;
            int[] transpositions2 = { 1, 1, 2, 2, 3, 3, 4, 4, 5, 5 };
            double[] factors = { 0.93, 0.865,0.804,0.748,0.696,0.647,0.602,0.56,0.52,0.484 };
            for(int i = 0; i < 10; ++i )
            {
                LocalMidiDurationDef snore = new LocalMidiDurationDef(palette[i / 2]);

                snore.MsPosition = msPosition;
                factor = factors[i];
                msDuration = snore.MsDuration * factor;
                snore.MsDuration = (int)msDuration;
                msPosition += snore.MsDuration;
                snore.Transpose(transpositions2[i]);
                //snore.UniqueMidiDurationDef.MidiVelocity = (byte)((double)snore.UniqueMidiDurationDef.MidiVelocity * factor * factor);
                snores.Add(snore);

                restDuration = 2500 / factor;
                LocalMidiDurationDef rest = new LocalMidiDurationDef(msPosition, (int)restDuration);
                msPosition += rest.MsDuration;
                snores.Add(rest);

            }

            snores[snores.Count - 1].MsDuration = clytemnestra.EndMsPosition - snores[snores.Count - 1].MsPosition;

            VoiceDef furies4 = new VoiceDef(snores);

            furies4.VelocitiesHairpin(13, furies4.Count, 0.3);

            #region alignments in Verse 1
            furies4.AlignObjectAtIndex(7, 8, 9, clytemnestra[3].MsPosition);
            furies4.AlignObjectAtIndex(8, 9, 10, clytemnestra[7].MsPosition);
            furies4.AlignObjectAtIndex(9, 10, 11, clytemnestra[16].MsPosition);
            furies4.AlignObjectAtIndex(10, 11, 12, clytemnestra[24].MsPosition);
            furies4.AlignObjectAtIndex(11, 12, 13, clytemnestra[39].MsPosition);
            furies4.AlignObjectAtIndex(12, 13, 14, clytemnestra[42].MsPosition);
            furies4.AlignObjectAtIndex(14, 34, furies4.Count, wind1[38].MsPosition);
            //furies4.AlignObjectAtIndex(15, 23, 37, wind1[28].MsPosition);
            #endregion

            furies4.RemoveScorePitchWheelCommands(0, 12);

            return furies4;
        }
    }
}
