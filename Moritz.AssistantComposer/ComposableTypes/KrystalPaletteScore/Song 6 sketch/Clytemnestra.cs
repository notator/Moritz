using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System;

using Moritz.Score;
using Moritz.Score.Midi;

namespace Moritz.AssistantComposer
{
    /// <summary>
    /// Encapsulates the composition of Clytamnestra's Voice
    /// and the positions of barlines -- which are actually set in Song6SketchAlgorithm.DoAlgorithm().
    /// </summary>
    public class Clytemnestra
    {
        public Clytemnestra(List<PaletteDef> paletteDefs, List<int> blockMsDurations)
        {
            Debug.Assert(paletteDefs != null);

            SetMomentDefsListPerVerse(paletteDefs);
            SetBlockMsDurations(blockMsDurations);
            SetBarLineMsPositionsPerBlock();
        }

        /// <summary>
        /// Sets _momentDefsListPerVerse to contain a list of MomentDefs for each verse.
        /// Each MomentDef is positioned with respect to the beginning of its verse, and contains
        /// a single LocalMidiChordDef in its MidiChordDefs list.
        /// Each LocalMidiChordDef contains a clone of a PaletteMidiChordDef, whose duration has
        /// been customised according to MidiChordDefMsDurationsPerVerse.
        /// The PaletteMidiChordDef is found using VowelPaletteIndicesPerVerse
        /// </summary>
        private void SetMomentDefsListPerVerse(List<PaletteDef> paletteDefs)
        {
            _momentDefsListPerVerse = new List<List<MomentDef>>();

            List<List<int>> vowelsPerVerse = VowelPaletteIndicesPerVerse;
            List<List<int>> mdMsPosPerVerse = MomentDefMsPositionsPerVerse;
            List<List<int>> mcMsDurPerVerse = MidiChordDefMsDurationsPerVerse;
            List<List<string>> lyricsPerVerse = LyricsPerVerse;

            MomentDef finalMomentDef = null;

            for(int verseIndex = 0; verseIndex < 5; ++verseIndex)
            {
                int paletteIndex = 0;

                List<int> vowels = vowelsPerVerse[verseIndex];
                List<int> mdMsPos = mdMsPosPerVerse[verseIndex];
                List<int> mcMsDur = mcMsDurPerVerse[verseIndex];
                List<string> lyrics = lyricsPerVerse[verseIndex];

                List<MomentDef> momentDefs = new List<MomentDef>();
                _momentDefsListPerVerse.Add(momentDefs);

                for(int syllableIndex = 0; syllableIndex < vowels.Count; ++syllableIndex)
                {                    
                    paletteIndex = vowels[syllableIndex];
                    PaletteMidiChordDef paletteMidiChordDef = paletteDefs[paletteIndex][0] as PaletteMidiChordDef;

                    Debug.Assert(paletteMidiChordDef != null); // a MidiRestDef!

                    MomentDef momentDef = new MomentDef(mdMsPos[syllableIndex]);
                    momentDefs.Add(momentDef);

                    LocalMidiChordDef localMidiChordDef = new LocalMidiChordDef(paletteMidiChordDef);

                    localMidiChordDef.MsDuration = mcMsDur[syllableIndex];
                    localMidiChordDef.Lyric = lyrics[syllableIndex];

                    // the following can determine what is seen
                    //localMidiChordDef.MidiHeadSymbols = new List<byte>(){50, 55};
                    //localMidiChordDef.MidiVelocitySymbol = 65;

                    //// the following can determine what is heard
                    //localMidiChordDef.BasicMidiChordDefs[0].Velocities[0] = 64;
                    //localMidiChordDef.BasicMidiChordDefs[0].Notes[0] = 50;

                    momentDef.MidiChordDefs.Add(localMidiChordDef);
                    momentDef.MsWidth = localMidiChordDef.MsDuration;

                    //if(finalMomentDef != null) // the previous moment
                    //{
                    //    finalMomentDef.MsWidth = momentDef.MsPosition - finalMomentDef.MsPosition;
                    //}

                    //finalMomentDef = momentDef;
                }
            }
        }

        /// <summary>
        /// Sets the msDurations of blocks 2,4,6,8,10 to the durations of the Verses.
        /// </summary>
        /// <param name="blockMsDurations"></param>
        private void SetBlockMsDurations(List<int> blockMsDurations)
        {
            Debug.Assert(blockMsDurations.Count == 11);
            int blockIndex = 0;
            for(int verseIndex = 0; verseIndex < 5; ++verseIndex)
            {
                blockIndex++;
                List<MomentDef> mDefs = _momentDefsListPerVerse[verseIndex];
                MomentDef lastMDef = mDefs[mDefs.Count-1];
                blockMsDurations[blockIndex] = lastMDef.MsPosition + lastMDef.MaximumMsDuration;

                blockIndex++; // do not change the blockMsDurations between the verses.
            }
        }

