using Krystals5ObjectLibrary;

using Moritz.Palettes;
using Moritz.Spec;

using System.Collections.Generic;

namespace Moritz.Algorithm.Study3Sketch1
{
    /// <summary>
    /// Algorithm for testing Song 6's palettes.
    /// This may develope as composition progresses...
    /// </summary>
    public class Study3Sketch1Algorithm : CompositionAlgorithm
    {
        public Study3Sketch1Algorithm()
            : base()
        {
            CheckParameters();
        }

        public override int NumberOfMidiChannels { get { return 8; } }
        public override int NumberOfBars { get { return 5; } }

        /// <summary>
        /// See CompositionAlgorithm.DoAlgorithm()
        /// </summary>
        public override List<Bar> DoAlgorithm(List<Krystal> krystals, List<Palette> palettes)
        {
            _krystals = krystals;
            _palettes = palettes;

            var approximateBarlineMsPositions = new List<double>();

            Bar bar1 = CreateBar1();
            approximateBarlineMsPositions.Add(bar1.MsDuration);

            Bar bar2 = CreateBar2();
            approximateBarlineMsPositions.Add(bar1.MsDuration + bar2.MsDuration);

            Bar bars345 = bar2.Clone();
            approximateBarlineMsPositions.Add(bar1.MsDuration + bar2.MsDuration + 5950);
            approximateBarlineMsPositions.Add(bar1.MsDuration + bar2.MsDuration + 10500);
            approximateBarlineMsPositions.Add(bar1.MsDuration + bar2.MsDuration + bars345.MsDuration);

            Bar mainBar = bar1;
            mainBar.Concat(bar2);
            mainBar.Concat(bars345);

            List<int> barlineMsPositions = GetBarlinePositions(mainBar.Trks0, approximateBarlineMsPositions);

            List<List<SortedDictionary<int, string>>> clefChangesPerBar = GetClefChangesPerBar(barlineMsPositions.Count, mainBar.Trks.Count);

            List<List<SortedDictionary<int, string>>> lyricsPerBar = GetLyricsPerBar(barlineMsPositions.Count, mainBar.Trks.Count);

            List<Bar> bars = GetBars(mainBar, barlineMsPositions, clefChangesPerBar, lyricsPerBar);

            return bars;
        }

        /// <summary>
        /// See summary and example code on abstract definition in CompositionAlogorithm.cs
        /// </summary>
        protected override List<List<SortedDictionary<int, string>>> GetClefChangesPerBar(int nBars, int nVoicesPerBar)
        {
            return null;
        }

        /// <summary>
        /// Lyrics can be attached to MidiChordDefs or InputChordDefs earlier in the algorithm, but this function
        /// provides the possibility of adding them all in one place.
        /// This function returns null or a SortedDictionary per ChannelDef in each bar.
        /// The dictionary contains the index of the MidiChordDef or InputChordDef in the bar to which the associated
        /// lyric string will be attached. The index is of MidiChordDefs or InputChordDefs only, beginning with 0 for
        /// the first MidiChordDef or InptChordDef in the bar.
        /// Lyrics that are attached to top voices on a staff will, like dynamics, be automatically placed above the staff.
        /// </summary>
        protected override List<List<SortedDictionary<int, string>>> GetLyricsPerBar(int nBars, int nVoicesPerBar)
        {
            //var lyricsPerBar = GetEmptyStringExtrasPerBar(nBars, nVoicesPerBar);

            //SortedDictionary<int, string> bar2VoiceDef1 = lyricsPerBar[1][0]; // Bar 2 Voice 1.
            //bar2VoiceDef1.Add(9, "lyric9");
            //bar2VoiceDef1.Add(8, "lyric8");
            //bar2VoiceDef1.Add(6, "lyric6");
            //bar2VoiceDef1.Add(4, "lyric4");
            //bar2VoiceDef1.Add(2, "lyric2");

            //SortedDictionary<int, string> bar2VoiceDef2 = lyricsPerBar[1][1]; // Bar 2 Voice 2.
            //bar2VoiceDef2.Add(9, "lyric9a");
            //bar2VoiceDef2.Add(8, "lyric8a");
            //bar2VoiceDef2.Add(6, "lyric6a");
            //bar2VoiceDef2.Add(4, "lyric4a");
            //bar2VoiceDef2.Add(2, "lyric2a");

            //return lyricsPerBar;

            return null;
        }

        #region CreateBar1()
        private Bar CreateBar1()
        {
            List<ChannelDef> channelDefs = new List<ChannelDef>();

            byte channel = (byte)(_palettes.Count - 1);
            foreach(Palette palette in _palettes)
            {
                List<Trk> trks = new List<Trk>();
                Trk trk = new Trk(new List<IUniqueDef>());
                trks.Add(trk);
                ChannelDef channelDef = new ChannelDef(trks);
                WriteVoiceMidiDurationDefs1(trk, palette);
                channel--;
            }

            Bar seq = new Bar(0, channelDefs);

            return seq;
        }

        private void WriteVoiceMidiDurationDefs1(Trk trk, Palette palette)
        {
            int msPositionReFirstIUD = 0;
            int bar1ChordMsSeparation = 1500;
            for(int i = 0; i < palette.Count; ++i)
            {
                IUniqueDef durationDef = palette.GetIUniqueDef(i);
                durationDef.MsPositionReFirstUD = msPositionReFirstIUD;
                MidiRestDef restDef = new MidiRestDef(msPositionReFirstIUD + durationDef.MsDuration, bar1ChordMsSeparation - durationDef.MsDuration);
                msPositionReFirstIUD += bar1ChordMsSeparation;
                trk.UniqueDefs.Add(durationDef);
                trk.UniqueDefs.Add(restDef);
            }
        }
        #endregion CreateBar1()

        /// <summary>
        /// This function creates only one bar, using Trk objects. 
        /// </summary>
        private Bar CreateBar2()
        {
            List<ChannelDef> channelDefs = new List<ChannelDef>();

            byte paletteIndex = (byte)(_palettes.Count - 1);
            foreach(Palette palette in _palettes)
            {
                List<Trk> trks = new List<Trk>();
                ChannelDef channelDef = new ChannelDef(trks);
                Trk trk = palette.NewTrk(paletteIndex);
                trk.MsDuration = 6000; // stretches or compresses the trk duration to 6000ms
                trks.Add(trk);
                channelDefs.Add(channelDef);
                paletteIndex--;
            }

            List<Trk> trks0 = new List<Trk>();
            foreach(var channel in channelDefs)
            {
                trks0.Add(channel.Trks[0]);
            }

            int maxMsPosReBar = 0;
            // insert rests at the start of the Trks
            int restMsDuration = 0;
            foreach(Trk trk in trks0)
            {
                if(restMsDuration > 0)
                {
                    MidiRestDef restDef = new MidiRestDef(0, restMsDuration);
                    trk.Insert(0, restDef);
                }
                restMsDuration += 1500;
                maxMsPosReBar = trk.EndMsPositionReFirstIUD;
            }

            // add the final rest in the bar
            foreach(Trk trk in trks0)
            {
                int trkEndMsPosReBar = trk.EndMsPositionReFirstIUD;
                if(maxMsPosReBar > trkEndMsPosReBar)
                {
                    MidiRestDef restDef = new MidiRestDef(trkEndMsPosReBar, maxMsPosReBar - trkEndMsPosReBar);
                    trk.Add(restDef);
                }
            }

            Bar bar = new Bar(0, channelDefs);

            return bar;
        }
    }
}
