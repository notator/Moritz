using System;
using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;

using Moritz.Globals;
using Moritz.Palettes;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
	public partial class Tombeau1Algorithm : CompositionAlgorithm
	{
        private static class Tombeau1Templates
        {
            #region envelopes
            private static List<List<byte>> Envelopes2 = new List<List<byte>>()
            {
                { new List<byte>() {64, 0} },
                { new List<byte>() {64, 18} },
                { new List<byte>() {64, 36} },
                { new List<byte>() {64, 54} },
                { new List<byte>() {64, 72} },
                { new List<byte>() {64, 91} },
                { new List<byte>() {64, 109} },
                { new List<byte>() {64, 127} }
            };
            private static List<List<byte>> Envelopes3 = new List<List<byte>>()
            {
                { new List<byte>() {64, 0, 64} },
                { new List<byte>() {64, 18, 64} },
                { new List<byte>() {64, 36, 64} },
                { new List<byte>() {64, 54, 64} },
                { new List<byte>() {64, 72, 64} },
                { new List<byte>() {64, 91, 64} },
                { new List<byte>() {64, 109, 64} },
                { new List<byte>() {64, 127, 64} }
            };
            private static List<List<byte>> Envelopes4 = new List<List<byte>>()
            {
                { new List<byte>() {64, 0, 64, 64} },
                { new List<byte>() {64, 22, 64, 64} },
                { new List<byte>() {64, 22, 96, 64} },
                { new List<byte>() {64, 64, 0, 64} },
                { new List<byte>() {64, 64, 22, 64} },
                { new List<byte>() {64, 64, 80, 64} },
                { new List<byte>() {64, 80, 64, 64} },
                { new List<byte>() {64, 96, 22, 64 } }
            };
            private static List<List<byte>> Envelopes5 = new List<List<byte>>()
            {
                { new List<byte>() {64, 50, 72, 50, 64} },
                { new List<byte>() {64, 64, 0, 64, 64} },
                { new List<byte>() {64, 64, 64, 80, 64} },
                { new List<byte>() {64, 64, 64, 106, 64} },
                { new List<byte>() {64, 64, 127, 64, 64} },
                { new List<byte>() {64, 70, 35, 105, 64} },
                { new List<byte>() {64, 72, 50, 70, 64} },
                { new List<byte>() {64, 80, 64, 64, 64} },
                { new List<byte>() {64, 105, 35, 70, 64} },
                { new List<byte>() {64, 106, 64, 64, 64} }
            };
            private static List<List<byte>> Envelopes6 = new List<List<byte>>()
            {
                { new List<byte>() {64, 22, 43, 64, 64, 64} },
                { new List<byte>() {64, 30, 78, 64, 40, 64} },
                { new List<byte>() {64, 40, 64, 78, 30, 64} },
                { new List<byte>() {64, 43, 106, 64, 64, 64} },
                { new List<byte>() {64, 64, 64, 43, 22, 64} },
                { new List<byte>() {64, 64, 64, 64, 106, 64} },
                { new List<byte>() {64, 64, 64, 64, 127, 64} },
                { new List<byte>() {64, 64, 64, 106, 43, 64} },
                { new List<byte>() {64, 106, 64, 64, 64, 64} },
                { new List<byte>() {64, 127, 127, 22, 64, 64} }
            };
            private static List<List<byte>> Envelopes7 = new List<List<byte>>()
            {
                { new List<byte>() {64, 0, 0, 106, 106, 64, 64} },
                { new List<byte>() {64, 28, 68, 48, 108, 88, 64} },
                { new List<byte>() {64, 40, 20, 80, 60, 100, 64} },
                { new List<byte>() {64, 55, 50, 75, 50, 64, 64} },
                { new List<byte>() {64, 64, 64, 64, 64, 32, 64} },
                { new List<byte>() {64, 64, 50, 75, 50, 55, 64} },
                { new List<byte>() {64, 73, 78, 53, 78, 64, 64} },
                { new List<byte>() {64, 85, 64, 106, 64, 127, 64} },
                { new List<byte>() {64, 88, 108, 48, 68, 28, 64} },
                { new List<byte>() {64, 100, 60, 80, 20, 40, 64} },
                { new List<byte>() {64, 127, 127, 64, 64, 64, 64} }
            };
            private static List<List<byte>> EnvelopesLong = new List<List<byte>>()
            {
                { new List<byte>() {64, 0, 64, 96, 127, 30, 0, 64} },
                { new List<byte>() {64, 64, 64, 127, 64, 106, 43, 64} },
                { new List<byte>() {64, 64, 43, 43, 64, 64, 85, 22, 64} },
                { new List<byte>() {64, 64, 64, 0, 64, 127, 0, 64, 64} },
                { new List<byte>() {64, 80, 64, 92, 64, 64, 64, 98, 64} },
                { new List<byte>() {64, 98, 64, 64, 64, 92, 64, 80, 64} },
                { new List<byte>() {64, 64, 64, 0, 64, 127, 0, 64, 127, 0, 64, 64} },
                { new List<byte>() {64, 64, 64, 64, 64, 64, 64, 64, 64, 100, 50, 100} },
                { new List<byte>() {64, 64, 64, 64, 64, 64, 64, 64, 64, 127, 43, 127, 64} },
                { new List<byte>() {64, 127, 43, 127, 64, 64, 64, 64, 64, 64, 64, 64, 64} },
                { new List<byte>() {64, 64, 64, 64, 64, 64, 64, 127, 43, 127, 64, 127, 43, 127, 64} },
                { new List<byte>() {64, 127, 43, 127, 43, 127, 64, 64, 64, 64, 64, 64, 64, 64, 64} },
                { new List<byte>() {64, 64, 64, 0, 64, 127, 0, 64, 127, 64, 0, 64, 127, 64, 0, 64} },
                { new List<byte>() {64, 127, 64, 64, 0, 64, 127, 0, 64, 127, 64, 0, 64, 127, 64, 0, 64} },
                { new List<byte>() {64, 127, 43, 127, 43, 127, 64, 127, 43, 127, 43, 127, 64, 64, 64, 64, 64, 64, 64, 64, 64} }
            };
            #endregion envelopes

            #region duration modi
            private static List<int> Durations1 = new List<int>()
            {   1000 };
            private static List<int> Durations2 = new List<int>()
            {   1000, 707 }; // 1 / ( 2^(1 / 2) )
            private static List<int> Durations3 = new List<int>()
            {   1000, 794, 630 }; // 1 / ( 2^(1 / 3) )
            private static List<int> Durations4 = new List<int>()
            {   1000, 841, 707, 595 }; // 1 / ( 2^(1 / 4) )
            private static List<int> Durations5 = new List<int>()
            {   1000, 871, 758, 660, 574 }; // 1 / ( 2^(1 / 5) )
            private static List<int> Durations6 = new List<int>()
            {   1000, 891, 794, 707, 630, 561 }; // 1 / ( 2^(1 / 6) )
            private static List<int> Durations7 = new List<int>()
            {   1000, 906, 820, 743, 673, 610, 552 }; // 1 / ( 2^(1 / 7) )
            private static List<int> Durations8 = new List<int>()
            {   1000, 917, 841, 771, 707, 648, 595, 545}; // 1 / ( 2^(1 / 8) )
            private static List<int> Durations9 = new List<int>()
            {   1000, 926, 857, 794, 735, 680, 630, 583, 540}; // 1 / ( 2^(1 / 9) )
            private static List<int> Durations10 = new List<int>()
            {   1000, 933, 871, 812, 758, 707, 660, 616, 574, 536}; // 1 / ( 2^(1 / 10) )
            private static List<int> Durations11 = new List<int>()
            {   1000, 939, 882, 828, 777, 730, 685, 643, 604, 567, 533 }; // 1 / ( 2^(1 / 11) )
            private static List<int> Durations12 = new List<int>()
            {   1000, 944, 891, 841, 794, 749, 707, 667, 630, 595, 561, 530 }; // 1 / ( 2^(1 / 12) )

            #endregion duration modi

            #region Init 
            /// <summary>
            /// Sets up the standard MidiChordDefs, Trks etc. that will be used in the composition.
            /// </summary>
            public static void Init(List<Palette> paletteList)
            {
                SetPaletteMidiChordDefs(paletteList);
                SetPitchWheelTestMidiChordDefs();
                SetOrnamentTestMidiChordDefs();

                //SetTemplateTrks();
            }

            //private static void SetTemplateTrks()
            //{

            //    List<MidiChordDef> lastMidiChordDefList = _ornamentTestMidiChordDefs[_ornamentTestMidiChordDefs.Count - 1];

            //    SetTrk1(_trks[0][0]);
            //}

            //private static void SetTrk1(Trk trk)
            //{
            //    int relativePitchHierarchyIndex = 0;
            //    int gamutRoot = 0;
            //    int nPitchesPerOctave = 8;

            //    List<int> absolutePitchHierarchy = M.GetAbsolutePitchHierarchy(relativePitchHierarchyIndex, gamutRoot);
            //     Gamut gamut = new Gamut(absolutePitchHierarchy, nPitchesPerOctave);

            //    int nMidiChordDefs = 4;
            //    List<int> chordDensities = new List<int>() { 4, 4, 5, 4 };
            //    for(int i = 0; i < nMidiChordDefs; ++i)
            //    {
            //        MidiChordDef mcd = null;
            //        int mcdRootPitch = gamut.List[i];

            //        if(i == 2)
            //        {
            //            //List<byte> basicMidiChordRootPitches = new List<byte>() { };
            //            //// create an ornament
            //            //mcd = new MidiChordDef(1000, basicMidiChordRootPitches);

            //        }
            //        else
            //        {
            //            mcd = new MidiChordDef(chordDensities[i], mcdRootPitch, absolutePitchHierarchy, 127, 1000, true);
            //        }

            //        List<int> velocityPerAbsolutePitch = M.GetVelocityPerAbsolutePitch(absolutePitchHierarchy, 0);
            //        mcd.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch);

            //        trk.Add(mcd);
            //    }
            //}

            private static void SetPaletteMidiChordDefs(List<Palette> paletteList)
            {
                List<List<MidiChordDef>> paletteMidiChordDefs = new List<List<MidiChordDef>>();
                foreach(Palette palette in paletteList)
                {
                    _paletteMidiChordDefs.Add(GetPaletteMidiChordDefs(palette));
                }
            }
            private static void SetPitchWheelTestMidiChordDefs()
            {
                foreach(List<List<byte>> envList in _envelopes)
                {
                    List<MidiChordDef> pwmcds = GetPitchWheelTestMidiChordDefs(envList);
                    _pitchWheelTestMidiChordDefs.Add(pwmcds);
                }
            }
            private static void SetOrnamentTestMidiChordDefs()
            {
                int relativePitchHierarchyIndex = 0;
                int absHierarchyRoot = 0;
                foreach(List<List<byte>> envList in _envelopes)
                {
                    List<List<byte>> constList = _envelopes[6];
                    List<MidiChordDef> omcds = GetOrnamentTestMidiChordDefs(constList, relativePitchHierarchyIndex++, absHierarchyRoot++);
                    _ornamentTestMidiChordDefs.Add(omcds);
                }
            }

            private static List<MidiChordDef> GetPitchWheelTestMidiChordDefs(List<List<byte>> envList)
            {
                List<MidiChordDef> rval = new List<MidiChordDef>();
                foreach(List<byte> envelope in envList)
                {
                    MidiChordDef mcd = new MidiChordDef(new List<byte>() { 60 }, new List<byte>() { 127 }, 1000, true);
                    mcd.SetPitchWheelEnvelope(new Envelope(envelope, 127, 127, envelope.Count));
                    rval.Add(mcd);
                }
                return rval;
            }
            private static List<MidiChordDef> GetOrnamentTestMidiChordDefs(List<List<byte>> envList, int relativePitchHierarchyIndex, int absHierarchyRoot)
            {
                List<MidiChordDef> rval = new List<MidiChordDef>();

                List<int> absolutePitchHierarchy = M.GetAbsolutePitchHierarchy(relativePitchHierarchyIndex, absHierarchyRoot);
                Gamut gamut = new Gamut(absolutePitchHierarchy, 8);

                foreach(List<byte> envelope in envList)
                {
                    Envelope ornamentEnvelope = new Envelope(envelope, 127, 8, 6);
                    Envelope timeWarpEnvelope = new Envelope(new List<byte>() { 127, 0, 127 }, 127, 127, 6);

                    int msDuration = 1000;
                    int firstPitch = 60;
                    while(!gamut.Contains(firstPitch))
                    {
                        firstPitch++;
                    }
                    MidiChordDef mcd = new MidiChordDef(msDuration, gamut, firstPitch, 1, ornamentEnvelope);

                    mcd.TimeWarp(timeWarpEnvelope, 16);

                    //mcd.Transpose(gamut, 3);

                    rval.Add(mcd);
                }
                return rval;
            }

            /// <summary>
            /// A list of the MidiChordDefs defined in the palette.
            /// </summary>
            private static List<MidiChordDef> GetPaletteMidiChordDefs(Palette palette)
            {
                List<MidiChordDef> midiChordDefs = new List<MidiChordDef>();
                for(int i = 0; i < palette.Count; ++i)
                {
                    IUniqueDef iud = palette.UniqueDurationDef(i);
                    Debug.Assert(iud is MidiChordDef);
                    midiChordDefs.Add(iud as MidiChordDef);
                }
                return midiChordDefs;
            }
            #endregion init

            #region _envelopes, _durationModi
            public static List<List<List<byte>>> Envelopes
            {
                get
                {
                    return DeepCloneOf(_envelopes);
                }
            }
            public static List<List<int>> DurationModi
            {
                get
                {
                    return DeepCloneOf(_durationModi);
                }
            }
            private static List<List<List<byte>>> _envelopes = new List<List<List<byte>>>()
            {
                Envelopes2, Envelopes3, Envelopes4, Envelopes5, Envelopes6, Envelopes7, EnvelopesLong
            };
            private static List<List<int>> _durationModi = new List<List<int>>()
            {
                Durations1, Durations2, Durations3, Durations4, Durations5, Durations6, Durations7, Durations8, Durations9, Durations10, Durations11, Durations12
            };
            #endregion _envelopes, _durationModi

            public static List<List<Trk>> Trks
            {
                get
                {
                    return DeepCloneOf(_trks);
                }
            }
            public static List<List<MidiChordDef>> PaletteMidiChordDefs
            {
                get
                {
                    return DeepCloneOf(_paletteMidiChordDefs);
                }
            }
            public static List<List<MidiChordDef>> PitchWheelTestMidiChordDefs
            {
                get
                {
                    return DeepCloneOf(_pitchWheelTestMidiChordDefs);
                }
            }
            public static List<List<MidiChordDef>> OrnamentTestMidiChordDefs
            {
                get
                {
                    return DeepCloneOf(_ornamentTestMidiChordDefs);
                }
            }
            private static List<List<T>> DeepCloneOf<T>(List<List<T>> original)
            {
                List<List<T>> tListList = new List<List<T>>();
                foreach(List<T> tList in original)
                {
                    List<T> newTList = new List<T>();
                    tListList.Add(newTList);
                    foreach(T t in tList)
                    {
                        ICloneable c = t as ICloneable;
                        if(c != null)
                        {
                            newTList.Add((T)c.Clone());
                        }
                        else
                        {
                            newTList.Add(t);
                        }
                    }
                }
                return tListList;
            }
            private static List<List<List<T>>> DeepCloneOf<T>(List<List<List<T>>> original)
            {
                List<List<List<T>>> tListListList = new List<List<List<T>>>();
                foreach(List<List<T>> tListList in original)
                {
                    tListListList.Add(DeepCloneOf(tListList));
                }
                return tListListList;
            }


            private static List<List<Trk>> _trks = new List<List<Trk>>();
            private static List<List<MidiChordDef>> _paletteMidiChordDefs = new List<List<MidiChordDef>>();

            private static List<List<MidiChordDef>> _pitchWheelTestMidiChordDefs = new List<List<MidiChordDef>>();
            private static List<List<MidiChordDef>> _ornamentTestMidiChordDefs = new List<List<MidiChordDef>>();

        }       
    }
}