        /// <summary>
        /// Sets _barlineMsPositions containing the msPosition of each bar
        /// (not including 0 for bar 1, and not including the msPosition of the final barline).
        /// Each rest before and after a verse is set to one Bar, and the barlines in the verses are set
        /// to the same MomentDef positions as in the original sketch.
        /// </summary>
        private void SetBarLineMsPositionsPerBlock()
        {
            List<List<int>> barlineIndicesPerVerse = BarlineIndicesPerVerse;
            _barlineMsPositionsPerBlock = new List<List<int>>();
            int blockIndex = 0;
            _barlineMsPositionsPerBlock.Add(new List<int>()); // no barlines for block 1

            for(int vIndex = 0; vIndex < _momentDefsListPerVerse.Count; ++vIndex)
            {
                blockIndex++;
                List<int> blockBarlinePoss = new List<int>();
                
                List<MomentDef> momentDefs = _momentDefsListPerVerse[vIndex];
                List<int> indices = barlineIndicesPerVerse[vIndex];
                foreach(int index in indices)
                {
                    blockBarlinePoss.Add(momentDefs[index].MsPosition);
                }

                _barlineMsPositionsPerBlock.Add(blockBarlinePoss);

                blockIndex++;
                _barlineMsPositionsPerBlock.Add(new List<int>()); // no barlines for odd numbered blocks
            }
        }

        /// <summary>
        /// The indices of the momentDefs in Clytamnestra's  _momentDefsListPerVerse
        /// that are at the start of bars. These are the positions in the original sketch.
        /// </summary>
        private List<List<int>> BarlineIndicesPerVerse
        {
            get
            {
                List<int> v1BarlineIndices = new List<int>() { 0, 1, 5, 9, 11, 13, 17, 21, 27, 31, 33, 39, 40, 41, 45, 49 };
                List<int> v2BarlineIndices = new List<int>() { 0, 1, 5, 9, 11, 15, 19, 23, 25, 29, 30, 33, 35, 39, 41, 45, 49 };
                List<int> v3BarlineIndices = new List<int>() { 0, 1, 5, 7, 9, 11, 15, 19, 20, 25, 27, 30, 37, 41, 45, 49 };
                List<int> v4BarlineIndices = new List<int>() { 0, 3, 9, 10, 17, 21, 29, 31, 39, 43, 45, 49, 51, 53, 57, 59, 65, 75, 79, 81, 85, 89 };
                List<int> v5BarlineIndices = new List<int>() { 0, 1, 3, 7, 9, 13, 17 };
                List<List<int>> returnList = new List<List<int>>();
                returnList.Add(v1BarlineIndices);
                returnList.Add(v2BarlineIndices);
                returnList.Add(v3BarlineIndices);
                returnList.Add(v4BarlineIndices);
                returnList.Add(v5BarlineIndices);
                return returnList;
            }
        }

