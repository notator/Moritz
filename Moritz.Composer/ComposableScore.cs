using Krystals5ObjectLibrary;

using Moritz.Algorithm;
using Moritz.Palettes;
using Moritz.Spec;
using Moritz.Symbols;
using Moritz.Xml;

using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Composer
{
    public partial class ComposableScore : SvgScore
    {
        public ComposableScore(string folder, string scoreFolderName, CompositionAlgorithm algorithm, string keywords, string comment, PageFormat pageFormat)
            : base(folder, scoreFolderName, keywords, comment, pageFormat)
        {
            _algorithm = algorithm;
        }

        /// <summary>
        /// Called by the derived class after setting _algorithm and Notator.
        /// Returns false if systems cannot be fit vertically on the page. Otherwise true.
        /// </summary>
        protected bool CreateScore(List<Krystal> krystals, List<Palette> palettes)
        {
            List<Bar> bars = _algorithm.DoAlgorithm(krystals, palettes);

            CheckBars(bars);

            this.ScoreData = _algorithm.SetScoreRegionsData(bars);

            InsertInitialClefDefs(bars, _pageFormat.InitialClefPerMIDIChannel);

            CreateEmptySystems(bars); // one system per bar

            bool success = true;
            if(_pageFormat.ChordSymbolType != "none") // set by AudioButtonsControl
            {
                Notator.ConvertVoiceDefsToNoteObjects(this.Systems);

                FinalizeSystemStructure(); // adds barlines, joins bars to create systems, etc.

                /// The systems do not yet contain Metrics info.
                /// The systems are given Metrics inside the following function then justified internally,
                /// both horizontally and vertically.
                Notator.CreateMetricsAndJustifySystems(this.Systems);
                success = CreatePages();
            }

            return success;
        }

        /// <summary>
        /// Inserts a ClefDef at the beginning of each Trk in each bar, taking any cautionaryChordDefs into account.
        /// </summary>
        /// <param name="bars"></param>
        /// <param name="initialClefPerMIDIChannel">The clefs at the beginning of the score.</param>
        private void InsertInitialClefDefs(List<Bar> bars, List<string> initialClefPerMIDIChannel)
        {
            // bars can currently contain cautionary clefs, but no initial clefs
            List<string> currentClefs = new List<string>(initialClefPerMIDIChannel);
            int nVoiceDefs = bars[0].ChannelDefs.Count;        
            foreach(Bar bar in bars)
            {
                for(int i = 0; i < nVoiceDefs; ++i)
                {
                    ClefDef initialClefDef = new ClefDef(currentClefs[i], 0); // msPos is set later in Notator.ConvertVoiceDefsToNoteObjects()
                    foreach(var trk in bar.ChannelDefs[i].Trks)
                    {
                        trk.Insert(0, initialClefDef);
                        List<IUniqueDef> iuds = trk.UniqueDefs;
                        for(int j = 1; j < iuds.Count; ++j)
                        {
                            if(iuds[j] is ClefDef cautionaryClefDef)
                            {
                                currentClefs[i] = cautionaryClefDef.ClefType;
                            }
                        }
                    }
                }
            }
        }

        private void CheckBars(List<Bar> bars)
        {
            string errorString = null;
            if(bars.Count == 0)
                errorString = "The algorithm has not created any bars!";
            else
            {
                errorString = BasicChecks(bars);
            }
            Debug.Assert(string.IsNullOrEmpty(errorString), errorString);
        }
        #region private to CheckBars(...)
        private string BasicChecks(List<Bar> bars)
        {
            string errorString = null;
            //List<int> visibleLowerVoiceIndices = new List<int>();
            //Dictionary<int, string> upperVoiceClefDict = GetUpperVoiceClefDict(bars[0], _pageFormat, /*sets*/ visibleLowerVoiceIndices);

            for(int barIndex = 0; barIndex < bars.Count; ++barIndex)
            {
                Bar bar = bars[barIndex];
                IReadOnlyList<ChannelDef> channelDefs = bar.ChannelDefs;

                if(channelDefs.Count == 0)
                {
                    errorString = $"Bar (index {barIndex}) contains no voices.";
                    break;
                }

                for(int channelIndex = 0; channelIndex < channelDefs.Count; ++channelIndex)
                {
                    ChannelDef channelDef = channelDefs[channelIndex];
                    for(int trkIndex = 0; trkIndex < channelDef.Trks.Count; ++trkIndex)
                    {
                        if(channelDef.Trks[trkIndex].UniqueDefs.Count == 0)
                        {
                            errorString = $"Trk (index {trkIndex}) in Voice (index {channelIndex}) in Bar (index {barIndex}) has an empty UniqueDefs list.";
                            break;
                        }
                    }
                }

                errorString = CheckThatLowerVoicesHaveNoSmallClefs(channelDefs);

                if(!string.IsNullOrEmpty(errorString))
                    break;
            }
            return errorString;
        }

        private string CheckThatLowerVoicesHaveNoSmallClefs(IReadOnlyList<ChannelDef> channelDefs)
        {
            string errorString = "";

            List<int> lowerVoiceIndices = GetLowerVoiceIndices();

            foreach(int lowerVoiceIndex in lowerVoiceIndices)
            {
                var trks = channelDefs[lowerVoiceIndex].Trks;
                foreach(var trk in trks)
                {

                    foreach(IUniqueDef iud in trk.UniqueDefs)
                    {
                        if(iud is ClefDef)
                        {
                            errorString = "Small Clefs may not be defined for lower voices on a staff.";
                            break;
                        }
                    }
                }
                if(!string.IsNullOrEmpty(errorString))
                    break;
            }

            return errorString;
        }

        private List<int> GetLowerVoiceIndices()
        {
            List<int> lowerVoiceIndices = new List<int>();
            int voiceIndex = 0;

            List<List<byte>> voiceIndicesPerStaff = _pageFormat.VoicesPerStaff;

            for(int staffIndex = 0; staffIndex < voiceIndicesPerStaff.Count; ++staffIndex)
            {
                if(voiceIndicesPerStaff[staffIndex].Count > 1)
                {
                    voiceIndex++;
                    lowerVoiceIndices.Add(voiceIndex);
                }
                voiceIndex++;
            }

            return lowerVoiceIndices;
        }

        private int NOutputVoices(List<ChannelDef> bar1)
        {
            int nOutputVoices = 0;
            foreach(ChannelDef channelDef in bar1)
            {
                if(channelDef is Trk)
                {
                    nOutputVoices++;
                }
            }
            return nOutputVoices;
        }

        #endregion

        /// <summary>
        /// Creates one System per bar (=list of ChannelDefs) in the argument.
        /// The Systems are complete with staves and voices of the correct type:
        /// Each InputStaff is allocated parallel (empty) InputVoice fields.
        /// Each OutputStaff is allocated parallel (empty) OutputVoice fields.
        /// Each Voice has a ChannelDef field that is allocated to the corresponding
        /// ChannelDef from the argument.
        /// The OutputVoices have MIDIChannels arranged according to _pageFormat.OutputMIDIChannelsPerStaff.
        /// The InputVoices have MIDIChannels arranged according to _pageFormat.InputMIDIChannelsPerStaff.
        /// OutputVoices are given a midi channel allocated from top to bottom in the printed score.
        /// </summary>
        public void CreateEmptySystems(List<Bar> bars)
        {
            foreach(Bar bar in bars)
            {
                SvgSystem system = new SvgSystem(this);
                this.Systems.Add(system);
            }

            CreateEmptyStaves(bars);
        }

        private void CreateEmptyStaves(List<Bar> bars)
        {
            int nStaves = _pageFormat.VoicesPerStaff.Count;

            for(int systemIndex = 0; systemIndex < Systems.Count; systemIndex++)
            {
                SvgSystem system = Systems[systemIndex];
                IReadOnlyList<ChannelDef> channelDefs = bars[systemIndex].ChannelDefs;

                #region create visible staves
                for(int staffIndex = 0; staffIndex < nStaves; staffIndex++)
                {
                    string staffname = StaffName(systemIndex, staffIndex);
                    Staff staff = new Staff(system, staffname, _pageFormat.StafflinesPerStaff[staffIndex], _pageFormat.Gap, _pageFormat.StafflineStemStrokeWidth);

                    List<byte> channelIndices = _pageFormat.VoicesPerStaff[staffIndex];
                    for(int channelIndex = 0; channelIndex < channelIndices.Count; ++channelIndex)
                    {
                        Voice voice = new Voice(staff);
                        staff.Voices.Add(voice);
                    }
                    SetStemDirections(staff);
                    system.Staves.Add(staff);
                }
                #endregion
            }
        }

        private string StaffName(int systemIndex, int staffIndex)
        {
            if(systemIndex == 0)
            {
                return _pageFormat.LongStaffNames[staffIndex];
            }
            else
            {
                return _pageFormat.ShortStaffNames[staffIndex];
            }
        }

        private void SetStemDirections(Staff staff)
        {
            if(staff.Voices.Count == 1)
            {
                staff.Voices[0].StemDirection = VerticalDir.none;
            }
            else
            {
                Debug.Assert(staff.Voices.Count == 2);
                staff.Voices[0].StemDirection = VerticalDir.up;
                staff.Voices[1].StemDirection = VerticalDir.down;
            }
        }
        protected CompositionAlgorithm _algorithm = null;
    }
}
