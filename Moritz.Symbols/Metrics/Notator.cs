using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System;

using Moritz.Xml;
using Moritz.Spec;

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
        /// In each system, the staff list begins with HiddenOutputStaff objects (if any), followed by OutputStaff objects,
        /// followed by InputStaff objects. InputStaffs are never hidden.
        /// </summary>
        /// <param name="systems"></param>
        public void ConvertVoiceDefsToNoteObjects(List<SvgSystem> systems)
        {
            byte[] currentChannelVelocities = new byte[systems[0].Staves.Count];

            List<ClefDef> voice0ClefDefs = new List<ClefDef>();
            List<ClefDef> voice1ClefDefs = new List<ClefDef>();

            List<string> currentClefPerVisibleStaffList = new List<string>(_pageFormat.ClefsList);

            int systemAbsMsPos = 0;
            for(int systemIndex = 0; systemIndex < systems.Count; ++systemIndex)
            {
                SvgSystem system = systems[systemIndex];
                system.AbsStartMsPosition = systemAbsMsPos;
                int visibleStaffIndex = -1;
                int msPositionReVoiceDef = 0;
                for(int staffIndex = 0; staffIndex < system.Staves.Count; ++staffIndex)
                {
                    Staff staff = system.Staves[staffIndex];
                    if(!(staff is HiddenOutputStaff))
                    {
                        visibleStaffIndex++;
                    }
                    voice0ClefDefs.Clear();
                    voice1ClefDefs.Clear();
                    msPositionReVoiceDef = 0;
                    for(int voiceIndex = 0; voiceIndex < staff.Voices.Count; ++voiceIndex)
                    {
                        Voice voice = staff.Voices[voiceIndex];
                        voice.VoiceDef.AgglomerateRests();
                        float musicFontHeight = (voice is OutputVoice) ? _pageFormat.MusicFontHeight : _pageFormat.MusicFontHeight * _pageFormat.InputStavesSizeFactor;
                        if(!(staff is HiddenOutputStaff))
                        {
                            Debug.Assert(currentClefPerVisibleStaffList[visibleStaffIndex] != null);
                            string clefType = currentClefPerVisibleStaffList[visibleStaffIndex];
                            #region A clef at the beginning of the first voiceDef (stipulated by the algorithm) must agree with the pageFormat clef.                           
                            if(voice.VoiceDef.UniqueDefs.Count > 0)
                            {
                                ClefDef clefDef = voice.VoiceDef.UniqueDefs[0] as ClefDef;
                                if(clefDef != null)
                                {
                                    Debug.Assert((string.Compare(clefType, clefDef.ClefType) == 0), "The first clefs in the score (stipulated by the algorithm) must agree with the pageFormat clefs.");
                                }
                            }
                            #endregion
                            voice.NoteObjects.Add(new Clef(voice, clefType, musicFontHeight));
                        }
                        bool firstLmdd = true;

                        if (staff is InputStaff)
                        {
                            InputVoice inputVoice = staff.Voices[voiceIndex] as InputVoice;
                            if (systemIndex == 0)
                            {
                                InputVoiceDef inputVoiceDef = inputVoice.VoiceDef as InputVoiceDef;
                                inputVoice.MidiChannel = inputVoiceDef.MidiChannel; // The channel is only set in the first system
                            }
                        }
                        msPositionReVoiceDef = 0;
                        for(int iudIndex = 0; iudIndex < voice.VoiceDef.UniqueDefs.Count; ++ iudIndex)
                        {
                            IUniqueDef iud = voice.VoiceDef.UniqueDefs[iudIndex];
                            int absMsPosition = systemAbsMsPos + msPositionReVoiceDef;

                            NoteObject noteObject =
                                SymbolSet.GetNoteObject(voice, absMsPosition, iud, firstLmdd, ref currentChannelVelocities[staffIndex], musicFontHeight);

                            IUniqueSplittableChordDef iscd = iud as IUniqueSplittableChordDef;
                            if(iscd != null && iscd.MsDurationToNextBarline != null)
                            {
                                msPositionReVoiceDef += (int)iscd.MsDurationToNextBarline;
                            }
                            else
                            {
                                msPositionReVoiceDef += iud.MsDuration;
                            }
                            
                            SmallClef smallClef = noteObject as SmallClef;
                            if(smallClef != null)
                            {
                                if(voiceIndex == 0)
                                    voice0ClefDefs.Add(iud as ClefDef);
                                else
                                    voice1ClefDefs.Add(iud as ClefDef);
                            }

                            // Don't add initial clefs that are stipulated by the algorithm here!
                            // (They are the same as in the pageFormat -- see the Debug assertion above).
                            if(!(smallClef != null && iudIndex == 0))
                            {
                                voice.NoteObjects.Add(noteObject);
                            }

                            firstLmdd = false;
                        }
                    }

                    if(!(staff is HiddenOutputStaff) && (voice0ClefDefs.Count > 0 || voice1ClefDefs.Count > 0))
                    {
                        // the main clef on this staff in the next system
                        SetNextSystemClefType(currentClefPerVisibleStaffList, visibleStaffIndex, staffIndex, voice0ClefDefs, voice1ClefDefs);
                    }

                    if(staff.Voices.Count == 2)
                    {
                        InsertInvisibleSmallClefs(staff.Voices, systemAbsMsPos, voice0ClefDefs, voice1ClefDefs);

                        CheckClefTypes(staff.Voices);

                        StandardSymbolSet standardSymbolSet = SymbolSet as StandardSymbolSet;
                        if(standardSymbolSet != null)
                            standardSymbolSet.ForceNaturalsInSynchronousChords(staff);
                    }
                }
                systemAbsMsPos += msPositionReVoiceDef;
            }
        }

        private void SetNextSystemClefType(List<string> currentClefPerVisibleStaffList, int visibleStaffIndex, int staffIndex, List<ClefDef> voice0ClefDefs, List<ClefDef> voice1ClefDefs)
        {
            Debug.Assert(voice0ClefDefs.Count > 0 || voice1ClefDefs.Count > 0);

            ClefDef lastVoice0Def = null;
            if(voice0ClefDefs.Count > 0)
            {
                lastVoice0Def = voice0ClefDefs[voice0ClefDefs.Count - 1];
            }
            ClefDef lastVoice1Def = null;
            if(voice1ClefDefs.Count > 0)
            {
                lastVoice1Def = voice1ClefDefs[voice1ClefDefs.Count - 1];
            }
            string lastClefType = null;
            if(lastVoice0Def != null)
            {
                if((lastVoice1Def == null) || (lastVoice0Def.MsPositionReFirstUD > lastVoice1Def.MsPositionReFirstUD))
                {
                    lastClefType = lastVoice0Def.ClefType;
                }
            }

            if(lastVoice1Def != null)
            {
                if((lastVoice0Def == null) || (lastVoice1Def.MsPositionReFirstUD >= lastVoice0Def.MsPositionReFirstUD))
                {
                    lastClefType = lastVoice1Def.ClefType;
                }
            }

            currentClefPerVisibleStaffList[visibleStaffIndex] = lastClefType;
        }

        /// <summary>
        /// Insert invisible smallClefs into the other voice.
        /// SmallClefs are used by the Notator when deciding how to notate chords.
        /// </summary>
        private void InsertInvisibleSmallClefs(List<Voice> voices, int systemAbsMsPos, List<ClefDef> voice0ClefDefs, List<ClefDef> voice1ClefDefs)
        {
            Debug.Assert(voices.Count == 2);
            if(voice1ClefDefs.Count > 0)
                InsertInvisibleClefChanges(voices[0], systemAbsMsPos, voice1ClefDefs);
            if(voice0ClefDefs.Count > 0)
                InsertInvisibleClefChanges(voices[1], systemAbsMsPos, voice0ClefDefs);
        }

        /// <summary>
        /// Check that both voices really do contain the same clefsTypes.
        /// This becomes very important later on. It is assumed (e.g.) in
        ///     SvgScore.JoinToPreviousSystem(int systemIndex)
        /// and
        ///     ChordMetrics.GetStaffParameters(NoteObject rootObject) 
        /// </summary>
        private void CheckClefTypes(List<Voice> voices)
        {
            Debug.Assert(voices.Count == 2);
            List<Clef> voice0Clefs = GetClefs(voices[0]);
            List<Clef> voice1Clefs = GetClefs(voices[1]);
            Debug.Assert(voice0Clefs.Count == voice1Clefs.Count);
            for(int i = 0; i < voice0Clefs.Count; ++i)
            {
                Debug.Assert(voice0Clefs[i].ClefType == voice1Clefs[i].ClefType);
            }
        }

        private List<Clef> GetClefs(Voice voice)
        {
            List<Clef> clefs = new List<Clef>();
            foreach(NoteObject noteObject in voice.NoteObjects)
            {
                Clef clef = noteObject as Clef;
                if(clef != null)
                    clefs.Add(clef);
            }
            return clefs;
        }

        private void InsertInvisibleClefChanges(Voice voice, int systemAbsMsPos, List<ClefDef> clefDefs)
        {
            foreach(ClefDef ccd in clefDefs)
            {
                int absPos = systemAbsMsPos + ccd.MsPositionReFirstUD;
                SmallClef invisibleSmallClef = new SmallClef(voice, ccd.ClefType, absPos, (_pageFormat.MusicFontHeight * _pageFormat.SmallFactor));
                invisibleSmallClef.IsVisible = false;
                InsertInvisibleClefChangeInNoteObjects(voice, invisibleSmallClef);
            }
        }

        private void InsertInvisibleClefChangeInNoteObjects(Voice voice, SmallClef invisibleSmallClef)
        {
            Debug.Assert(!(voice.NoteObjects[voice.NoteObjects.Count - 1] is Barline));

            int absMsPos = invisibleSmallClef.AbsMsPosition;
            List<DurationSymbol> durationSymbols = new List<DurationSymbol>();
            foreach(DurationSymbol durationSymbol in voice.DurationSymbols)
            {
                durationSymbols.Add(durationSymbol);
            }

            Debug.Assert(durationSymbols.Count > 0);

            if(absMsPos <= durationSymbols[0].AbsMsPosition)
            {
                InsertBeforeDS(voice.NoteObjects, durationSymbols[0], invisibleSmallClef);
            }
            else if(absMsPos > durationSymbols[durationSymbols.Count - 1].AbsMsPosition)
            {
                // the noteObjects do not yet have a final barline (see Debug.Assert() above)
                voice.NoteObjects.Add(invisibleSmallClef);
            }
            else
            {
                Debug.Assert(durationSymbols.Count > 1);
                for(int i = 1; i < durationSymbols.Count; ++i)
                {
                    if(durationSymbols[i - 1].AbsMsPosition < absMsPos && durationSymbols[i].AbsMsPosition >= absMsPos)
                    {
                        InsertBeforeDS(voice.NoteObjects, durationSymbols[i], invisibleSmallClef);
                        break;
                    }
                }
            }
        }

        private void InsertBeforeDS(List<NoteObject> noteObjects, DurationSymbol insertBeforeDS, SmallClef invisibleSmallClef)
        {
            for(int i = 0; i < noteObjects.Count; ++i)
            {
                DurationSymbol durationSymbol = noteObjects[i] as DurationSymbol;
                if(durationSymbol != null && durationSymbol == insertBeforeDS)
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
                foreach(Voice voice in staff.Voices)
                {
                    currentVoiceMsPositions.Add(0);
                }
            }
            return currentMsPositionPerVoicePerStaff;
        }

        /// <summary>
        /// The systems do not yet contain Metrics info.
        /// They are given Metrics and justified horizontally and vertically (internally) 
        /// inside system.RealizeGraphics().
        /// Returns false if the score can't be justified horizontally
        /// or a single system will not fit vertially on a page.
        /// In these cases, a MessageBox is displayed (explaining the reason for the failure)
        /// by the inner function that discovers the error.
        /// </summary>
        public bool CreateMetricsAndJustifySystems(List<SvgSystem> systems)
        {
            bool success = true;
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
                        success = systems[sysIndex].MakeGraphics(graphics, sysIndex + 1, _pageFormat, leftMargin);
                        if(!success)
                            break;
                    }
                }
            }
            return success;
        }

        private float GetLeftMarginPos(SvgSystem system, Graphics graphics, PageFormat pageFormat)
        {
            float leftMarginPos = pageFormat.LeftMarginPos;
            float maxNameWidth = 0;
            foreach(Staff staff in system.Staves)
            {
                foreach(NoteObject noteObject in staff.Voices[0].NoteObjects)
                {
                    Barline firstBarline = noteObject as Barline;
                    if(firstBarline != null)
                    {
                        foreach(DrawObject drawObject in firstBarline.DrawObjects)
                        {
							StaffNameText staffName = drawObject as StaffNameText;
                            if(staffName != null)
                            {
                                Debug.Assert(staffName.TextInfo != null);

                                TextMetrics staffNameMetrics = new TextMetrics(CSSClass.staffName, graphics, staffName.TextInfo);
                                float nameWidth = staffNameMetrics.Right - staffNameMetrics.Left;
                                maxNameWidth = (maxNameWidth > nameWidth) ? maxNameWidth : nameWidth;
                            }
                        }
                        break;
                    }
                }
            }
            leftMarginPos = maxNameWidth + (pageFormat.Gap * 2.0F);
            leftMarginPos = (leftMarginPos > pageFormat.LeftMarginPos) ? leftMarginPos : pageFormat.LeftMarginPos;

            return leftMarginPos;
        }

        #region properties
        protected readonly PageFormat _pageFormat;

        public SymbolSet SymbolSet = null;

        #endregion

    }
}