        /// <summary>
        /// the palette _index_ for the vowel of each syllable in the song (per Verse).
        /// </summary>
        private List<List<int>> VowelPaletteIndicesPerVerse
        {
            get
            {
                // Vowels have the following palette indices:
                // had:0, hard:1, hayed:2, head:3, heed:4, herd:5, hid:6 hod:7, hood:8, who'd:9
                List<int> v1PaletteIndices = new List<int>()
                {7,1,5,4,6,6,1,1,9,4,
                 1,1,6,7,5,1,5,2,4,5,
                 1,4,6,4,6,7,6,1,5,2,
                 1,9,0,9,9,7,5,6,6,2,
                 1,4,5,1,5,3,6,3,7,3};

                List<int> v2PaletteIndices = new List<int>()
                {7,1,5,4,1,1,9,6,5,1,
                 0,1,5,0,5,0,7,5,5,4,
                 7,1,1,2,9,2,4,9,1,2,
                 7,1,5,2,6,3,2,1,5,7,
                 5,6,6,7,5,0,6,1,5,1};

                List<int> v3PaletteIndices = new List<int>()
                {7,1,5,4,4,5,4,4,6,9,
                 0,2,6,7,7,2,5,7,1,3,
                 7,3,7,5,1,7,1,5,6,2,
                 4,7,6,5,3,6,5,1,6,1,
                 0,0,4,7,6,5,1,1,8,2};

                List<int> v4PaletteIndices = new List<int>()
                {7,0,4,3,3,1,7,2,5,2,
                 1,2,1,2,5,0,5,9,6,6,
                 1,1,4,7,7,0,7,1,7,1,
                 0,1,1,4,7,5,5,9,5,7,
                 1,7,4,6,1,0,5,1,5,4,
                 0,4,5,5,5,5,0,7,2,3,
                 0,3,5,7,4,1,4,7,5,2,
                 7,4,1,6,8,1,5,0,4,7,
                 5,1,4,7,4,2,7,6,6,2};

                List<int> v5PaletteIndices = new List<int>()
                {6,4,4,7,2,2,7,2,
                 5,4,0,9,9,1,2,2,1,7 };

                List<List<int>> vowelPaletteIndices = new List<List<int>>();
                vowelPaletteIndices.Add(v1PaletteIndices);
                vowelPaletteIndices.Add(v2PaletteIndices);
                vowelPaletteIndices.Add(v3PaletteIndices);
                vowelPaletteIndices.Add(v4PaletteIndices);
                vowelPaletteIndices.Add(v5PaletteIndices);
                return vowelPaletteIndices;
            }
        }
        /// <summary>
        /// the msPosition (with respect to the beginning of its verse) of each syllable's MomentDef.
        /// </summary>
        private List<List<int>> MomentDefMsPositionsPerVerse
        {
            get
            {
                List<int> verse1 = new List<int>()
                {0,1230,1774,2079,3142,3540,3838,4154,4799,5152,
                 7271,7725,8712,9080,9393,9832,10313,10649,11826,12215,
                 13409,13859,14218,14535,15176,15534,15928,16208,16544,16901,
                 17619,18104,18858,19189,19953,20314,20605,20937,21337,21735,
                 22680,23675,24037,24328,24592,24888,25282,25662,26235,26619};

                List<int> verse2 = new List<int>()
                {0,1200,1947,2302,3892,4326,5039,5301,5618,5967,
                 6431,6816,7066,7355,7611,7944,8293,8756,9121,9502,
                 10506,11319,11568,11806,12466,12793,13167,13579,13898,14231,
                 15712,16511,16808,17110,18315,18657,19025,19364,19736,20191,
                 21475,21916,22492,23187,23554,23938,24546,24862,25225,25692};

                List<int> verse3 = new List<int>()
                {0,1678,2610,3435,5261,5712,6661,7443,7951,8630,
                 9886,10580,11239,12011,12630,13095,13738,14451,14927,15514,
                 18144,19142,19472,19800,20359,20703,21915,22288,22753,23266,
                 25223,25831,26411,26781,27572,28383,28803,29303,30169,30735,
                 32698,33151,33603,33985,34723,35121,35596,36231,36964,37446};

                List<int> verse4 = new List<int>()
                {0,931,1262,1570,1935,2480,2776,3096,3527,4035,
                 5511,6018,6569,6865,7228,8134,8421,8702,9006,9402,
                 10118,10506,10810,11140,11424,11942,12209,12511,13093,13523,
                 14417,14848,15500,15828,16117,16378,16643,17227,17857,18217,
                 20356,21410,21783,22155,22900,23256,23623,23884,24163,24477,
                 25901,26330,27170,27487,27752,28021,28514,28851,29847,30202,
                 30871,31217,31528,31890,32589,33419,33776,34052,34319,34628,
                 34898,35177,35478,35899,36367,36824,37559,38875,39190,39551,
                 40438,40771,41062,41371,42030,42400,43001,43363,43636,43987};

                List<int> verse5 = new List<int>()
                {0,478,1697,2091,2414,2786,3160,3676,
                 5922,6415,7412,7844,8311,8759,9434,10184,11024,12595};

                List<List<int>> returnList = new List<List<int>>();
                returnList.Add(verse1);
                returnList.Add(verse2);
                returnList.Add(verse3);
                returnList.Add(verse4);
                returnList.Add(verse5);

                return returnList;
            }
        }
        /// <summary>
        /// returns the msDuration of each syllable's MidiChordDef.
        /// </summary>
        private List<List<int>> MidiChordDefMsDurationsPerVerse
        {
            get
            {
                List<int> verse1 = new List<int>()
                {866,544,305,839,398,298,316,645,353,1172,
                 454,329,368,313,439,481,336,392,389,1194,
                 450,359,317,641,358,394,280,336,357,718,
                 485,661,331,764,361,291,332,400,398,945,
                 995,362,291,264,296,394,380,573,384,1271};

                List<int> verse2 = new List<int>()
                {781,747,355,852,434,597,262,317,349,464,
                 385,250,289,256,333,349,463,365,381,789,
                 813,249,238,660,327,374,412,319,333,911,
                 799,297,302,915,342,368,339,372,455,1009,
                 441,576,695,367,384,608,316,363,467,1323};

                List<int> verse3 = new List<int>()
                {1177,932,825,1299,451,949,782,508,679,1256,
                 694,659,772,619,465,643,713,476,587,1526,
                 998,330,328,559,344,1212,373,465,513,957,
                 608,580,370,791,811,420,500,866,566,1262,
                 453,452,382,738,398,475,635,733,482,1437};

                List<int> verse4 = new List<int>()
                {931,331,308,365,545,296,320,431,508,1003,
                 507,551,296,363,906,287,281,304,396,716,
                 388,304,330,284,518,267,302,582,430,894,
                 431,652,328,289,261,265,584,630,360,1129,
                 1054,373,372,745,356,367,261,279,314,474,
                 429,840,317,265,269,493,337,856,355,669,
                 346,311,362,699,830,357,276,267,309,270,
                 279,301,421,468,457,735,880,315,361,887,
                 333,291,309,659,370,601,362,273,351,1638};

                List<int> verse5 = new List<int>()
                {478,1219,394,323,372,374,516,1322,
                 493,997,432,467,448,675,750,840,816,3183};

                List<List<int>> returnList = new List<List<int>>();
                returnList.Add(verse1);
                returnList.Add(verse2);
                returnList.Add(verse3);
                returnList.Add(verse4);
                returnList.Add(verse5);

                return returnList;
            }
        }

