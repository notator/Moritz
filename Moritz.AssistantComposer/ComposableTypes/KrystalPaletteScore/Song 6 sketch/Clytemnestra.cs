using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Moritz.Score;
using Moritz.Score.Midi;

namespace Moritz.AssistantComposer
{
    /// <summary>
    /// Encapsulates the composition of Clytamnestra's Bars.
    /// </summary>
    public class Clytemnestra
    {
        public Clytemnestra(string midiFolder, List<List<int>> interludeBars)
        {
            List<List<MomentDef>> momentDefsListPerVerse = GetMomentDefsListPerVerse(midiFolder);
            momentDefsListPerVerse = AdjustPhrases(momentDefsListPerVerse);
            momentDefsListPerVerse = AdjustControls(momentDefsListPerVerse);
            _bars = GetBars(momentDefsListPerVerse, interludeBars);
        }
        private List<List<MomentDef>> GetMomentDefsListPerVerse(string midiFolder)
        {
            Multimedia.Midi.Sequence sequence;
            List<List<MomentDef>> momentDefsListPerVerse = new List<List<MomentDef>>();
            MidiUtilities mu = new MidiUtilities();

            for(int verse = 1; verse < 6; ++verse)
            {
                List<int> midiChannels = null;
                string fileName = @"What Slumber Ye (verse" + verse.ToString() + ").mid";
                string path = midiFolder + @"\" + fileName;
                if(File.Exists(path))
                {
                    sequence = new Multimedia.Midi.Sequence(path); // Multimedia.Midi type
                    List<MomentDef> momentDefs = mu.GetMomentDefs(sequence, out midiChannels);
                    Debug.Assert(midiChannels[0] == 0);
                    Debug.Assert(momentDefs != null && momentDefs.Count > 0);
                    momentDefsListPerVerse.Add(momentDefs);
                }
                else
                    Debug.Assert(false, "MidiScore: cannot find the file\n\n" + path);
            }

            return momentDefsListPerVerse;
        }
        /// <summary>
        /// Adjusts the durations of notes in the singer's part.
        /// These values were decided by inspection of the text.
        /// They have been written into the score Song 6.capx.
        /// A phrase is a sequence of MidiChords ending with a rest.
        /// </summary>
        private List<List<MomentDef>> AdjustPhrases(List<List<MomentDef>> momentDefsListPerVerse)
        {
            List<List<int>> midiChordsPerPhrasePerVerse = MidiChordsPerPhrasePerVerse();

            for(int verseIndex = 0; verseIndex < 5; ++verseIndex)
            {
                List<MomentDef> momentDefs = momentDefsListPerVerse[verseIndex];
                List<int> midiChordsPerPhrase = midiChordsPerPhrasePerVerse[verseIndex];
                int phraseIndex = 0;
                int remainingChordsInPhrase = midiChordsPerPhrase[phraseIndex];
                foreach(MomentDef momentDef in momentDefs)
                {
                    if(remainingChordsInPhrase > 1)
                    {
                        momentDef.MidiChordDefs[0].MsDuration = momentDef.MsWidth;
                        momentDef.MidiChordDefs[0].BasicMidiChordDefs[0] = new BasicMidiChordDef(momentDef.MidiChordDefs[0].BasicMidiChordDefs[0], momentDef.MsWidth); 
                        --remainingChordsInPhrase;
                    }
                    else
                    {
                        if(momentDef.MidiChordDefs[0].MsDuration != momentDef.MsWidth
                            && momentDef.MidiChordDefs[0].MsDuration < 20)
                        {
                            // this happens once at the end of the penultimate phrase in verse 1
                            momentDef.MidiChordDefs[0].MsDuration = momentDef.MsWidth / 3;
                            momentDef.MidiChordDefs[0].BasicMidiChordDefs[0] = new BasicMidiChordDef(momentDef.MidiChordDefs[0].BasicMidiChordDefs[0], momentDef.MsWidth / 3); 
                        }

                        ++phraseIndex;
                        if(phraseIndex < midiChordsPerPhrase.Count)
                            remainingChordsInPhrase = midiChordsPerPhrase[phraseIndex];
                    }
                }
            }
            return momentDefsListPerVerse;
        }
        /// <summary>
        /// Removes all controls except patch (which is set to 71 (MIDI "clarinet"))
        /// </summary>
        private List<List<MomentDef>> AdjustControls(List<List<MomentDef>> momentDefsListPerVerse)
        {
            for(int verseIndex = 0; verseIndex < 5; ++verseIndex)
            {
                List<MomentDef> momentDefs = momentDefsListPerVerse[verseIndex];
                foreach(MomentDef momentDef in momentDefs)
                {
                    MidiChordDef mcd = momentDef.MidiChordDefs[0]; // there is only one

                    MidiChordDef midiChordDef = new ComposableMidiChordDef(mcd.MidiHeadSymbols, new List<byte>(){64},
                        mcd.MsDuration, true, new List<MidiControl>());

                    midiChordDef.BasicMidiChordDefs[0].PatchIndex = 71; // Midi "clarinet"

                    momentDef.MidiChordDefs.Clear();
                    momentDef.MidiChordDefs.Add(midiChordDef);
                }
            }
            return momentDefsListPerVerse;
        }
        /// <summary>
        /// Gets bar sizes. In other words, sets the positions of barlines.
        /// </summary>
        /// <param name="momentDefsListPerVerse"></param>
        private List<Voice> GetBars(List<List<MomentDef>> momentDefsListPerVerse, List<List<int>> interludeBars)
        {
            List<List<int>> midiChordsPerBarPerVerse = MidiChordsPerBarPerVerse();
            // Each inner list in bars contains the voices in a particular bar, from top to bottom
            int msPosition = 0;
            List<Voice> bars = new List<Voice>();
            AppendSilentBars(bars, ref msPosition, interludeBars[0]);
            for(int verseIndex = 0; verseIndex < 5; ++verseIndex)
            {
                List<MomentDef> momentDefs = momentDefsListPerVerse[verseIndex];
                int momentDefIndex = 0;

                List<int> midiChordDefsPerBar = midiChordsPerBarPerVerse[verseIndex];
                for(int barIndex = 0; barIndex < midiChordDefsPerBar.Count; ++barIndex)
                {
                    Voice bar = new Voice(null, 0);
                    int nMidiChordDefsPerBar = midiChordDefsPerBar[barIndex];
                    for(int chordIndex = 0; chordIndex < nMidiChordDefsPerBar; ++chordIndex)
                    {
                        MomentDef momentDef = momentDefs[momentDefIndex++];
                        int restWidth = momentDef.MsWidth - momentDef.MaximumMsDuration;

                        MidiChordDef mcd = momentDef.MidiChordDefs[0];
                        LocalizedMidiDurationDef lmdd = new LocalizedMidiDurationDef(mcd);
                        lmdd.MsPosition = msPosition;
                        msPosition += lmdd.MsDuration;
                        bar.LocalizedMidiDurationDefs.Add(lmdd);
                        if(restWidth > 0)
                        {
                            LocalizedMidiDurationDef rmdd = new LocalizedMidiDurationDef(restWidth);
                            rmdd.MsPosition = msPosition;
                            msPosition += rmdd.MsDuration;
                            bar.LocalizedMidiDurationDefs.Add(rmdd);
                        }
                    }
                    bars.Add(bar);
                }
                AppendSilentBars(bars, ref msPosition, interludeBars[verseIndex + 1]);
            }
            return bars;
        }

