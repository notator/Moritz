using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System;

using Moritz.Score.Midi;

namespace Moritz.Score.Notation
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
                    SymbolSet = new StandardSymbolSet();
                    break;
                case "2b2":
                    SymbolSet = new Study2b2SymbolSet();
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

        public void CreateSystems(SvgScore svgScore, List<List<Voice>> voicesPerSystemPerBar, float gap)
        {             
            foreach(List<Voice> systemVoices in voicesPerSystemPerBar)
            {
                int midiVoice_StaffIndex = 0;
                int inputVoice_StaffIndex = 0;
                int nOutputVoices = 0;
                SvgSystem system = new SvgSystem(svgScore);
                svgScore.Systems.Add(system);
                for(int staffIndex = 0; staffIndex < _pageFormat.StafflinesPerStaff.Count; staffIndex++)
                {
                    string staffname = null;
                    if(svgScore.Systems.Count == 1)
                    {
                        staffname = _pageFormat.LongStaffNames[staffIndex];
                    }
                    else
                    {
                        staffname = _pageFormat.ShortStaffNames[staffIndex];                        
                    }
                    Staff staff;
                    if(staffIndex >= _pageFormat.MidiChannelsPerStaff.Count)
                    {
                        staff = new InputStaff(system, staffname, _pageFormat.StafflinesPerStaff[staffIndex], gap * _pageFormat.InputStavesSizeFactor);
                        
                        List<byte> inputVoiceIndices = _pageFormat.InputVoiceIndicesPerStaff[inputVoice_StaffIndex];
                        for(int i = 0; i < inputVoiceIndices.Count; ++i)
                        {
                            int systemVoiceIndex = nOutputVoices + inputVoiceIndices[i];
                            staff.Voices.Add(systemVoices[systemVoiceIndex]);
                            systemVoices[systemVoiceIndex].Staff = staff;
                        }
                        inputVoice_StaffIndex++;
                        SetStemDirections(staff);
                    }
                    else
                    {
                        staff = new OutputStaff(system, staffname, _pageFormat.StafflinesPerStaff[staffIndex], gap);

                        List<byte> midiChannels = _pageFormat.MidiChannelsPerStaff[midiVoice_StaffIndex];
                        for(int i = 0; i < midiChannels.Count; ++i)
                        {
                            int systemVoiceIndex = midiChannels[i];
                            staff.Voices.Add(systemVoices[systemVoiceIndex]);
                            systemVoices[systemVoiceIndex].Staff = staff;
                            nOutputVoices++;
                        }
                        midiVoice_StaffIndex++;
                        SetStemDirections(staff);
                    }
                    system.Staves.Add(staff);
                }
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

        public void AddSymbolsToSystems(List<SvgSystem> systems)
        {
            byte[] currentChannelVelocities = new byte[systems[0].Staves.Count];

            List<UniqueClefChangeDef> voice0ClefChangeDefs = new List<UniqueClefChangeDef>();
            List<UniqueClefChangeDef> voice1ClefChangeDefs = new List<UniqueClefChangeDef>();

            for(int systemIndex = 0; systemIndex < systems.Count; ++systemIndex)
            {
                SvgSystem system = systems[systemIndex];
                for(int staffIndex = 0; staffIndex < system.Staves.Count; ++staffIndex)
                {
                    Staff staff = system.Staves[staffIndex];
                    voice0ClefChangeDefs.Clear();
                    voice1ClefChangeDefs.Clear();
                    for(int voiceIndex = 0; voiceIndex < staff.Voices.Count; ++voiceIndex)
                    {
                        Debug.Assert(_pageFormat.ClefsList[staffIndex] != null);
                        Voice voice = staff.Voices[voiceIndex];                        
                        float musicFontHeight = (voice is OutputVoice) ? _pageFormat.MusicFontHeight : _pageFormat.MusicFontHeight * _pageFormat.InputStavesSizeFactor;
                        voice.NoteObjects.Add(new ClefSymbol(voice, _pageFormat.ClefsList[staffIndex], musicFontHeight));
                        bool firstLmdd = true;

                        // change voice.UniqueMidiDurationDefs to a new list voice.NoteObjectDefs.
                        // continue to add existing UniqueMidiChordDefs anda UniqueMidiRestDef to this list.
                        // A NoteObjectDef is either a IUniqueMidiDurationDef (a UniqueMidiChordDef or a UniqueMidiRestDef) or
                        // a  
                        foreach(IUniqueMidiDurationDef iumdd in voice.UniqueMidiDurationDefs)
                        {
                            NoteObject noteObject =
                                SymbolSet.GetNoteObject(voice, iumdd, firstLmdd, ref currentChannelVelocities[staffIndex], musicFontHeight);

                            ClefChangeSymbol clefChangeSymbol = noteObject as ClefChangeSymbol;
                            if(clefChangeSymbol != null)
                            {
                                if(voiceIndex == 0)
                                    voice0ClefChangeDefs.Add(iumdd as UniqueClefChangeDef);
                                else
                                    voice1ClefChangeDefs.Add(iumdd as UniqueClefChangeDef);
                            }

                            voice.NoteObjects.Add(noteObject);

                            firstLmdd = false;
                        }
                    }

                    if(voice0ClefChangeDefs.Count > 0 || voice1ClefChangeDefs.Count > 0)
                    { 
                        // the main clef on this staff in the next system
                        SetNextSystemClefType(staffIndex, voice0ClefChangeDefs, voice1ClefChangeDefs);
                    }

                    if(staff.Voices.Count == 2)
                    {
                        InsertInvisibleClefChangeSymbols(staff.Voices, voice0ClefChangeDefs, voice1ClefChangeDefs);

                        CheckClefTypes(staff.Voices);

                        StandardSymbolSet standardSymbolSet = SymbolSet as StandardSymbolSet;
                        if(standardSymbolSet != null)
                            standardSymbolSet.ForceNaturalsInSynchronousChords(staff);
                    }
                }
            }
        }

        private void SetNextSystemClefType(int staffIndex, List<UniqueClefChangeDef> voice0ClefChangeDefs, List<UniqueClefChangeDef> voice1ClefChangeDefs)
        {
            Debug.Assert(voice0ClefChangeDefs.Count > 0 || voice1ClefChangeDefs.Count > 0);

            UniqueClefChangeDef lastVoice0Def = null;
            if(voice0ClefChangeDefs.Count > 0)
            {
                lastVoice0Def = voice0ClefChangeDefs[voice0ClefChangeDefs.Count - 1];
            }
            UniqueClefChangeDef lastVoice1Def = null;
            if(voice1ClefChangeDefs.Count > 0)
            {
                lastVoice1Def = voice1ClefChangeDefs[voice1ClefChangeDefs.Count - 1];
            }
            string lastClefType = null;
            if(lastVoice0Def != null)
            {
                if((lastVoice1Def == null) || (lastVoice0Def.MsPosition > lastVoice1Def.MsPosition))
                {
                    lastClefType = lastVoice0Def.ClefType;
                }
            }

            if(lastVoice1Def != null)
            {
                if((lastVoice0Def == null) || (lastVoice1Def.MsPosition >= lastVoice0Def.MsPosition))
                {
                    lastClefType = lastVoice1Def.ClefType;
                }
            }

            _pageFormat.ClefsList[staffIndex] = lastClefType;           
        }

        /// <summary>
        /// Insert invisible clefChangeSymbols into the other voice.
        /// ClefChangeSymbols are used by the Notator when deciding how to notate chords.
        /// </summary>
        private void InsertInvisibleClefChangeSymbols(List<Voice> voices, List<UniqueClefChangeDef> voice0ClefChangeDefs, List<UniqueClefChangeDef> voice1ClefChangeDefs)
        {
            Debug.Assert(voices.Count == 2);
            if(voice1ClefChangeDefs.Count > 0)
                InsertInvisibleClefChanges(voices[0], voice1ClefChangeDefs);
            if(voice0ClefChangeDefs.Count > 0)
                InsertInvisibleClefChanges(voices[1], voice0ClefChangeDefs);
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
            List<ClefSymbol> voice0Clefs = GetClefs(voices[0]);
            List<ClefSymbol> voice1Clefs = GetClefs(voices[1]);
            Debug.Assert(voice0Clefs.Count == voice1Clefs.Count);
            for(int i = 0; i < voice0Clefs.Count; ++i)
            {
                Debug.Assert(voice0Clefs[i].ClefType == voice1Clefs[i].ClefType);
            }
        }

        private List<ClefSymbol> GetClefs(Voice voice)
        {
            List<ClefSymbol> clefs = new List<ClefSymbol>();
            foreach(NoteObject noteObject in voice.NoteObjects)
            {
                ClefSymbol clef = noteObject as ClefSymbol;
                if(clef != null)
                    clefs.Add(clef);
            }
            return clefs;
        }

        private void InsertInvisibleClefChanges(Voice voice, List<UniqueClefChangeDef> clefChangeDefs)
        {
            foreach(UniqueClefChangeDef ccd in clefChangeDefs)
            {
                ClefChangeSymbol invisibleClefChangeSymbol = new ClefChangeSymbol(voice, ccd.ClefType, _pageFormat.CautionaryNoteheadsFontHeight, ccd.MsPosition);
                invisibleClefChangeSymbol.IsVisible = false;
                InsertInvisibleClefChangeInNoteObjects(voice, invisibleClefChangeSymbol);
            }
        }

        private void InsertInvisibleClefChangeInNoteObjects(Voice voice, ClefChangeSymbol invisibleClefChangeSymbol)
        {
            Debug.Assert(!(voice.NoteObjects[voice.NoteObjects.Count-1] is Barline));

            int msPos = invisibleClefChangeSymbol.MsPosition;
            List<DurationSymbol> durationSymbols = new List<DurationSymbol>();
            foreach(DurationSymbol durationSymbol in voice.DurationSymbols)
            {
                durationSymbols.Add(durationSymbol);
            }

            Debug.Assert(durationSymbols.Count > 0);

            if(msPos <= durationSymbols[0].MsPosition)
            {
                InsertBeforeDS(voice.NoteObjects, durationSymbols[0], invisibleClefChangeSymbol);
            }
            else if(msPos > durationSymbols[durationSymbols.Count-1].MsPosition)
            {
                // the noteObjects do not yet have a final barline (see Debug.Assert() above)
                voice.NoteObjects.Add(invisibleClefChangeSymbol);
            }
            else
            {
                Debug.Assert(durationSymbols.Count > 1);
                for(int i = 1; i < durationSymbols.Count; ++i)
                {
                    if(durationSymbols[i - 1].MsPosition < msPos && durationSymbols[i].MsPosition >= msPos)
                    {
                        InsertBeforeDS(voice.NoteObjects, durationSymbols[i], invisibleClefChangeSymbol);
                        break;
                    }
                }
            }
        }

        private void InsertBeforeDS(List<NoteObject> noteObjects, DurationSymbol insertBeforeDS, ClefChangeSymbol invisibleClefChangeSymbol)
        {
            for(int i = 0; i < noteObjects.Count; ++i)
            {
                DurationSymbol durationSymbol = noteObjects[i] as DurationSymbol;
                if(durationSymbol != null && durationSymbol == insertBeforeDS)
                {
                    noteObjects.Insert(i, invisibleClefChangeSymbol);
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
        /// Returns the clefType current at msPosition.
        /// </summary>
        private string GetClefType(int msPosition, SortedDictionary<int, string> clefTypesPerMsPos)
        {
            Debug.Assert(clefTypesPerMsPos.Count > 0 && clefTypesPerMsPos.ContainsKey(0));

            string clefType = null;
            if(clefTypesPerMsPos.Count == 1)
            {
                clefType = clefTypesPerMsPos[0]; 
            }
            else
            {
                List<int> keys = new List<int>(clefTypesPerMsPos.Keys);
                if(msPosition >= keys[keys.Count - 1])
                {
                    clefType = clefTypesPerMsPos[keys[keys.Count - 1]];
                }
                else
                {
                    for(int i = 1; i < clefTypesPerMsPos.Count; ++i)
                    {
                        if(msPosition >= keys[i - 1] && msPosition < keys[i])
                        {
                            clefType = clefTypesPerMsPos[keys[i - 1]];
                            break;
                        }
                    }
                }
            }
            
            return clefType;
        }

        private void InsertInUniqueMidiDurationDefs(List<UniqueClefChangeDef> uClefChangeDefs, List<IUniqueMidiDurationDef> uniqueMidiDurationDefs)
        {
            Debug.Assert(uniqueMidiDurationDefs.Count > 1);
            foreach(UniqueClefChangeDef uClefChangeDef in uClefChangeDefs)
            {
                int msPosition = uClefChangeDef.MsPosition;
                int count = uniqueMidiDurationDefs.Count;

                if(msPosition > uniqueMidiDurationDefs[count - 1].MsPosition)
                {
                    uniqueMidiDurationDefs.Add(uClefChangeDef);
                }
                else if(msPosition <= uniqueMidiDurationDefs[0].MsPosition)
                {
                    uniqueMidiDurationDefs.Insert(0, uClefChangeDef);
                }
                else if(count > 1)
                {
                    for(int i = 1; i < count; ++i)
                    {
                        if((msPosition > uniqueMidiDurationDefs[i - 1].MsPosition)
                            && msPosition <= uniqueMidiDurationDefs[i].MsPosition)
                        {
                            uniqueMidiDurationDefs.Insert(i, uClefChangeDef);
                            break;
                        }
                    }
                }
            }
            for(int i = 1; i < uniqueMidiDurationDefs.Count; ++i)
            {
                UniqueClefChangeDef uccd1 = uniqueMidiDurationDefs[i - 1] as UniqueClefChangeDef;
                UniqueClefChangeDef uccd2 = uniqueMidiDurationDefs[i] as UniqueClefChangeDef;
                Debug.Assert((uccd1 == null || uccd2 == null) || uccd1.MsPosition < uccd2.MsPosition);
            }
        }

        /// <summary>
        /// The systems do not yet contain Metrics info.
        /// They are given Metrics and justified horizontally and vertically (internally) 
        /// inside system.RealizeGraphics().
        /// </summary>
        public void CreateMetricsAndJustifySystems(List<SvgSystem> systems)
        {
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
                        systems[sysIndex].MakeGraphics(graphics, sysIndex + 1, _pageFormat, leftMargin);
                    }
                }
            }
        }

        public List<int> GroupTopStaffIndices
        {
            get
            {
                List<int> groupTopStaffIndices = new List<int>();
                List<bool> barlineContinuesDownList = BarlineContinuesDownList;
                groupTopStaffIndices.Add(0);
                for(int index = 1; index < barlineContinuesDownList.Count; ++index)
                {
                    if(barlineContinuesDownList[index-1] == false)
                        groupTopStaffIndices.Add(index);
                }
                return groupTopStaffIndices;
            }
        }

        public List<int> GroupBottomStaffIndices
        {
            get
            {
                List<int> groupBottomStaffIndices = new List<int>();
                List<bool> barlineContinuesDownList = BarlineContinuesDownList;
                for(int index = 0; index < barlineContinuesDownList.Count; ++index)
                {
                    if(barlineContinuesDownList[index] == false)
                        groupBottomStaffIndices.Add(index);
                }
                return groupBottomStaffIndices;
            }
        }

        /// <summary>
        /// A list having one value per staff in the system
        /// </summary>
        public List<bool> BarlineContinuesDownList
        {
            get
            {
                List<bool> barlineContinuesDownPerStaff = new List<bool>();
                foreach(int nStaves in _pageFormat.StaffGroups)
                {
                    int remainingStavesInGroup = nStaves - 1;
                    while(remainingStavesInGroup > 0)
                    {
                        barlineContinuesDownPerStaff.Add(true);
                        --remainingStavesInGroup;
                    }
                    barlineContinuesDownPerStaff.Add(false);
                }
                return barlineContinuesDownPerStaff;
            }
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
                            Text staffName = drawObject as Text;
                            if(staffName != null)
                            {
                                Debug.Assert(staffName.TextInfo != null);
                                if(staffName.TextInfo.FontFamily == "Times New Roman") // staff name
                                {
                                    TextMetrics staffNameMetrics = new TextMetrics(graphics, null, staffName.TextInfo);
                                    float nameWidth = staffNameMetrics.Right - staffNameMetrics.Left;
                                    maxNameWidth = (maxNameWidth > nameWidth) ? maxNameWidth : nameWidth;
                                }
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