        private List<List<string>> LyricsPerVerse
        {
            get
            {
                List<string> verse1 = new List<string>()
                {
                    "What!", "Slum-", "ber", "ye?", "Is", "this", "a", "time", "to", "sleep?",
                    "While", "I,", "dis-", "hon-", "oured", "by", "the", "dead,", "re-", "proached",
                    "Un-", "ceas-","ing-", "ly,", "be-", "cause", "in", "right-", "eous", "rage",
                    "I", "slew,", "am", "doomed", "to", "wan-", "der", "in", "dis-", "grace",
                    "Shunned", "e-", "ven", "by", "the", "de-", "ni-", "zens", "of", "Hell."
                };

                List<string> verse2 = new List<string>()
                {
                    "What!", "Slum-", "ber", "ye?", "While", "I,", "who", "things", "so", "dire",
                    "Have", "suf-", "fered", "at", "the", "hands", "of", "those", "most", "dear",
                    "Stalk", "un-", "a-", "venged", "through", "Ha-", "des'", "gloo-", "my", "shades",
                    "Scorned", "by", "the", "dead,", "ne-", "gle-", "cted", "by", "the", "Gods,",
                    "The", "vic-", "tim", "of", "a", "ma-", "tri-", "ci-", "dal", "son."
                };

                List<string> verse3 = new List<string>()
                {
                    "What!", "Slum-", "ber", "ye?", "Be-", "hold", "these", "ree-", "king", "wounds",
                    "That,", "ga-", "ping,", "call", "for", "ven-", "geance", "from", "my", "breast.",
                    "Oft", "when", "on", "earth", "I", "walked,", "I", "heard", "it", "said,",
                    "\"Keen", "of", "dis-", "cerne-", "ment", "is", "the", "slumb-", "'ring", "mind\"",
                    "And", "can", "ye", "not", "dis-", "cern", "my", "dire-", "ful", "fate?"
                };

                List<string> verse4 = new List<string>()
                {
                    "Oft", "have", "ye", "ta-", "sted", "my", "ob-", "la-", "tions", "rare,",
                    "Wine-", "less", "li-", "ba-", "tions,", "and", "the", "soo-", "thing", "gifts",
                    "I", "night-", "ly", "of-", "fered", "at", "the", "hearth", "of", "fire",
                    "At", "hours", "un-", "sea-", "son-", "a-", "ble", "to", "the", "Gods,",
                    "But", "all", "these", "gifts", "are", "tram-", "pled", "un-", "der", "heel",
                    "And", "he", "the", "mur-", "der-", "er", "has", "gone,", "es-", "caped",
                    "As", "when", "the", "fawn", "leaps", "light-", "ly", "o'er", "the", "net",
                    "Of", "the", "un-", "skill-", "ful", "hun-", "ter,", "and", "he", "mocks",
                    "The", "clum-", "sy", "toils", "ye", "spread", "for", "him", "in", "vain."
                };

                List<string> verse5 = new List<string>()
                {
                    "Give", "ear", "ye", "God-", "dess-", "es", "of", "Hell!",
                    "A", "dream", "that", "once", "was", "Cly-", "tem-", "nes-", "tra", "calls!"
                };

                List<List<string>> lyrics = new List<List<string>>();
                lyrics.Add(verse1);
                lyrics.Add(verse2);
                lyrics.Add(verse3);
                lyrics.Add(verse4);
                lyrics.Add(verse5);
                return lyrics;
            }
        }   
        
        private List<List<MomentDef>> _momentDefsListPerVerse;
        public List<List<MomentDef>> MomentDefsListPerVerse { get { return _momentDefsListPerVerse; } }

        private List<List<int>> _barlineMsPositionsPerBlock;
        public List<List<int>> BarlineMsPositionsPerBlock { get { return _barlineMsPositionsPerBlock; } }

    }
}
