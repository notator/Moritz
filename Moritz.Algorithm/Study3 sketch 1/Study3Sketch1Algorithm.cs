using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Algorithm;
using Moritz.Palettes;
using Moritz.Spec;

namespace Moritz.Algorithm.Study3Sketch1
{
    /// <summary>
    /// Algorithm for testing Song 6's palettes.
    /// This may develope as composition progresses...
    /// </summary>
    public class Study3Sketch1Algorithm : CompositionAlgorithm
    {
        /// <summary>
        /// This constructor can be called with both parameters null,
        /// just to get the overridden properties.
        /// </summary>
        public Study3Sketch1Algorithm()
            : base()
        {
        }


        public override List<int> MidiChannelIndexPerOutputVoice { get { return new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7 }; } }
        public override List<int> MasterVolumePerOutputVoice { get { return new List<int>() { 100, 100, 100, 100, 100, 100, 100, 100, 100 }; } }
        public override int NumberOfInputVoices { get { return 0; } }
        public override int NumberOfBars { get { return 5; } }

        /// <summary>
        /// See CompositionAlgorithm.DoAlgorithm()
        /// </summary>
        public override List<List<VoiceDef>> DoAlgorithm(List<Krystal> krystals, List<Palette> palettes)
        {
            _krystals = krystals;
            _palettes = palettes;

            List<List<VoiceDef>> bars = new List<List<VoiceDef>>();
            List<VoiceDef> bar1 = CreateBar1();
            bars.Add(bar1);
            int bar2StartMsPos = GetEndMsPosition(bar1);
            List<VoiceDef> bar2 = CreateBar2(bar2StartMsPos);
            bars.Add(bar2);
            int bar3StartMsPos = GetEndMsPosition(bar2);
            List<List<VoiceDef>> bars3to5 = CreateBars3to5(bar3StartMsPos);
            foreach(List<VoiceDef> bar in bars3to5)
            {
                bars.Add(bar);
            }

            Debug.Assert(bars.Count == NumberOfBars);

            base.SetOutputVoiceChannelsAndMasterVolumes(bars[0]);

            return bars;
        }

        #region CreateBar1()
        List<VoiceDef> CreateBar1()
        {
            List<VoiceDef> bar = new List<VoiceDef>();

            foreach(Palette palette in _palettes)
            {
                OutputVoiceDef voice = new OutputVoiceDef();
                bar.Add(voice);
                WriteVoiceMidiDurationDefs1(voice, palette);
            }
            return bar;
        }

        private void WriteVoiceMidiDurationDefs1(OutputVoiceDef voice, Palette palette)
        {
            int msPosition = 0;
            int bar1ChordMsSeparation = 1500;
            for(int i = 0; i < palette.Count;++i)
            {
                IUniqueDef durationDef = palette.UniqueDurationDef(i);
                durationDef.MsPosition = msPosition;
                RestDef restDef = new RestDef(msPosition + durationDef.MsDuration, bar1ChordMsSeparation - durationDef.MsDuration);
                msPosition += bar1ChordMsSeparation;
                voice.UniqueDefs.Add(durationDef);
                voice.UniqueDefs.Add(restDef);
            }
        }
        #endregion CreateBar1()

        #region CreateBar2()
        /// <summary>
        /// This function creates only one bar, but with VoiceDef objects. 
        /// </summary>
        List<VoiceDef> CreateBar2(int bar2StartMsPos)
        {
            List<VoiceDef> bar = new List<VoiceDef>();

            byte channel = 0;
            List<OutputVoiceDef> voiceDefs = new List<OutputVoiceDef>();
            foreach(Palette palette in _palettes)
            {
                bar.Add(new OutputVoiceDef());
                OutputVoiceDef voiceDef = palette.NewOutputVoiceDef();
                voiceDef.SetMsDuration(6000);
                voiceDefs.Add(voiceDef);
                ++channel;
            }
            int msPosition = bar2StartMsPos;
            int maxBarMsPos = 0;
            for(int i = 0; i < voiceDefs.Count; ++i)
            {
                int maxMsPos = WriteVoiceMidiDurationDefsInBar2(bar[i], voiceDefs[i], msPosition, bar2StartMsPos);
                maxBarMsPos = maxBarMsPos > maxMsPos ? maxBarMsPos : maxMsPos;
                msPosition += 1500;
            }

            // now add the final rest in the bar
            for(int i = 0; i < voiceDefs.Count; ++i)
            {
                int mdsdEndPos = voiceDefs[i].EndMsPosition;
                if(maxBarMsPos > mdsdEndPos)
                {
                    RestDef rest2Def = new RestDef(mdsdEndPos, maxBarMsPos - mdsdEndPos);
                    bar[i].UniqueDefs.Add(rest2Def);
                }
            }
            return bar;
        }

        /// <summary>
        /// Writes the first rest (if any) and the VoiceDef to the voice.
        /// Returns the endMsPos of the VoiceDef. 
        /// </summary>
        private int WriteVoiceMidiDurationDefsInBar2(VoiceDef voice, OutputVoiceDef voiceDef, int msPosition, int bar2StartMsPos)
        {
            RestDef rest1Def = null;
            if(msPosition > bar2StartMsPos)
            {
                rest1Def = new RestDef(bar2StartMsPos, msPosition - bar2StartMsPos);
                voice.UniqueDefs.Add(rest1Def);
            }

            voiceDef.StartMsPosition = msPosition;
            foreach(IUniqueDef iumdd in voiceDef)
            {
                voice.UniqueDefs.Add(iumdd);
            }

            return voiceDef.EndMsPosition;
        }
        #endregion CreateBar2()

        #region CreateBars3to5()
        /// <summary>
        /// This function creates three bars, identical to bar2 with two internal barlines.
        /// The VoiceDef objects cross barlines. 
        /// </summary>
        List<List<VoiceDef>> CreateBars3to5(int bar3StartMsPos)
        {
            List<List<VoiceDef>> bars = new List<List<VoiceDef>>();
            List<VoiceDef> threeBars = CreateBar2(bar3StartMsPos);

            //int bar4StartPos = bar3StartMsPos + 6000;
            int bar4StartPos = bar3StartMsPos + 5950;
            List<List<VoiceDef>> bars3And4Plus5 = SplitBar(threeBars, bar4StartPos);
            int bar5StartPos = bar3StartMsPos + 10500;
            List<List<VoiceDef>> bars4and5 = SplitBar(bars3And4Plus5[1], bar5StartPos);

            bars.Add(bars3And4Plus5[0]); // bar 3
            //bars.Add(bars3And4Plus5[1]); // bars 4 and 5
            bars.Add(bars4and5[0]); // bar 4
            bars.Add(bars4and5[1]); // bar 5
            return bars;
        }

        #endregion CreateBars3to5()
    }
}
