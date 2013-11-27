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
            return new List<byte>() { 0, 1, 2, 3, 4, 5 };
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
            // Palettes: projected contents:
            // palette 1 (_paletteDefs[0]): wind definitions.
            // palette 2 (_paletteDefs[1]): very high furies - models and definitions
            // palette 3 (_paletteDefs[2]): high furies - models and definitions
            // palette 4 (_paletteDefs[3]): low furies - models and definitions
            // palette 5 (_paletteDefs[4]): very low furies - models and definitions

            // The wind3 is the lowest wind. The winds are numbered from top to bottom in the score.
            VoiceDef wind3 = GetWind3(_paletteDefs[0], _krystals[2]);
            
            Clytemnestra clytemnestra = new Clytemnestra(wind3);

            VoiceDef wind2 = GetWind2(wind3, clytemnestra);
            VoiceDef wind1 = GetWind1(wind3, clytemnestra);

            // WindPitchWheelDeviations change per section in Song Six
            AdjustWindPitchWheelDeviations(wind1);
            AdjustWindPitchWheelDeviations(wind2);
            AdjustWindPitchWheelDeviations(wind3);
            
            // Construct the Furies.
            VoiceDef fury1 = GetFury1(wind3, _paletteDefs[1]);

            VoiceDef control = GetControlVoiceDef(fury1, clytemnestra, wind1, wind2, wind3);

            #region code for testing VoiceDef functions
            //bassWind.SetContour(11, new List<int>() { 1, 4, 1, 2 }, 1, 1);
            //bassWind.Translate(15, 4, 16);
            // TODO:
            // Cut, Copy, PasteAt (List<LocalMididurationDefs>) !!
            #endregion

            // Add each voiceDef to voiceDefs here, in top to bottom (=channelIndex) order in the score.
            List<VoiceDef> voiceDefs = new List<VoiceDef>() {control, fury1, clytemnestra, wind1, wind2, wind3 /* etc.*/};
            Debug.Assert(voiceDefs.Count == MidiChannels().Count);
            foreach(VoiceDef voiceDef in voiceDefs)
            {
                voiceDef.SetLyricsToIndex();
            }
            List<int> barlineMsPositions = GetBarlineMsPositions(control, clytemnestra, wind1, wind2, wind3 /* etc.*/);
            // this system contains one Voice per channel (not divided into bars)
            List<Voice> system = GetVoices(voiceDefs);
            List<List<Voice>> bars = GetBars(system, barlineMsPositions);

            return bars;
        }

        private void AdjustWindPitchWheelDeviations(VoiceDef wind)
        {
            byte? versePwdValue = 3;
            double windStartPwdValue = 12, windEndPwdValue=28;
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
        private List<int> GetBarlineMsPositions(VoiceDef control, Clytemnestra clytemnestra, VoiceDef wind1, VoiceDef wind2, VoiceDef wind3 /* etc.*/)
        {
            VoiceDef ctl = control;
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
                c[59].MsPosition,
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
                c[116].MsPosition,
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
                c[173].MsPosition,
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
                c[268].MsPosition,
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
                #region finale
                c[289].MsPosition,
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
        /// The control VoiceDef consists of single note + rest pairs,
        /// whose msPositions and msDurations are composed here.
        /// </summary>
        private VoiceDef GetControlVoiceDef(VoiceDef fury1, Clytemnestra clytemnestra, VoiceDef wind1, VoiceDef wind2, VoiceDef wind3)
        {
            VoiceDef f1 = fury1;
            VoiceDef w1 = wind1;
            VoiceDef w2 = wind2;
            VoiceDef w3 = wind3;
            VoiceDef c = clytemnestra;
            // The control note msPositions and following rest msDurations.
            // The columns here are note MsPositions and rest MsDurations respectively.
            // A rest's MsPosition is found by subtracting its MsDuration from the following note msPosition.
            List<int> controlNoteAndRestInfo = new List<int>()
            {
                #region positions (in temporal order)
                #region introduction
                0, 800,
                f1[1].MsPosition, f1[2].MsDuration / 2,
                f1[3].MsPosition, f1[4].MsDuration / 2,
                f1[5].MsPosition, f1[6].MsDuration / 2,
                f1[7].MsPosition, f1[8].MsDuration / 2,
                f1[9].MsPosition, w3[3].MsPosition - w1[3].MsPosition,

                w3[3].MsPosition, 100,
                w3[5].MsPosition, 100,
                #endregion
                #region verse 1
                c[1].MsPosition,  100,
                c[3].MsPosition,  100,
                c[7].MsPosition,  100,
                c[14].MsPosition, 100,
                c[17].MsPosition, 100,
                c[24].MsPosition, 100,
                c[31].MsPosition, 100,
                c[40].MsPosition, 100,
                c[49].MsPosition, 100,
                #endregion
                #region interlude after verse 1
                c[59].MsPosition,  100,
                w2[16].MsPosition, 100,
                w2[18].MsPosition, 100,
                #endregion
                #region verse 2
                c[60].MsPosition, 100,
                c[62].MsPosition, 100,
                c[66].MsPosition, 100,
                c[83].MsPosition, 100,
                c[94].MsPosition, 100,
                c[99].MsPosition, 100,
                #endregion
                #region interlude after verse 2
                c[106].MsPosition, 100,
                c[116].MsPosition, 100,
                w1[26].MsPosition, 100,
                w1[28].MsPosition, 100,
                #endregion
                #region verse 3
                c[117].MsPosition, 100,
                c[119].MsPosition, 100,
                c[123].MsPosition, 100,
                c[130].MsPosition, 100,
                c[141].MsPosition, 100,
                c[152].MsPosition, 100,
                c[163].MsPosition, 100,
                #endregion
                #region interlude after verse 3
                c[173].MsPosition, 100,
                w3[40].MsPosition, 100,
                w3[45].MsPosition, 100,
                #endregion
                #region verse 4
                c[174].MsPosition, 100,
                c[185].MsPosition, 100,
                c[216].MsPosition, 100,
                c[235].MsPosition, 100,
                c[255].MsPosition, 100,
                #endregion
                #region interlude after verse 4
                c[268].MsPosition, 100,
                w3[63].MsPosition, 100,
                #endregion
                #region verse 5
                c[269].MsPosition, 100,
                c[278].MsPosition, 100,
                c[288].MsPosition, 100,
                #endregion
                #region finale
                c[289].MsPosition, 100,
                w3[77].MsPosition, w3[81].MsDuration / 2,
                #endregion
                w3.EndMsPosition // final barline position
                #endregion
            };

            #region check consistency of controlNoteAndRestInfo
            for(int i = 0; i < controlNoteAndRestInfo.Count - 3; i += 2)
            {
                int noteMsPosition = controlNoteAndRestInfo[i];
                int restMsDuration = controlNoteAndRestInfo[i + 1];
                int nextNoteMsPosition = controlNoteAndRestInfo[i + 2];
                int restMsPosition = nextNoteMsPosition - restMsDuration;
                int noteMsDuration = restMsPosition - noteMsPosition;

                Debug.Assert(nextNoteMsPosition > noteMsPosition);
                Debug.Assert(restMsPosition > noteMsPosition);
                Debug.Assert(noteMsDuration > 0 && restMsDuration > 0);
            }
            #endregion

            List<LocalMidiDurationDef> controlLmdds = new List<LocalMidiDurationDef>();

            for(int i = 0; i < controlNoteAndRestInfo.Count - 2; i += 2)
            {
                int noteMsPosition = controlNoteAndRestInfo[i];
                int restMsDuration = controlNoteAndRestInfo[i + 1]; 
                int nextNoteMsPosition = controlNoteAndRestInfo[i + 2];
                int restMsPosition = nextNoteMsPosition - restMsDuration;
                int noteMsDuration = restMsPosition - noteMsPosition;
                
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
