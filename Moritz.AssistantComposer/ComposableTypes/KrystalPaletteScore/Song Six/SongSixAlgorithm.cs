using System.Collections.Generic;
using System.Diagnostics;
using System;

using Moritz.Score;
using Moritz.Score.Midi;
using Krystals4ObjectLibrary;

namespace Moritz.AssistantComposer
{
    /// <summary>
    /// The Algorithm for Song 6.
    /// This will develope as composition progresses...
    /// </summary>
    internal partial class SongSixAlgorithm : MidiCompositionAlgorithm
    {
        public SongSixAlgorithm(List<Krystal> krystals, List<PaletteDef> paletteDefs)
            : base(krystals, paletteDefs)
        {
        }

        /// <summary>
        /// The values are then checked for consistency in the base constructor.
        /// </summary>
        public override List<byte> MidiChannels()
        {
            return new List<byte>() { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        }

        /// <summary>
        /// The number of bars produced by DoAlgorithm().
        /// </summary>
        /// <returns></returns>
        public override int NumberOfBars()
        {
            return 96;
        }

        /// <summary>
        /// Sets the midi content of the score, independent of its notation.
        /// This means adding LocalMidiDurationDefs to each VoiceDef's LocalMidiDurationDefs list.
        /// The LocalMidiDurationDefs will later be transcribed into a particular notation by a Notator.
        /// Notations are independent of the midi info.
        /// This DoAlgorithm() function is special to this composition.
        /// </summary>
        /// <returns>
        /// A list of sequential bars. Each bar contains all the voices in the bar, from top to bottom.
        /// </returns>
        public override List<List<Voice>> DoAlgorithm()
        {
            // Palettes contain:
            // -- begin to be used in prelude (exposition in prelude)
            // palette 1 : winds [domain 7] (used, dont change!)
            // palette 2 : furies4 snores [domain 7] (used, dont change!)
            //
            // -- can begin to be used in verse 1 (exposition in interlude 1)
            // palette 3 : furies3 feathers [domain 7] (used, dont change!)
            // palette 4 : furies4 growls [domain 7] (index 0,2,5,1 used)
            // palette 5 : furies2 ticks [domain 7] (used, dont change!)
            //
            // -- "bird sounds" from Interlude2 and verse 3
            // palette 6 : furies4 songs [domain 12] 
            // palette 7 : furies3 chirps1 [domain 12] 
            // palette 8 : furies2 chirps2 [domain 12] 
            // palette 9 : furies1 cheeps [domain 12] (4,8,2,6,10 used, dont change!) 

            // All palettes can be accessed here at _paletteDefs[ paletteNumber - 1 ].

            // The wind3 is the lowest wind. The winds are numbered from top to bottom in the score.
            VoiceDef wind3 = GetWind3(_paletteDefs[0], _krystals[2]);
            
            Clytemnestra clytemnestra = new Clytemnestra(wind3);

            VoiceDef wind2 = GetWind2(wind3, clytemnestra);
            VoiceDef wind1 = GetWind1(wind3, wind2, clytemnestra);

            AdjustFinalWindChordPosition(wind1, wind2, wind3); // "fermata"

            // WindPitchWheelDeviations change approximately per section in Song Six
            AdjustWindPitchWheelDeviations(wind1);
            AdjustWindPitchWheelDeviations(wind2);
            AdjustWindPitchWheelDeviations(wind3);

            AdjustWindVelocities(wind1, wind2, wind3);

            // contouring test code
            //wind1.SetContour(2, new List<int>() { 1, 1, 1 }, 12, 1);
            
            // Construct the Furies.
            VoiceDef furies4 = GetFuries4(wind3[0].MsDuration / 2, clytemnestra, wind1, _paletteDefs);
            VoiceDef furies3 = GetFuries3(wind1[15].MsPosition, clytemnestra, wind1, _paletteDefs);
            VoiceDef furies2 = GetFuries2(clytemnestra, wind1, furies3, _paletteDefs);// steals ticks from furies3
            VoiceDef furies1 = GetFuries1(clytemnestra, wind1, furies3, furies2, _paletteDefs);
            GetFuries3ChirpsInInterlude2AndVerse3(furies1, furies2, furies3, clytemnestra, wind1, _paletteDefs[6]);

            // contouring test code 
            // fury1.SetContour(1, new List<int>(){2,2,2,2,2}, 1, 6);

            VoiceDef control = GetControlVoiceDef(furies1, furies2, furies3, furies4, clytemnestra, wind1, wind2, wind3);

            //furies1.AgglomerateRests();
            //furies2.AgglomerateRests();
            //furies3.AgglomerateRests();
            //furies4.AgglomerateRests();

            // Add each voiceDef to voiceDefs here, in top to bottom (=channelIndex) order in the score.
            List<VoiceDef> voiceDefs = new List<VoiceDef>() { furies1, furies2, furies3, furies4, control, clytemnestra, wind1, wind2, wind3 };
            Debug.Assert(voiceDefs.Count == MidiChannels().Count);
            foreach(VoiceDef voiceDef in voiceDefs)
            {
                voiceDef.SetLyricsToIndex();
            }
            List<int> barlineMsPositions = GetBarlineMsPositions(control, furies1, furies2, furies3, furies4, clytemnestra, wind1, wind2, wind3);
            // this system contains one Voice per channel (not divided into bars)
            List<Voice> system = GetVoices(voiceDefs);
            List<List<Voice>> bars = GetBars(system, barlineMsPositions);

            return bars;
        }

        private void AdjustFinalWindChordPosition(VoiceDef wind1, VoiceDef wind2, VoiceDef wind3)
        {
            wind1.AlignObjectAtIndex(71, 81, 82, wind1[81].MsPosition - (wind1[81].MsDuration / 2));
            wind2.AlignObjectAtIndex(71, 81, 82, wind2[81].MsPosition - (wind2[81].MsDuration / 2));
            wind3.AlignObjectAtIndex(71, 81, 82, wind3[81].MsPosition - (wind3[81].MsDuration / 2));
        }

        private void AdjustWindVelocities(VoiceDef wind1, VoiceDef wind2, VoiceDef wind3)
        {
            int beginInterlude2DimIndex = 25; // start of Interlude2
            int beginVerse3DimIndex = 31; // non-inclusive
            int beginVerse5CrescIndex = 70;
            int beginPostludeIndex = 74;

            wind1.VelocitiesHairpin(beginInterlude2DimIndex, beginVerse3DimIndex, 0.5);
            wind2.VelocitiesHairpin(beginInterlude2DimIndex, beginVerse3DimIndex, 0.5);
            wind3.VelocitiesHairpin(beginInterlude2DimIndex, beginVerse3DimIndex, 0.5);

            wind1.VelocitiesHairpin(beginVerse5CrescIndex, beginPostludeIndex, 2);
            wind2.VelocitiesHairpin(beginVerse5CrescIndex, beginPostludeIndex, 2);
            wind3.VelocitiesHairpin(beginVerse5CrescIndex, beginPostludeIndex, 2);

            wind1.VelocitiesHairpin(beginPostludeIndex, wind1.Count, 2);
            wind2.VelocitiesHairpin(beginPostludeIndex, wind2.Count, 2);
            wind3.VelocitiesHairpin(beginPostludeIndex, wind3.Count, 2);
        }

        private void AdjustWindPitchWheelDeviations(VoiceDef wind)
        {
            byte? versePwdValue = 3;
            double windStartPwdValue = 6, windEndPwdValue=28;
            double pwdfactor = Math.Pow(windEndPwdValue/windStartPwdValue, (double)1/5); // 5th root of windEndPwdValue/windStartPwdValue -- the last pwd should be windEndPwdValue

            for(int i = 0; i < wind.Count; ++i)
            {
                UniqueMidiChordDef umcd = wind[i].UniqueMidiDurationDef as UniqueMidiChordDef;
                if(umcd != null)
                {
                    if(i < 8) //prelude
                    {
                        umcd.PitchWheelDeviation = (byte?) windStartPwdValue;
                    }
                    else if(i < 15) // verse 1
                    {
                        umcd.PitchWheelDeviation = versePwdValue;
                    }
                    else if(i < 20) // interlude 1
                    {
                        umcd.PitchWheelDeviation = (byte?)(windStartPwdValue * pwdfactor);
                    }
                    else if(i < 24) // verse 2
                    {
                        umcd.PitchWheelDeviation = versePwdValue;
                    }
                    else if(i < 33) // interlude 2
                    {
                        umcd.PitchWheelDeviation = (byte?)(windStartPwdValue * (Math.Pow(pwdfactor, 2)));
                    }
                    else if(i < 39) // verse 3
                    {
                        umcd.PitchWheelDeviation = versePwdValue;
                    }
                    else if(i < 49) // interlude 3
                    {
                        umcd.PitchWheelDeviation = (byte?)(windStartPwdValue * (Math.Pow(pwdfactor, 3)));
                    }
                    else if(i < 57) // verse 4
                    {
                        umcd.PitchWheelDeviation = versePwdValue;
                    }
                    else if(i < 70) // interlude 4
                    {
                        umcd.PitchWheelDeviation = (byte?)(windStartPwdValue * (Math.Pow(pwdfactor, 4)));
                    }
                    else if(i < 74) // verse 5
                    {
                        umcd.PitchWheelDeviation = versePwdValue;
                    }
                    else // postlude
                    {
                        umcd.PitchWheelDeviation = (byte?)(windStartPwdValue * (Math.Pow(pwdfactor, 5)));
                    }
                }                
            }
        }

        /// <summary>
        /// The returned barlineMsPositions contain both the position of bar 1 (0ms) and the position of the final barline.
        /// </summary>
        private List<int> GetBarlineMsPositions(VoiceDef control, VoiceDef fury1, VoiceDef fury2, VoiceDef fury3, VoiceDef fury4, Clytemnestra clytemnestra, VoiceDef wind1, VoiceDef wind2, VoiceDef wind3)
        {
            VoiceDef ctl = control;
            VoiceDef f1 = fury1;
            VoiceDef f2 = fury2;
            VoiceDef f3 = fury3;
            VoiceDef f4 = fury4;
            Clytemnestra c = clytemnestra;
            VoiceDef w1 = wind1;
            VoiceDef w2 = wind2;
            VoiceDef w3 = wind3;

            List<int> barlineMsPositions = new List<int>()
            {
                #region msPositions
                #region intro
                0,
                w3[1].MsPosition,
                w3[3].MsPosition,
                w3[5].MsPosition,
                #endregion
                #region verse 1
                c[1].MsPosition,
                c[3].MsPosition,
                c[8].MsPosition,
                c[12].MsPosition,
                c[15].MsPosition,
                c[18].MsPosition,
                c[22].MsPosition,
                c[27].MsPosition,
                c[34].MsPosition,
                c[38].MsPosition,
                c[41].MsPosition,
                c[47].MsPosition,
                c[49].MsPosition,
                c[50].MsPosition,
                c[54].MsPosition,
                c[58].MsPosition,
                #endregion
                #region interlude after verse 1
                w2[15].MsPosition,
                w2[16].MsPosition,
                w2[18].MsPosition,
                #endregion
                #region verse 2
                c[60].MsPosition,
                c[62].MsPosition,
                c[67].MsPosition,
                c[71].MsPosition,
                c[73].MsPosition,
                c[77].MsPosition,
                c[81].MsPosition,
                c[86].MsPosition,
                c[88].MsPosition,
                c[92].MsPosition,
                c[94].MsPosition,
                c[97].MsPosition,
                c[100].MsPosition,
                c[104].MsPosition,
                c[107].MsPosition,
                c[111].MsPosition,
                c[115].MsPosition,
                #endregion
                #region interlude after verse 2
                w1[25].MsPosition,
                w1[26].MsPosition,
                w1[28].MsPosition,
                w1[30].MsPosition,
                #endregion
                #region verse 3
                c[117].MsPosition,
                c[119].MsPosition,
                c[124].MsPosition,
                c[126].MsPosition,
                c[128].MsPosition,
                c[131].MsPosition,
                c[135].MsPosition,
                c[139].MsPosition,
                c[141].MsPosition,
                c[146].MsPosition,
                c[148].MsPosition,
                c[152].MsPosition,
                c[159].MsPosition,
                c[164].MsPosition,
                c[168].MsPosition,
                c[172].MsPosition,
                #endregion
                #region interlude after verse 3
                w1[38].MsPosition,
                w3[40].MsPosition,
                w3[45].MsPosition,
                #endregion
                #region verse 4, Oft have ye...
                c[174].MsPosition,
                c[177].MsPosition,
                c[183].MsPosition,
                c[185].MsPosition,
                c[192].MsPosition,
                c[196].MsPosition,
                c[204].MsPosition,
                c[206].MsPosition,
                c[214].MsPosition,
                c[219].MsPosition,
                c[221].MsPosition,
                c[225].MsPosition,
                c[227].MsPosition,
                c[229].MsPosition,
                c[233].MsPosition,
                c[236].MsPosition,
                c[242].MsPosition,
                c[252].MsPosition,
                c[257].MsPosition,
                c[259].MsPosition,
                c[263].MsPosition,
                c[267].MsPosition,
                #endregion
                #region interlude after verse 4
                w1[57].MsPosition,
                w3[63].MsPosition,
                #endregion
                #region verse 5
                c[269].MsPosition,
                c[270].MsPosition,
                c[272].MsPosition,
                c[276].MsPosition,
                c[279].MsPosition,
                c[283].MsPosition,
                c[288].MsPosition,
                #endregion
                #region postlude
                w3[74].MsPosition,
                w3[77].MsPosition,
                #endregion
                // final barline
                w3.EndMsPosition
                #endregion
            };

            Debug.Assert(barlineMsPositions.Count == NumberOfBars() + 1); // includes bar 1 (mPos=0) and the final barline.

            return barlineMsPositions;
        }

        /// <summary>
        /// These barlines do not include the barlines at the beginning, middle or end of Clytemnestra's verses.
        /// </summary>
        /// <param name="wind3"></param>
        /// <param name="barlineMsPositions"></param>
        /// <returns></returns>
        private List<int> AddInterludeBarlinePositions(VoiceDef wind3, List<int> barlineMsPositions)
        {
            List<int> newBarlineIndices = new List<int>() { 1, 3, 5, 15, 27, 40, 45, 63, 77 }; // by inspection of the score
            foreach(int index in newBarlineIndices)
            {
                barlineMsPositions.Add(wind3.LocalMidiDurationDefs[index].MsPosition);
            }
            barlineMsPositions.Sort();

            return barlineMsPositions;
        }

        /// <summary>
        /// Returns a VoiceDef containing a single rest having msDuration
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private VoiceDef GetEmptyVoiceDef(int msDuration)
        {
            List<LocalMidiDurationDef> lmdds = new List<LocalMidiDurationDef>();
            LocalMidiDurationDef lmRestDef = new LocalMidiDurationDef(0, msDuration);
            lmdds.Add(lmRestDef);
            VoiceDef emptyVoiceDef = new VoiceDef(lmdds);
            return emptyVoiceDef;
        }

        /// <summary>
        /// The control VoiceDef consists of single note + rest pairs whose msPositions are composed here.
        /// </summary>
        private VoiceDef GetControlVoiceDef(VoiceDef furies1, VoiceDef furies2, VoiceDef furies3, VoiceDef furies4, Clytemnestra clytemnestra, VoiceDef wind1, VoiceDef wind2, VoiceDef wind3)
        {
            VoiceDef f1 = furies1;
            VoiceDef f2 = furies2;
            VoiceDef f3 = furies3;
            VoiceDef f4 = furies4;
            VoiceDef w1 = wind1;
            VoiceDef w2 = wind2;
            VoiceDef w3 = wind3;
            VoiceDef c = clytemnestra;

            // The columns here are note MsPositions and rest MsPositions respectively.
            List<int> controlNoteAndRestMsPositions = new List<int>()
            {
                0, f4[1].MsPosition / 2, 
                f4[1].MsPosition, f4[2].MsPosition, 
                f4[3].MsPosition, f4[4].MsPosition, 
                f4[5].MsPosition, f4[6].MsPosition,
                f4[7].MsPosition, f4[8].MsPosition, // verse 1 starts inside f4[7] 
                f4[9].MsPosition, f4[10].MsPosition,
                f4[11].MsPosition, f4[12].MsPosition,
                c[49].MsPosition, (c[58].MsPosition + c[59].MsPosition) / 2,

                // Interlude 1
                f3[1].MsPosition, f3[12].MsPosition,
                f3[13].MsPosition, f3[24].MsPosition,

                // Verse 2
                c[60].MsPosition, c[61].MsPosition,
                c[62].MsPosition, c[65].MsPosition,
                c[66].MsPosition, c[82].MsPosition,
                c[83].MsPosition, c[93].MsPosition,
                c[94].MsPosition, c[98].MsPosition,
                c[99].MsPosition, c[105].MsPosition,
                c[106].MsPosition, c[116].MsPosition,

                // Interlude 2
                f3[61].MsPosition, f3[72].MsPosition,
                f1[1].MsPosition, f1[2].MsPosition,
                f1[3].MsPosition, f2[18].MsPosition,
                f3[96].MsPosition, f1[5].MsPosition,
                f2[29].MsPosition, f3[114].MsPosition,
                f4[29].MsPosition, f1[10].MsPosition,
                f3[131].MsPosition, f2[47].MsPosition,

                // Verse 3
                c[117].MsPosition, c[118].MsPosition,
                c[119].MsPosition, c[122].MsPosition,
                c[123].MsPosition, c[129].MsPosition,
                c[130].MsPosition, c[140].MsPosition,
                c[141].MsPosition, c[151].MsPosition,
                c[152].MsPosition, c[162].MsPosition,
                c[163].MsPosition, c[173].MsPosition,



                w3.EndMsPosition // final barline position
            };

            VoiceDef controlVoiceDef = MakeControlVoiceDef(controlNoteAndRestMsPositions);
            return controlVoiceDef;
        }

        // This code should not change while composing the ControlVoiceDef.
        // It just makes the VoiceDef from the controlNoteAndRestMsPositions defined above.
        private static VoiceDef MakeControlVoiceDef(List<int> controlNoteAndRestMsPositions)
        {
            List<LocalMidiDurationDef> controlLmdds = new List<LocalMidiDurationDef>();

            for(int i = 0; i < controlNoteAndRestMsPositions.Count - 2; i += 2)
            {
                int noteMsPosition = controlNoteAndRestMsPositions[i];
                int restMsPosition = controlNoteAndRestMsPositions[i + 1];
                int nextNoteMsPosition = controlNoteAndRestMsPositions[i + 2];

                Debug.Assert(noteMsPosition < restMsPosition && restMsPosition < nextNoteMsPosition);

                int noteMsDuration = restMsPosition - noteMsPosition;
                int restMsDuration = nextNoteMsPosition - restMsPosition;

                UniqueMidiChordDef umcd = new UniqueMidiChordDef(new List<byte>() { (byte)67 }, new List<byte>() { (byte)0 }, noteMsDuration, false, new List<MidiControl>());
                LocalMidiDurationDef lmChordd = new LocalMidiDurationDef(umcd, noteMsPosition, noteMsDuration);

                LocalMidiDurationDef lmRestd = new LocalMidiDurationDef(restMsPosition, restMsDuration);

                controlLmdds.Add(lmChordd);
                controlLmdds.Add(lmRestd);
            }
            VoiceDef controlVoiceDef = new VoiceDef(controlLmdds);
            return controlVoiceDef;
        }

        private List<Voice> GetVoices(List<VoiceDef> voiceDefs)
        {
            byte channelIndex = 0;
            List<Voice> voices = new List<Voice>();

            foreach(VoiceDef voiceDef in voiceDefs)
            {
                Voice voice = new Voice(null, channelIndex++);
                voice.LocalMidiDurationDefs = voiceDef.LocalMidiDurationDefs;
                voices.Add(voice);
            }

            return voices;
        }

        private List<List<Voice>> GetBars(List<Voice> system, List<int> barlineMsPositions)
        {
            // barlineMsPositions contains both msPos=0 and the position of the final barline
            List<List<Voice>> bars = new List<List<Voice>>();
            bars = GetBarsFromBarlineMsPositions(system, barlineMsPositions);
            Debug.Assert(bars.Count == NumberOfBars());
            return bars;
        }

        /// <summary>
        /// Splits the voices (currently in a single bar) into bars
        /// barlineMsPositions contains both msPosition 0, and the position of the final barline.
        /// </summary>
        private List<List<Voice>> GetBarsFromBarlineMsPositions(List<Voice> voices, List<int> barLineMsPositions)
        {
            List<List<Voice>> bars = new List<List<Voice>>();
            List<List<Voice>> twoBars = null;

            for(int i = barLineMsPositions.Count - 2; i >= 1; --i)
            {
                twoBars = SplitBar(voices, barLineMsPositions[i]);
                bars.Insert(0, twoBars[1]);
                voices = twoBars[0];
            }
            bars.Insert(0, twoBars[0]);

            return bars;
        }
    }
}
