using System.Collections.Generic;
using System.Diagnostics;
using System;

using Krystals4ObjectLibrary;
using Moritz.Algorithm;
using Moritz.Palettes;
using Moritz.Spec;
using Moritz.Globals;

namespace Moritz.Algorithm.SongSix
{
    /// <summary>
    /// The Algorithm for Song 6.
    /// This will develop as composition progresses...
    /// </summary>
    public partial class SongSixAlgorithm : CompositionAlgorithm
    {
        public SongSixAlgorithm()
            : base()
        {
        }

        public override IReadOnlyList<int> MidiChannelIndexPerOutputVoice { get { return new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7 }; } }
        public override IReadOnlyList<int> MasterVolumePerOutputVoice { get { return new List<int>() { 100, 100, 100, 100, 100, 100, 100, 100 }; } }
        public override int NumberOfInputVoices { get { return 0; } }
        public override int NumberOfBars { get { return 106; } }

        /// <summary>
        /// See CompositionAlgorithm.DoAlgorithm()
        /// </summary>
        public override List<List<VoiceDef>> DoAlgorithm(List<Krystal> krystals, List<Palette> palettes)
        {
            // Palette indices:
            // Winds use palette 0.
            // Furies use:
            //                       Interlude
            //         Prelude   1    2    3    4   Postlude
            //         ------------------------------------------
            // Furies1    -      -    8   12   16      20
            // Furies2    -      4    7   11   15      19
            // Furies3    -      2    6   10   14      18
            // Furies4    1      3    5    9   13      17
            // All palettes can be accessed here at _paletteDefs[ paletteIndex ].
            // The wind3 is the lowest wind. The winds are numbered from top to bottom in the score.

            _krystals = krystals;
            _palettes = palettes;
            Trk wind3 = GetWind3(7,_palettes[0], _krystals[8]);
            Clytemnestra clytemnestra = new Clytemnestra(4, wind3);
            clytemnestra.AdjustVelocities(49, 59, 1.4);
            Trk wind2 = GetWind2(6, wind3, clytemnestra);
            Trk wind1 = GetWind1(5, wind3, wind2, clytemnestra);
            AdjustFinalWindChordPosition(wind1, wind2, wind3); // "fermata"
            // WindPitchWheelDeviations change approximately per section in Song Six
            AdjustWindPitchWheelDeviations(wind1);
            AdjustWindPitchWheelDeviations(wind2);
            AdjustWindPitchWheelDeviations(wind3);
            AdjustWindVelocities(wind1, wind2, wind3);
            Dictionary<string, int> msPositions = new Dictionary<string, int>();
            msPositions.Add("verse1", clytemnestra[1].MsPositionReTrk);
            msPositions.Add("interlude1", wind1[15].MsPositionReTrk);
            msPositions.Add("verse2", clytemnestra[60].MsPositionReTrk);
            msPositions.Add("interlude2", wind1[25].MsPositionReTrk);
            msPositions.Add("verse3", clytemnestra[117].MsPositionReTrk);
            msPositions.Add("interlude3", wind1[38].MsPositionReTrk);
            msPositions.Add("interlude3Bar2", wind3[40].MsPositionReTrk);
            msPositions.Add("verse4", clytemnestra[174].MsPositionReTrk);
            msPositions.Add("verse4EsCaped", clytemnestra[236].MsPositionReTrk);
            msPositions.Add("interlude4", wind1[57].MsPositionReTrk);
            msPositions.Add("interlude4End", wind3[65].MsPositionReTrk);
            msPositions.Add("verse5", clytemnestra[269].MsPositionReTrk);
            msPositions.Add("verse5Calls", clytemnestra[288].MsPositionReTrk);
            msPositions.Add("postlude", clytemnestra[289].MsPositionReTrk);
            msPositions.Add("postludeDiminuendo", wind1[80].MsPositionReTrk);
            msPositions.Add("finalWindChord", wind1[81].MsPositionReTrk);
            msPositions.Add("endOfPiece", wind1.EndMsPosition);
            // other positions are added as the voices are completed (see GetFuriesInterlude3ToEnd() )
            // contouring test code
            //wind1.SetContour(2, new List<int>() { 1, 1, 1 }, 12, 1);
            // Construct the Furies up to Interlude3.
            Furies4 furies4 = new Furies4(3, msPositions["endOfPiece"]);
            furies4.GetBeforeInterlude3(wind3[0].MsDuration / 2, clytemnestra, wind1, _palettes);
            Furies3 furies3 = new Furies3(2, msPositions["endOfPiece"]);
            furies3.GetBeforeInterlude3(msPositions["interlude1"], clytemnestra, wind1, _palettes);
            Furies2 furies2 = new Furies2(1, msPositions["endOfPiece"]);
            furies2.GetBeforeInterlude3(clytemnestra, wind1, furies3, _palettes);
            Furies1 furies1 = new Furies1(0, msPositions["endOfPiece"]);
            furies1.GetBeforeInterlude3(clytemnestra, wind1, furies2, _palettes[8]);
            furies3.GetChirpsInInterlude2AndVerse3(furies1, furies2, clytemnestra, wind1, _palettes[6]);
            GetFuriesInterlude3ToEnd(furies1, furies2, furies3, furies4, clytemnestra, wind1, wind2, wind3, _palettes, msPositions);
            // contouring test code 
            // fury1.SetContour(1, new List<int>(){2,2,2,2,2}, 1, 6);

            // Add each voiceDef to voiceDefs here, in top to bottom (=channelIndex) order in the score.
            List<VoiceDef> voiceDefs = new List<VoiceDef>() { furies1, furies2, furies3, furies4, clytemnestra, wind1, wind2, wind3 };
            Debug.Assert(voiceDefs.Count == MidiChannelIndexPerOutputVoice.Count);
            //********************************************************
            //foreach(VoiceDef voiceDef in voiceDefs)
            //{
            //    voiceDef.SetLyricsToIndex();
            //}
            //********************************************************
            List<int> barlineMsPositions = GetBarlineMsPositions(furies1, furies2, furies3, furies4, clytemnestra, wind1, wind2, wind3);
            InsertClefChanges(furies1, furies2, furies3, furies4);
            List<List<VoiceDef>> bars = GetBars(voiceDefs, barlineMsPositions);
            return bars;
        }
        /// <summary>
        /// Note that clef changes must be inserted backwards per voiceDef, so that IUniqueDef
        /// indices are correct. Inserting a clef change changes the indices.
        /// Note also that if a ClefChange is defined here on a RestDef which has no MidiChordDef
        /// to its right on the staff, the resulting ClefSymbol will be placed immediately before the final barline
        /// on the staff.
        /// ClefChanges which would happen at the beginning of a staff are written as cautionary clefs on the
        /// equivalent staff in the previous system.
        /// A ClefChange defined here on a MidiChordDef or RestDef which is eventually preceded
        /// by a barline, are placed to the left of the barline. 
        /// </summary>
        private void InsertClefChanges(Furies1 furies1, Furies2 furies2, Furies3 furies3, Furies4 furies4)
        {
            furies1.InsertClefChange(24, "t1"); // bar 60
            furies2.InsertClefChange(57, "t1"); // bar 60
            furies3.InsertClefChange(146, "t1"); // bar 60 
            furies3.InsertClefChange(131, "b");  // bar 44 
            furies3.InsertClefChange(114, "b1"); // bar 43
            furies3.InsertClefChange(112, "b");  // bar 43
            furies4.InsertClefChange(59, "b1"); // bar 104
        }
        private void AdjustFinalWindChordPosition(Trk wind1, Trk wind2, Trk wind3)
        {
            wind1.AlignObjectAtIndex(71, 81, 82, wind1[81].MsPositionReTrk - (wind1[81].MsDuration / 2));
            wind2.AlignObjectAtIndex(71, 81, 82, wind2[81].MsPositionReTrk - (wind2[81].MsDuration / 2));
            wind3.AlignObjectAtIndex(71, 81, 82, wind3[81].MsPositionReTrk - (wind3[81].MsDuration / 2));
        }
        private void AdjustWindVelocities(Trk wind1, Trk wind2, Trk wind3)
        {
            int beginInterlude2DimIndex = 25; // start of Interlude2
            int beginVerse3DimIndex = 31; // non-inclusive
            int beginVerse5CrescIndex = 70;
            int beginPostludeIndex = 74;
            wind1.AdjustVelocitiesHairpin(beginInterlude2DimIndex, beginVerse3DimIndex, 0.5);
            wind2.AdjustVelocitiesHairpin(beginInterlude2DimIndex, beginVerse3DimIndex, 0.5);
            wind3.AdjustVelocitiesHairpin(beginInterlude2DimIndex, beginVerse3DimIndex, 0.5);
            wind1.AdjustVelocitiesHairpin(beginVerse5CrescIndex, beginPostludeIndex, 2);
            wind2.AdjustVelocitiesHairpin(beginVerse5CrescIndex, beginPostludeIndex, 2);
            wind3.AdjustVelocitiesHairpin(beginVerse5CrescIndex, beginPostludeIndex, 2);
            wind1.AdjustVelocitiesHairpin(beginPostludeIndex, wind1.Count, 2.3);
            wind2.AdjustVelocitiesHairpin(beginPostludeIndex, wind2.Count, 2.3);
            wind3.AdjustVelocitiesHairpin(beginPostludeIndex, wind3.Count, 2.3);
        }
        private void AdjustWindPitchWheelDeviations(Trk wind)
        {
            byte versePwdValue = 3;
            double windStartPwdValue = 6, windEndPwdValue=28;
            double pwdfactor = Math.Pow(windEndPwdValue/windStartPwdValue, (double)1/5); // 5th root of windEndPwdValue/windStartPwdValue -- the last pwd should be windEndPwdValue
            MidiChordDef umcd = null;
            for(int i = 0; i < wind.Count; ++i)
            {
                if(i < 8) //prelude
                {
                    umcd = wind[i] as MidiChordDef;
                    if(umcd != null)
                        umcd.PitchWheelDeviation = M.MidiValue((int) windStartPwdValue);
                }
                else if(i < 15) // verse 1
                {
                    umcd = wind[i] as MidiChordDef;
                    if(umcd != null)
                        umcd.PitchWheelDeviation = versePwdValue;
                }
                else if(i < 20) // interlude 1
                {
                    umcd = wind[i] as MidiChordDef;
                    if(umcd != null)
                        umcd.PitchWheelDeviation = M.MidiValue((int)(windStartPwdValue * pwdfactor));
                }
                else if(i < 24) // verse 2
                {
                    umcd = wind[i] as MidiChordDef;
                    if(umcd != null)
                        umcd.PitchWheelDeviation = versePwdValue;
                }
                else if(i < 33) // interlude 2
                {
                    umcd = wind[i] as MidiChordDef;
                    if(umcd != null)
                        umcd.PitchWheelDeviation = M.MidiValue((int)(windStartPwdValue * (Math.Pow(pwdfactor, 2))));
                }
                else if(i < 39) // verse 3
                {
                    umcd = wind[i] as MidiChordDef;
                    if(umcd != null)
                        umcd.PitchWheelDeviation = versePwdValue;
                }
                else if(i < 49) // interlude 3
                {
                    umcd = wind[i] as MidiChordDef;
                    if(umcd != null)
                        umcd.PitchWheelDeviation = M.MidiValue((int)(windStartPwdValue * (Math.Pow(pwdfactor, 3))));
                }
                else if(i < 57) // verse 4
                {
                    umcd = wind[i] as MidiChordDef;
                    if(umcd != null)
                        umcd.PitchWheelDeviation = versePwdValue;
                }
                else if(i < 70) // interlude 4
                {
                    umcd = wind[i] as MidiChordDef;
                    if(umcd != null)
                        umcd.PitchWheelDeviation = M.MidiValue((int)(windStartPwdValue * (Math.Pow(pwdfactor, 4))));
                }
                else if(i < 74) // verse 5
                {
                    umcd = wind[i] as MidiChordDef;
                    if(umcd != null)
                        umcd.PitchWheelDeviation = versePwdValue;
                }
                else // postlude
                {
                    umcd = wind[i] as MidiChordDef;
                    if(umcd != null)
                        umcd.PitchWheelDeviation = M.MidiValue((int)(windStartPwdValue * (Math.Pow(pwdfactor, 5))));
                }            
            }
        }
        /// <summary>
        /// The returned barlineMsPositions contain both the position of bar 1 (0ms) and the position of the final barline.
        /// </summary>
        private List<int> GetBarlineMsPositions(Trk fury1, Trk fury2, Trk fury3, Trk fury4, Clytemnestra clytemnestra, Trk wind1, Trk wind2, Trk wind3)
        {
            Trk f1 = fury1;
            Trk f2 = fury2;
            Trk f3 = fury3;
            Trk f4 = fury4;
            Clytemnestra c = clytemnestra;
            Trk w1 = wind1;
            Trk w2 = wind2;
            Trk w3 = wind3;
            List<int> barlineMsPositions = new List<int>()
            {
                #region msPositions
                #region intro
                0,
                w3[1].MsPositionReTrk,
                w3[3].MsPositionReTrk,
                w3[5].MsPositionReTrk,
                #endregion
                #region verse 1
                c[1].MsPositionReTrk,
                c[3].MsPositionReTrk,
                c[8].MsPositionReTrk,
                c[12].MsPositionReTrk,
                c[15].MsPositionReTrk,
                c[18].MsPositionReTrk,
                c[22].MsPositionReTrk,
                c[27].MsPositionReTrk,
                c[34].MsPositionReTrk,
                c[38].MsPositionReTrk,
                c[41].MsPositionReTrk,
                c[47].MsPositionReTrk,
                c[49].MsPositionReTrk,
                c[50].MsPositionReTrk,
                c[54].MsPositionReTrk,
                c[58].MsPositionReTrk,
                #endregion
                #region interlude after verse 1
                w2[15].MsPositionReTrk,
                w2[16].MsPositionReTrk,
                w2[18].MsPositionReTrk,
                #endregion
                #region verse 2
                c[60].MsPositionReTrk,
                c[62].MsPositionReTrk,
                c[67].MsPositionReTrk,
                c[71].MsPositionReTrk,
                c[73].MsPositionReTrk,
                c[77].MsPositionReTrk,
                c[81].MsPositionReTrk,
                c[86].MsPositionReTrk,
                c[88].MsPositionReTrk,
                c[92].MsPositionReTrk,
                c[94].MsPositionReTrk,
                c[97].MsPositionReTrk,
                c[100].MsPositionReTrk,
                c[104].MsPositionReTrk,
                c[107].MsPositionReTrk,
                c[111].MsPositionReTrk,
                c[115].MsPositionReTrk,
                #endregion
                #region interlude after verse 2
                w1[25].MsPositionReTrk,
                w1[26].MsPositionReTrk,
                w1[28].MsPositionReTrk,
                w1[30].MsPositionReTrk,
                #endregion
                #region verse 3
                c[117].MsPositionReTrk,
                c[119].MsPositionReTrk,
                c[124].MsPositionReTrk,
                c[126].MsPositionReTrk,
                c[128].MsPositionReTrk,
                c[131].MsPositionReTrk,
                c[135].MsPositionReTrk,
                c[139].MsPositionReTrk,
                c[141].MsPositionReTrk,
                c[146].MsPositionReTrk,
                c[148].MsPositionReTrk,
                c[152].MsPositionReTrk,
                c[159].MsPositionReTrk,
                c[164].MsPositionReTrk,
                c[168].MsPositionReTrk,
                c[172].MsPositionReTrk,
                #endregion
                #region interlude after verse 3
                w1[38].MsPositionReTrk,
                w3[40].MsPositionReTrk,
                w3[42].MsPositionReTrk,
                w3[44].MsPositionReTrk,
                w3[45].MsPositionReTrk,
                w3[47].MsPositionReTrk,
                #endregion
                #region verse 4, Oft have ye...
                c[174].MsPositionReTrk,
                c[177].MsPositionReTrk,
                c[183].MsPositionReTrk,
                c[185].MsPositionReTrk,
                c[192].MsPositionReTrk,
                c[196].MsPositionReTrk,
                c[204].MsPositionReTrk,
                c[206].MsPositionReTrk,
                c[214].MsPositionReTrk,
                c[219].MsPositionReTrk,
                c[221].MsPositionReTrk,
                c[225].MsPositionReTrk,
                c[227].MsPositionReTrk,
                c[229].MsPositionReTrk,
                c[233].MsPositionReTrk,
                c[236].MsPositionReTrk,
                c[242].MsPositionReTrk,
                c[252].MsPositionReTrk,
                c[257].MsPositionReTrk,
                c[259].MsPositionReTrk,
                c[263].MsPositionReTrk,
                c[267].MsPositionReTrk,
                c[268].MsPositionReTrk, // new bar 89
                #endregion
                #region interlude after verse 4
                w1[57].MsPositionReTrk,
                w3[59].MsPositionReTrk,
                f4[45].MsPositionReTrk, // was w3[61].MsPosition,
                w3[63].MsPositionReTrk,
                w2[65].MsPositionReTrk, // was w3[65].MsPosition,
                w1[66].MsPositionReTrk, // w3[67].MsPosition,
                w1[68].MsPositionReTrk,
                #endregion
                #region verse 5
                c[269].MsPositionReTrk,
                c[270].MsPositionReTrk,
                c[272].MsPositionReTrk,
                c[276].MsPositionReTrk,
                c[279].MsPositionReTrk,
                c[283].MsPositionReTrk,
                c[288].MsPositionReTrk,
                #endregion
                #region postlude
                c[289].MsPositionReTrk,
                f1[248].MsPositionReTrk,
                f1[280].MsPositionReTrk, // new bar 105
                #endregion
                // final barline
                w3.EndMsPosition
                #endregion
            };
            Debug.Assert(barlineMsPositions.Count == NumberOfBars + 1); // includes bar 1 (mPos=0) and the final barline.
            return barlineMsPositions;
        }

        private List<List<VoiceDef>> GetBars(List<VoiceDef> voiceDefs, List<int> barlineMsPositions)
        {
            // barlineMsPositions contains both msPos=0 and the position of the final barline
            List<List<VoiceDef>> bars = new List<List<VoiceDef>>();
            bars = GetBarsFromBarlineMsPositions(voiceDefs, barlineMsPositions);
            Debug.Assert(bars.Count == NumberOfBars);
            return bars;
        }
        /// <summary>
        /// Splits the voices (currently in a single bar) into bars
        /// barlineMsPositions contains both msPosition 0, and the position of the final barline.
        /// </summary>
        private List<List<VoiceDef>> GetBarsFromBarlineMsPositions(List<VoiceDef> voices, List<int> barLineMsPositions)
        {
            List<List<VoiceDef>> bars = new List<List<VoiceDef>>();
            List<List<VoiceDef>> twoBars = null;
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
