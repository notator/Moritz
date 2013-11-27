using System.Collections.Generic;
using System.Diagnostics;
using System;

using Moritz.Score;
using Moritz.Score.Midi;
using Krystals4ObjectLibrary;

namespace Moritz.AssistantComposer
{
    /// <summary>
    /// The Wind "constructors" (part of the Song Six algorithm).
    /// </summary>
    internal partial class SongSixAlgorithm : MidiCompositionAlgorithm
    {
        private VoiceDef GetWind3(PaletteDef palette, Krystal krystal)
        {
            VoiceDef wind3 = new VoiceDef(palette, krystal);
            wind3.Transpose(0, wind3.Count, -13);
            wind3.StepwiseGliss(74, wind3.Count, 19);
            return wind3;
        }

        private VoiceDef GetWind2(VoiceDef wind3, Clytemnestra clytemnestra)
        {
            List<LocalMidiDurationDef> clytLmdds = clytemnestra.LocalMidiDurationDefs;
            int rotationMsPosition = clytLmdds[59].MsPosition;

            VoiceDef wind2 = GetRotatedWind(wind3, rotationMsPosition);
            wind2.Transpose(0, wind2.Count, 12);
            wind2.StepwiseGliss(0, 15, 7);
            wind2.Transpose(15, wind2.Count, 7);
            wind2.StepwiseGliss(75, wind2.Count, 12);

            wind2.AlignObjectAtIndex(0, 15, 82, rotationMsPosition);
            wind2.AlignObjectAtIndex(15, 39, 82, clytLmdds[173].MsPosition);
            wind2.AlignObjectAtIndex(39, 57, 82, clytLmdds[268].MsPosition);

            return wind2;
        }

        //private void RemovePitchBendCommands(LocalMidiDurationDef lmdd)
        //{
        //    UniqueMidiChordDef umcd = lmdd.UniqueMidiDurationDef as UniqueMidiChordDef;
        //    if(umcd != null)
        //    {
        //        umcd.PitchWheelDeviation = 24;
        //        umcd.MidiChordSliderDefs.PitchWheelMsbs.Clear();
        //    }
        //}

        /// <summary>
        /// Pitchwheel commands have been removed from Wind 1.
        /// </summary>
        /// <param name="wind3"></param>
        /// <param name="clytemnestra"></param>
        /// <returns></returns>
        private VoiceDef GetWind1(VoiceDef wind3, Clytemnestra clytemnestra)
        {
            List<LocalMidiDurationDef> clytLmdds = clytemnestra.LocalMidiDurationDefs;
            int rotationMsPosition = clytLmdds[116].MsPosition;

            VoiceDef wind1 = GetRotatedWind(wind3, rotationMsPosition);
            wind1.Transpose(0, wind1.Count, 19);
            wind1.StepwiseGliss(0, 25, 12);
            wind1.Transpose(25, wind1.Count, 12);

            wind1.AlignObjectAtIndex(0, 25, 82, rotationMsPosition);
            wind1.AlignObjectAtIndex(25, 74, 82, clytLmdds[289].MsPosition);

            //// Remove the PitchBend Commands before Verse 1
            //for(int i = 0; i < 9; ++i)
            //{
            //    RemovePitchBendCommands(wind1[i]);
            //}

            //SetPitchWheelDeviation(wind1[9], 2);

            return wind1;
        }

        /// <summary>
        /// Returns a VoiceDef containing clones of the LocalMidiDurationDefs in the originalVoiceDef argument,
        /// rotated so that the original first LocalMidiDurationDef is positioned close to rotationMsPosition.
        /// </summary>
        /// <param name="originalVoiceDef"></param>
        /// <returns></returns>
        private VoiceDef GetRotatedWind(VoiceDef originalVoiceDef, int rotationMsPosition)
        {
            VoiceDef tempWind = originalVoiceDef.Clone();
            int finalBarlineMsPosition = originalVoiceDef.EndMsPosition;
            int msDurationAfterSynch = finalBarlineMsPosition - rotationMsPosition;

            List<LocalMidiDurationDef> originalLmdds = tempWind.LocalMidiDurationDefs;
            List<LocalMidiDurationDef> originalStartLmdds = new List<LocalMidiDurationDef>();
            List<LocalMidiDurationDef> newWindLmdds = new List<LocalMidiDurationDef>();
            int accumulatingMsDuration = 0;
            for(int i = 0; i < tempWind.Count; ++i)
            {
                if(accumulatingMsDuration < msDurationAfterSynch)
                {
                    originalStartLmdds.Add(originalLmdds[i]);
                    accumulatingMsDuration += originalLmdds[i].MsDuration;
                }
                else
                {
                    newWindLmdds.Add(originalLmdds[i]);
                }
            }
            newWindLmdds.AddRange(originalStartLmdds);

            int msPosition = 0;
            foreach(LocalMidiDurationDef lmdd in newWindLmdds)
            {
                lmdd.MsPosition = msPosition;
                msPosition += lmdd.MsDuration;
            }
            VoiceDef newRotatedWind = new VoiceDef(newWindLmdds);

            return newRotatedWind;
        }
    }
}
