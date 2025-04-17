using Moritz.Globals;
using Moritz.Spec;
using Moritz.Xml;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Moritz.Symbols
{
    public class Notator
    {
        public Notator(PageFormat pageFormat)
        {
            _pageFormat = pageFormat;
            bool error = false;
            switch(pageFormat.ChordSymbolType)
            {
                case "standard":
                    SymbolSet = new StandardSymbolSet(false); // _coloredVelocities = false;
                    break;
                case "coloredVelocities":
                    SymbolSet = new StandardSymbolSet(true); // _coloredVelocities = true;
                    break;
                case "none":
                    SymbolSet = null;
                    break;
                default:
                    error = true;
                    break;
            }
            if(error)
                throw new ApplicationException("Cannot construct Notator!");
        }

        /// <summary>
        /// There is still one system per bar.
        /// In each system, the staff list contains Staff objects containing empty Voice objects
        /// containing VoiceDefs containing one or more Trks. Only Trk[0] is used to construct the score's graphics.
		/// Trk[0] contains ClefDefs. The first is converted to a Clef, later ones to SmallClefs.
		/// An Exception will be thrown if a SmallClefDef is found on the lower voiceDef in a staff in the systems input.
		/// Small clefs (if there are any) are copied from the top to the bottom voice (if there is one) on each staff.
        /// </summary>
        /// <param name="systems"></param>
        public void ConvertVoiceDefsToNoteObjects(List<SvgSystem> systems)
        {
            int[] currentChannelVelocities = new int[systems[0].Staves.Count];
            var topVoiceSmallClefs = new List<SmallClef>();

            int systemAbsMsPos = 0;
            for(int systemIndex = 0; systemIndex < systems.Count; ++systemIndex)
            {
                SvgSystem system = systems[systemIndex];
                system.AbsStartMsPosition = systemAbsMsPos;
                int msPositionReVoiceDef = 0;
                for(int staffIndex = 0; staffIndex < system.Staves.Count; ++staffIndex)
                {
                    Staff staff = system.Staves[staffIndex];
                    msPositionReVoiceDef = 0;
                    topVoiceSmallClefs.Clear();
                    for(int voiceIndex = 0; voiceIndex < staff.Voices.Count; ++voiceIndex)
                    {
                        Voice voice = staff.Voices[voiceIndex];
                        Trk trk = voice.Trk;

                        trk.AssertConsistency();

                        msPositionReVoiceDef = 0;
                        List<IUniqueDef> iuds = trk.UniqueDefs;
                        Debug.Assert(iuds[0] is ClefDef);

                        for(int iudIndex = 0; iudIndex < iuds.Count; ++iudIndex)
                        {
                            IUniqueDef iud = iuds[iudIndex];
                            int absMsPosition = systemAbsMsPos + msPositionReVoiceDef;

                            NoteObject noteObject =
                                SymbolSet.GetNoteObject(voice, absMsPosition, iud, iudIndex, ref currentChannelVelocities[staffIndex], _pageFormat);

                            if(noteObject is SmallClef smallClef)
                            {
                                if(voiceIndex == 0)
                                {
                                    if(staff.Voices.Count > 1)
                                    {
                                        topVoiceSmallClefs.Add(smallClef);
                                    }
                                }
                                else
                                {
                                    throw new Exception("SmallClefs may not be defined for a lower voice. They will be copied from the top voice");
                                }
                            }

                            if(iud is MidiChordDef mcd && mcd.MsDurationToNextBarline != null)
                            {
                                msPositionReVoiceDef += (int)mcd.MsDurationToNextBarline;
                            }
                            else
                            {
                                msPositionReVoiceDef += iud.MsDuration;
                            }

                            voice.NoteObjects.Add(noteObject);
                        }
                    }

                    if(staff.Voices.Count == 2)
                    {
                        if(topVoiceSmallClefs.Count > 0)
                        {
                            AddSmallClefsToLowerVoice(staff.Voices[1], topVoiceSmallClefs);
                        }

                        if(SymbolSet is StandardSymbolSet standardSymbolSet)
                            standardSymbolSet.ForceNaturalsInSynchronousChords(staff);
                    }
                }
                systemAbsMsPos += msPositionReVoiceDef;
            }
        }

        private void AddSmallClefsToLowerVoice(Voice voice, List<SmallClef> clefsInTopStaff)
        {
            foreach(SmallClef smallClef in clefsInTopStaff)
            {
                InvisibleSmallClef invisibleSmallClef = new InvisibleSmallClef(voice, smallClef.ClefType, smallClef.AbsMsPosition)
                {
                    IsVisible = false
                };
                InsertInvisibleClefChangeInNoteObjects(voice, invisibleSmallClef);
            }
        }

        private void InsertInvisibleClefChangeInNoteObjects(Voice voice, InvisibleSmallClef invisibleSmallClef)
        {
            int absMsPos = invisibleSmallClef.AbsMsPosition;
            List<DurationSymbol> durationSymbols = new List<DurationSymbol>();
            foreach(DurationSymbol durationSymbol in voice.DurationSymbols)
            {
                durationSymbols.Add(durationSymbol);
            }

            Debug.Assert(!(voice.NoteObjects[voice.NoteObjects.Count - 1] is Barline));
            Debug.Assert(durationSymbols.Count > 0);
            Debug.Assert(absMsPos > durationSymbols[0].AbsMsPosition);

            if(absMsPos > durationSymbols[durationSymbols.Count - 1].AbsMsPosition)
            {
                // the noteObjects do not yet have a final barline (see Debug.Assert() above)
                voice.NoteObjects.Add(invisibleSmallClef);
            }
            else
            {
                for(int i = durationSymbols.Count - 2; i >= 0; --i)
                {
                    if(durationSymbols[i].AbsMsPosition < absMsPos)
                    {
                        InsertBeforeDS(voice.NoteObjects, durationSymbols[i + 1], invisibleSmallClef);
                        break;
                    }
                }
            }
        }

        private void InsertBeforeDS(List<NoteObject> noteObjects, DurationSymbol insertBeforeDS, InvisibleSmallClef invisibleSmallClef)
        {
            for(int i = 0; i < noteObjects.Count; ++i)
            {
                if(noteObjects[i] is DurationSymbol durationSymbol && durationSymbol == insertBeforeDS)
                {
                    noteObjects.Insert(i, invisibleSmallClef);
                    break;
                }
            }
        }

        /// <summary>
        /// The current msPosition of a voice will be retrievable as currentMsPositionPerVoicePerStaff[staffIndex][voiceIndex].
        /// </summary>
        /// <param name="system"></param>
        /// <returns></returns>
        private List<List<int>> InitializeCurrentMsPositionPerVoicePerStaff(SvgSystem system)
        {
            List<List<int>> currentMsPositionPerVoicePerStaff = new List<List<int>>();
            foreach(Staff staff in system.Staves)
            {
                List<int> currentVoiceMsPositions = new List<int>();
                currentMsPositionPerVoicePerStaff.Add(currentVoiceMsPositions);
                for(int i = 0; i < staff.Voices.Count; i++)
                {
                    currentVoiceMsPositions.Add(0);
                }
            }
            return currentMsPositionPerVoicePerStaff;
        }

        /// <summary>
        /// The systems do not yet contain Metrics info.
        /// Puts up a Warning Message Box if there are overlapping symbols after the score has been justified horizontally.
        /// </summary>
        public void CreateMetricsAndJustifySystems(List<SvgSystem> systems)
        {
            // set when there are overlaps...
            List<Tuple<int, int, string>> overlaps;
            List<Tuple<int, int, string>> allOverlaps = new List<Tuple<int, int, string>>();

            using(Image image = new Bitmap(1, 1))
            {
                using(Graphics graphics = Graphics.FromImage(image)) // used for measuring strings
                {
                    float system1LeftMarginPos = GetLeftMarginPos(systems[0], graphics, _pageFormat);
                    float otherSystemsLeftMarginPos = 0F;
                    if(systems.Count > 1)
                        otherSystemsLeftMarginPos = GetLeftMarginPos(systems[1], graphics, _pageFormat);

                    for(int sysIndex = 0; sysIndex < systems.Count; ++sysIndex)
                    {
                        float leftMargin = (sysIndex == 0) ? system1LeftMarginPos : otherSystemsLeftMarginPos;
                        overlaps = systems[sysIndex].MakeGraphics(graphics, sysIndex + 1, _pageFormat, leftMargin);
                        foreach(Tuple<int, int, string> overlap in overlaps)
                        {
                            allOverlaps.Add(overlap);
                        }
                    }
                }
            }
            if(allOverlaps.Count > 0)
            {
                WarnAboutOverlaps(allOverlaps);
            }
        }

        private void WarnAboutOverlaps(List<Tuple<int, int, string>> allOverlaps)
        {
            string msg1 = "There was not enough horizontal space for all the symbols in\n" +
                          "the following systems:\n";
            string msg2Spacer = "      ";
            string msg3 = "\n" +
                          "Possible solutions:\n" +
                          "    Reduce the number of bars in the system(s).\n" +
                          "    Set a smaller gap size for the score.";

            StringBuilder sb = new StringBuilder();
            sb.Append(msg1);
            foreach(Tuple<int, int, string> t in allOverlaps)
            {
                sb.Append($"{msg2Spacer}System number: {t.Item1} -- ({t.Item2} overlaps in {t.Item3} voices)\n");
            }
            sb.Append(msg3);

            MessageBox.Show(sb.ToString(), "Overlap(s) Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private float GetLeftMarginPos(SvgSystem system, Graphics graphics, PageFormat pageFormat)
        {
            float maxNameWidth = 0;
            foreach(Staff staff in system.Staves)
            {
                foreach(NoteObject noteObject in staff.Voices[0].NoteObjects)
                {
                    if(noteObject is NormalBarline firstBarline)
                    {
                        foreach(DrawObject drawObject in firstBarline.DrawObjects)
                        {
                            if(drawObject is StaffNameText staffName)
                            {
                                Debug.Assert(staffName.TextInfo != null);

                                TextMetrics staffNameMetrics = new TextMetrics(CSSObjectClass.staffName, graphics, staffName.TextInfo);
                                float nameWidth = staffNameMetrics.Right - staffNameMetrics.Left;
                                maxNameWidth = (maxNameWidth > nameWidth) ? maxNameWidth : nameWidth;
                            }
                        }
                        break;
                    }
                }
            }
            float leftMarginPos = maxNameWidth + (pageFormat.Gap * 2.0F);
            leftMarginPos = (leftMarginPos > pageFormat.LeftMarginPos) ? leftMarginPos : pageFormat.LeftMarginPos;

            return leftMarginPos;
        }

        #region properties
        protected readonly PageFormat _pageFormat;

        public SymbolSet SymbolSet = null;

        #endregion

    }
}
