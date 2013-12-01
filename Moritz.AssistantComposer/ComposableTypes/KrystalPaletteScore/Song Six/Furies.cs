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

        private VoiceDef GetFury4(VoiceDef wind3, PaletteDef palette)
        {
            List<LocalMidiDurationDef> snores = new List<LocalMidiDurationDef>();
            int msPosition = 0;

            int paddingRestMsDuration = 3000;
            LocalMidiDurationDef paddingRest = new LocalMidiDurationDef(msPosition, paddingRestMsDuration);
            snores.Add(paddingRest);
            msPosition += paddingRestMsDuration;

            for(int i = 0; i < 5; ++i)
            {
                LocalMidiDurationDef snore = new LocalMidiDurationDef(palette[i]);
                snore.MsPosition = msPosition;
                msPosition += snore.MsDuration;
                snores.Add(snore);

                LocalMidiDurationDef rest = new LocalMidiDurationDef(msPosition,1000);
                msPosition += rest.MsDuration;
                snores.Add(rest);
            }

            paddingRestMsDuration = wind3.EndMsPosition - msPosition;
            paddingRest = new LocalMidiDurationDef(msPosition, paddingRestMsDuration);
            snores.Add(paddingRest);

            VoiceDef fury4 = new VoiceDef(snores);

            //fury4.AlignObjectAtIndex(0, 5, fury1.Count-1, wind3[1].MsPosition);

            return fury4;
        }
    }
}
