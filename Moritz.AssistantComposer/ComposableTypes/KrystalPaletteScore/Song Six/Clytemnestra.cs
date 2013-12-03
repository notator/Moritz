using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System;

using Moritz.Score;
using Moritz.Score.Midi;

namespace Moritz.AssistantComposer
{
    /// <summary>
    /// Encapsulates the composition of Clytemnestra's Voice
    /// </summary>
    public class Clytemnestra : VoiceDef
    {
        public Clytemnestra(VoiceDef wind3) 
            : base(new List<LocalMidiDurationDef>())
        {
            SetMomentDefsListPerVerse();
            SetLocalMidiDurationDefs(wind3);
            SetPitchWheelDeviation(0, _localMidiDurationDefs.Count - 1, 1);
        }

        /// <summary>
        /// Sets _momentDefsListPerVerse to contain a list of MomentDefs for each verse.
        /// Each MomentDef is positioned with respect to the beginning of its verse, and contains
        /// a single UniqueMidiChordDef in its MidiChordDefs list.
        /// </summary>
        private void SetMomentDefsListPerVerse()
        {
            _momentDefsListPerVerse = new List<List<MomentDef>>();

            List<List<int>> momentDefMsWidthPerVerse = MomentDefMsWidthPerVerse;
            List<List<int>> midiChordDefMsDurPerVerse = MidiChordDefMsDurationsPerVerse;
            List<List<string>> lyricsPerVerse = LyricsPerVerse;
            List<byte> verseVelocities = new List<byte>() { (byte)64, (byte)75, (byte)90, (byte)105, (byte)120 };
            List<byte> verseVolumes = new List<byte>() { (byte)120, (byte)100, (byte)100, (byte)100, (byte)100 };

            for(int verseIndex = 0; verseIndex < 5; ++verseIndex)
            {
                int momentMsPos = 0;

                List<int> momentMsWidth = momentDefMsWidthPerVerse[verseIndex];
                List<int> midiChordMsDur = midiChordDefMsDurPerVerse[verseIndex];
                List<string> lyrics = lyricsPerVerse[verseIndex];

                List<MomentDef> momentDefs = new List<MomentDef>();
                _momentDefsListPerVerse.Add(momentDefs);

                byte patch = (byte)(123 + verseIndex); // top 5 patches in bank 0
                List<byte> velocity = new List<byte>() { verseVelocities[verseIndex] };
                byte volume = verseVolumes[verseIndex];
 
                for(int syllableIndex = 0; syllableIndex < momentMsWidth.Count; ++syllableIndex)
                {
                    Debug.Assert(midiChordMsDur[syllableIndex] <= momentMsWidth[syllableIndex]);

                    MomentDef momentDef = new MomentDef(momentMsPos);
                    momentDef.MsWidth = momentMsWidth[syllableIndex];
                    momentDefs.Add(momentDef);

                    List<byte> pitch = new List<byte>() { (byte)syllableIndex }; // the syllables are organised like this in the soundfont.
                    int msDuration = midiChordMsDur[syllableIndex];

                    #region 
                    UniqueMidiChordDef lmcd = new UniqueMidiChordDef();
                    lmcd.MsDuration = msDuration;
                    //lmcd.Volume = (byte)100; // 100 is the default, and is not written into the score. Other values ARE added to each chord.
                    lmcd.Volume = volume;
                    lmcd.HasChordOff = true;
                    // Bank, Patch and Volume are added to *every* chord so that performances can start anywhere.
                    // If the interpreter is clever enough, repeated controls are not actually sent.
                    lmcd.Bank = (byte)(0);
                    lmcd.Patch = patch;
                    lmcd.Lyric = lyrics[syllableIndex];
                    // these two attributes determine the symbols in the score.
                    lmcd.MidiHeadSymbols = new List<byte>(){67}; // display middle G, even though "pitch" is different.
                    lmcd.MidiVelocity = velocity[0]; // determines the visible dynamic symbol
                    // the following determine what is actually heard 
                    List<byte> expressionMsbs = new List<byte>() { (byte)65 };
                    lmcd.MidiChordSliderDefs = new MidiChordSliderDefs(null, null, null, expressionMsbs);
                    lmcd.BasicMidiChordDefs.Add(new BasicMidiChordDef(msDuration, 0, patch, true, pitch, velocity));
                    #endregion                    

                    momentDef.MidiChordDefs.Add(lmcd);

                    momentMsPos += momentDef.MsWidth;
                }
            }
        }