        private void AppendSilentBars(List<Voice> bars, ref int msPosition, List<int> msDurations)
        {
            foreach(int msDuration in msDurations)
            {
                Voice restBar = new Voice(null, 0);
                LocalizedMidiDurationDef rest = new LocalizedMidiDurationDef(msDuration);
                rest.MsPosition = msPosition;
                msPosition += rest.MsDuration;
                restBar.LocalizedMidiDurationDefs.Add(rest);
                bars.Add(restBar);
            }
        }

        /// <summary>
        /// These values were decided by inspection of the text.
        /// They have been written into the score Song 6.capx.
        /// A phrase is a sequence of MidiChords ending with a rest.
        /// </summary>
        /// <returns></returns>
        private List<List<int>> MidiChordsPerPhrasePerVerse()
        {
            List<int> verse1MidiChordsPerPhrase = new List<int>() { 1, 3, 6, 2, 6, 14, 8, 10 };
            List<int> verse2MidiChordsPerPhrase = new List<int>() { 1, 3, 2, 14, 10, 4, 6, 10 };
            List<int> verse3MidiChordsPerPhrase = new List<int>() { 1, 3, 16, 10, 10, 10 };
            List<int> verse4MidiChordsPerPhrase = new List<int>() { 10, 30, 10, 8, 2, 17, 13 };
            List<int> verse5MidiChordsPerPhrase = new List<int>() { 8, 9, 1 };

            List<List<int>> midiChordsPerPhrasePerVerse = new List<List<int>>();
            midiChordsPerPhrasePerVerse.Add(verse1MidiChordsPerPhrase);
            midiChordsPerPhrasePerVerse.Add(verse2MidiChordsPerPhrase);
            midiChordsPerPhrasePerVerse.Add(verse3MidiChordsPerPhrase);
            midiChordsPerPhrasePerVerse.Add(verse4MidiChordsPerPhrase);
            midiChordsPerPhrasePerVerse.Add(verse5MidiChordsPerPhrase);

            return midiChordsPerPhrasePerVerse;

        }
        /// <summary>
        /// These values were decided by inspection of the text.
        /// They have been written into the score WhatSlumberYe(bars and rests).capx.
        /// </summary>
        /// <returns></returns>
        private List<List<int>> MidiChordsPerBarPerVerse()
        {
            List<int> verse1MidiChordsPerBar = new List<int>() { 1, 4, 4, 2, 2, 4, 4, 6, 4, 2, 6, 1, 1, 4, 4, 1 };
            List<int> verse2MidiChordsPerBar = new List<int>() { 1, 4, 4, 2, 4, 4, 4, 2, 4, 1, 3, 2, 4, 2, 4, 4, 1 };
            List<int> verse3MidiChordsPerBar = new List<int>() { 1, 4, 2, 2, 2, 4, 4, 1, 5, 2, 3, 7, 4, 4, 4, 1 };
            List<int> verse4MidiChordsPerBar = new List<int>() { 3, 6, 1, 7, 4, 8, 2, 8, 4, 2, 4, 2, 2, 4, 2, 6, 10, 4, 2, 4, 4, 1 };
            List<int> verse5MidiChordsPerBar = new List<int>() { 1, 2, 4, 2, 4, 4, 1 };

            List<List<int>> midiChordsPerBarPerVerse = new List<List<int>>();
            midiChordsPerBarPerVerse.Add(verse1MidiChordsPerBar);
            midiChordsPerBarPerVerse.Add(verse2MidiChordsPerBar);
            midiChordsPerBarPerVerse.Add(verse3MidiChordsPerBar);
            midiChordsPerBarPerVerse.Add(verse4MidiChordsPerBar);
            midiChordsPerBarPerVerse.Add(verse5MidiChordsPerBar);

            return midiChordsPerBarPerVerse;

        }
        /// <summary>
        /// Adds a single syllable to each chord that has a lyric.
        /// Lyrics are pure annotations. They mean nothing to the AssistantPerformer.
        /// </summary>
        /// <param name="systems"></param>
        public void AddLyrics(List<SvgSystem> systems)
        {
            List<List<string>> lyrics = Lyrics;
            int verseIndex = 0;
            int syllableIndex = 0;
            foreach(SvgSystem system in systems)
            {
                foreach(ChordSymbol chordSymbol in system.Staves[0].Voices[0].ChordSymbols)
                {
                    TextInfo textInfo = new TextInfo(lyrics[verseIndex][syllableIndex++], "Arial",
                        (float)(chordSymbol.FontHeight / 2.2F), TextHorizAlign.center);
                    Lyric lyric = new Lyric(this, textInfo);
                    chordSymbol.DrawObjects.Add(lyric);

                    if(syllableIndex == lyrics[verseIndex].Count)
                    {
                        syllableIndex = 0;
                        ++verseIndex;
                    }
                }
            }
            Debug.Assert(syllableIndex == 0 && verseIndex == 5);
        }

        private List<List<string>> Lyrics
        {
            get
            {
                List<List<string>> lyrics = new List<List<string>>();
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
                lyrics.Add(verse1);
                lyrics.Add(verse2);
                lyrics.Add(verse3);
                lyrics.Add(verse4);
                lyrics.Add(verse5);
                return lyrics;
            }
        }

        public List<Voice> Bars { get { return _bars; } }
        private List<Voice> _bars;
    }
}
