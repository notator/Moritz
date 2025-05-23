using Krystals5ObjectLibrary;

using Moritz.Algorithm;
using Moritz.Globals;
using Moritz.Spec;
using Moritz.Symbols;
using Moritz.Xml;

using System;
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
        protected bool CreateScore(List<Krystal> krystals)
        {
            List<Bar> bars = _algorithm.DoAlgorithm(_pageFormat, krystals);

            this.ScoreData = _algorithm.SetScoreRegionsData(bars);

            InsertInitialClefDefs(bars, _pageFormat.InitialClefPerVoice);

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
        /// Inserts a ClefDef at the beginning of the first Trk in each bar, taking any optional ChordDefs into account.
        /// </summary>
        /// <param name="bars"></param>
        /// <param name="initialClefPerVoice">The clefs at the beginning of the score.</param>
        private void InsertInitialClefDefs(List<Bar> bars, List<string> initialClefPerVoice)
        {
            // bars can currently contain cautionary clefs, but no initial clefs
            List<string> currentClefs = new List<string>(initialClefPerVoice);
            int nTrks = bars[0].Trks.Count;
            Debug.Assert(nTrks == currentClefs.Count);

            for(int barIndex = 0; barIndex < bars.Count; barIndex++)
            {
                Bar bar = bars[barIndex];
                for(int trkIndex = 0; trkIndex < nTrks; ++trkIndex)
                {
                    Trk trk = bar.Trks[trkIndex];                    

                    if(trk.UniqueDefs[0] is ClefDef startClef)
                    {
                        currentClefs[trkIndex] = startClef.ClefType;
                        if(barIndex > 0)
                        {
                            Trk trkInPreviousBar = bars[barIndex - 1].Trks[trkIndex];
                            trkInPreviousBar.UniqueDefs.Add(startClef);                           
                        }
                    }
                    else
                    {
                        ClefDef initialClefDef = new ClefDef(currentClefs[trkIndex], 0);
                        trk.Insert(0, initialClefDef);
                    }

                    List<IUniqueDef> iuds = trk.UniqueDefs;
                    for(int iudIndex = 1; iudIndex < iuds.Count; ++iudIndex)
                    {
                        if(iuds[iudIndex] is ClefDef midStaffClefDef)
                        {
                            int result = String.Compare(currentClefs[trkIndex], midStaffClefDef.ClefType);
                            Debug.Assert(result != 0, $"Redundant clef change in voice index {trkIndex}, position index {iudIndex}");                            
                            currentClefs[trkIndex] = midStaffClefDef.ClefType;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates one System per bar (=list of VoiceDefs) in the argument.
        /// The Systems are complete with staves and voices of the correct type:
        /// Each Staff is allocated parallel (empty) Voice fields.
        /// Each Voice has a VoiceDef field that is set using the corresponding
        /// VoiceDef in the argument bar.
        /// A Voice's midi channel will be its index from top to bottom in the score.
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

        /// <summary>
        /// Creates empty Staff objects in each System,
        /// and attaches corresponding VoiceDefs to Voices.
        /// </summary>
        private void CreateEmptyStaves(List<Bar> bars)
        {
            int nStaves = _pageFormat.VoiceIndicesPerStaff.Count;

            for(int systemIndex = 0; systemIndex < Systems.Count; systemIndex++)
            {
                SvgSystem system = Systems[systemIndex];

                int nVoices = 0;

                #region create visible staves
                int trkIndex = 0;
                for(int staffIndex = 0; staffIndex < nStaves; staffIndex++)
                {
                    string staffname = StaffName(systemIndex, staffIndex);
                    Staff staff = new Staff(system, staffname, _pageFormat.StafflinesPerStaff[staffIndex], _pageFormat.Gap, _pageFormat.StafflineStemStrokeWidth);
                    IReadOnlyList<Trk> trks = bars[systemIndex].Trks;

                    List<int> voiceIndices = _pageFormat.VoiceIndicesPerStaff[staffIndex];
                    for(int voiceIndex = 0; voiceIndex < voiceIndices.Count; ++voiceIndex)
                    {
                        Voice voice = new Voice(staff, trks[trkIndex++]);
                        staff.Voices.Add(voice);
                        nVoices++;
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