        /// <summary>
        /// The msWidth of each momentDef (the msDuration between momentDefs).
        /// </summary>
        private List<List<int>> MomentDefMsWidthPerVerse
        {
            get
            {
                List<int> verse1 = new List<int>()
                {2962,1189,410,2371,584,366,363,890,380,3225,
                 836,1807,440,373,601,396,302,1451,410,1282,
                 431,413,655,1424,356,672,367,570,550,706,
                 329,1512,551,934,329,480,635,414,441,1857,
                 1288,585,574,481,335,329,180,937,614,1102};

                List<int> verse2 = new List<int>()
                {1739,1074,291,1986,634,847,313,530,693,693,
                 347,462,462,400,351,758,473,680,678,1607,
                 1209,333,277,836,453,544,589,588,266,2018,
                 1290,275,294,1274,400,668,439,347,274,1427,
                 326,679,693,365,269,532,292,578,284,1149};

                List<int> verse3 = new List<int>()
                {1914,1233,384,3223,500,846,650,652,484,1830,
                 875,484,757,493,321,757,835,503,477,3537,
                 1140,438,375,509,406,1280,384,529,247,2103,
                 777,449,462,546,978,520,122,742,518,2595,
                 573,438,371,590,326,623,424,417,436,762};

                List<int> verse4 = new List<int>()
                {699,346,395,521,382,263,175,295,806,1799,
                 616,434,416,213,1441,395,105,416,599,670,
                 380,501,318,218,490,259,287,501,280,1116,
                 446,921,318,401,355,156,354,216,321,4209,
                 572,599,670,829,354,659,460,360,259,820,
                 527,714,444,308,170,706,599,1100,580,723,
                 394,406,175,839,622,522,351,372,340,388,
                 170,363,213,625,501,365,1823,361,576,604,
                 259,478,401,1159,266,921,266,337,501,933};
                
                List<int> verse5 = new List<int>()
                {812,861,758,500,420,406,528,2396,
                 604,1630,621,851,681,1615,1595,1438,2374,6843};

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
        /// Returns the msDuration of each syllable's MidiChordDef.
        /// If this value is less than the corresponding moment.MsWidth,
        /// The chord, which is at the beginning of the moment, is followed by a rest.
        /// </summary>
        private List<List<int>> MidiChordDefMsDurationsPerVerse
        {
            get
            {
                List<int> verse1 = new List<int>()
                {1716,1189,410,856,584,366,363,890,380,1364,
                 836,1251,440,373,601,396,302,699,410,1282,
                 431,413,655,591,356,672,367,570,550,706,
                 329,897,551,934,329,480,635,414,441,876,
                 1288,585,574,481,335,329,180,937,614,1102};

                List<int> verse2 = new List<int>()
                {929,1074,291,1015,634,847,313,530,693,693,
                 347,462,462,400,351,758,473,680,678,693,
                 1209,333,277,836,453,544,589,588,266,1402,
                 1290,275,294,668,400,668,439,347,274,889,
                 326,679,693,365,269,532,292,578,284,1149};

                List<int> verse3 = new List<int>()
                {1342,1233,384,1140,500,846,650,652,484,1153,
                 875,484,757,493,321,757,835,503,477,987,
                 1140,438,375,509,406,1280,384,529,247,896,
                 777,449,462,546,978,520,122,742,518,993,
                 573,438,371,590,326,623,424,417,436,762};

                List<int> verse4 = new List<int>()
                {699,346,395,521,382,263,175,295,806,918,
                 616,434,416,213,1441,395,105,416,599,670,
                 380,501,318,218,490,259,287,501,280,1116,
                 446,921,318,401,355,156,354,216,321,939,
                 572,599,670,829,354,659,460,360,259,820,
                 527,714,444,308,170,706,599,591,580,723,
                 394,406,175,839,622,522,351,372,340,388,
                 170,363,213,625,501,365,410,361,576,604,
                 259,478,401,1159,266,921,266,337,501,933};

                List<int> verse5 = new List<int>()
                {812,861,758,500,420,406,528,1195,
                 604,1630,621,851,681,1615,1595,1438,1111,6843};

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
                    "Un-", "ceas-","ing-", "ly,", "be-", "cause", "in", "righ-", "teous", "rage",
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
                    "A", "dream", "that", "once", "was", "Cly-", "tam-", "nes-", "tra", "calls!"
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

        /// <summary>
        /// Sets Clytamnestra's _localMidiDurationDefs for the whole piece including rests.
        /// Barline positions are set later.
        /// </summary>
        private void SetLocalMidiDurationDefs(VoiceDef wind3)
        {
            Debug.Assert(_momentDefsListPerVerse.Count == 5);

            List<int> verseMsPositions = new List<int>();
            verseMsPositions.Add(wind3[8].MsPosition);
            verseMsPositions.Add(wind3[20].MsPosition);
            verseMsPositions.Add(wind3[33].MsPosition);
            verseMsPositions.Add(wind3[49].MsPosition);
            verseMsPositions.Add(wind3[70].MsPosition);

            int currentVerseMsPosition = 0;
            int currentEndMsPosition = 0;
            LocalMidiDurationDef intervalRestDef = null;

            for(int verseIndex = 0; verseIndex < 5; ++verseIndex)
            { 
                currentVerseMsPosition = verseMsPositions[verseIndex];

                intervalRestDef = new LocalMidiDurationDef(currentEndMsPosition, currentVerseMsPosition - currentEndMsPosition);

                _localMidiDurationDefs.Add(intervalRestDef);

                List<MomentDef> momentDefs = _momentDefsListPerVerse[verseIndex];

                for(int momentDefIndex = 0; momentDefIndex < momentDefs.Count; ++momentDefIndex)
                {
                    MomentDef momentDef = momentDefs[momentDefIndex];
                    momentDef.MsPosition += currentVerseMsPosition;

                    int restWidth = momentDef.MsWidth - momentDef.MaximumMsDuration;
                    LocalMidiDurationDef lmrd = null;
                    if(restWidth > 0)
                    {
                        momentDef.MsWidth -= restWidth;
                        lmrd = new LocalMidiDurationDef(momentDef.MsPosition + momentDef.MsWidth, restWidth);
                        Debug.Assert(lmrd.MsDuration > 0);
                    }

                    MidiChordDef mcd = momentDef.MidiChordDefs[0];
                    LocalMidiDurationDef lmcd = new LocalMidiDurationDef(mcd, momentDef.MsPosition, momentDef.MsWidth);
                    Debug.Assert(lmcd.MsDuration > 0);

                    _localMidiDurationDefs.Add(lmcd);

                    if(lmrd != null)
                    {
                        _localMidiDurationDefs.Add(lmrd);
                    }
                    currentEndMsPosition = _localMidiDurationDefs[_localMidiDurationDefs.Count - 1].MsPosition +
                        _localMidiDurationDefs[_localMidiDurationDefs.Count - 1].MsDuration;
                }
            }
            intervalRestDef = new LocalMidiDurationDef(currentEndMsPosition, wind3.EndMsPosition - currentEndMsPosition);
            _localMidiDurationDefs.Add(intervalRestDef);
        }

        private List<List<MomentDef>> _momentDefsListPerVerse;

        /// <summary>
        /// A temporary measure while composing
        /// </summary>
        public override void SetLyricsToIndex()
        {
            for(int index = 0; index < _localMidiDurationDefs.Count; ++index)
            {
                UniqueMidiChordDef lmcd = _localMidiDurationDefs[index].UniqueMidiDurationDef as UniqueMidiChordDef;
                if(lmcd != null)
                {
                    lmcd.Lyric = index.ToString() + lmcd.Lyric;
                }
            }
        }
    }
}
