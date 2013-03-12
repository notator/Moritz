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
                SvgSystem system = new SvgSystem(svgScore);
                svgScore.Systems.Add(system);

                for(int i = 0; i < _pageFormat.StafflinesPerStaff.Count; i++)
                {
                    string staffname = null;
                    if(svgScore.Systems.Count == 1)
                    {
                        staffname = _pageFormat.LongStaffNames[i];
                    }
                    else
                    {
                        staffname = _pageFormat.ShortStaffNames[i];                        
                    }
                    Staff staff = new Staff(system, staffname, _pageFormat.StafflinesPerStaff[i], gap);
                    system.Staves.Add(staff);
                }


                for(int staffIndex = 0; staffIndex < system.Staves.Count; ++staffIndex)
                {
                    List<byte> midiChannels = _pageFormat.MidiChannelsPerVoicePerStaff[staffIndex];
                    for(int mIndex = 0; mIndex < midiChannels.Count; ++mIndex )
                    {
                        foreach(Voice voice in systemVoices)
                        {
                            if(voice.MidiChannel == midiChannels[mIndex])
                            {
                                Debug.Assert(voice.Staff == null);
                                voice.Staff = system.Staves[staffIndex];
                                if(midiChannels.Count == 1)
                                    voice.StemDirection = VerticalDir.none;
                                else if(mIndex == 0)
                                    voice.StemDirection = VerticalDir.up;
                                else
                                    voice.StemDirection = VerticalDir.down;
                                system.Staves[staffIndex].Voices.Add(voice);
                                break;
                            }
                        }
                    }
                }
            }
        }
        public void AddSymbolsToSystems(List<SvgSystem> systems)
        {
            byte[] currentChannelVelocities = new byte[systems[0].Staves.Count];
            int minimumQuaverDuration = _pageFormat.MinimumCrotchetDuration / 2; 
            for(int systemIndex = 0; systemIndex < systems.Count; ++systemIndex)
            {
                SvgSystem system = systems[systemIndex];
                for(int staffIndex = 0; staffIndex < system.Staves.Count; ++staffIndex)
                {
                    Staff staff = system.Staves[staffIndex];
                    foreach(Voice voice in staff.Voices)
                    {
                        voice.NoteObjects.Add(new ClefSign(voice, _pageFormat.ClefsList[staffIndex], _pageFormat.MusicFontHeight));
                    }
                    for(int voiceIndex = 0; voiceIndex < staff.Voices.Count; ++voiceIndex)
                    {
                        Voice voice = staff.Voices[voiceIndex];
                        bool firstLmdd = true;
                        foreach(LocalizedMidiDurationDef lmdd in voice.LocalizedMidiDurationDefs)
                        {
                            DurationSymbol durationSymbol =
                                SymbolSet.GetDurationSymbol(voice, lmdd, firstLmdd, ref currentChannelVelocities[staffIndex]);

                            voice.NoteObjects.Add(durationSymbol);
                            firstLmdd = false;
                        }
                    }
                    if(staff.Voices.Count == 2)
                    {
                        StandardSymbolSet standardSymbolSet = SymbolSet as StandardSymbolSet;
                        if(standardSymbolSet != null)
                            standardSymbolSet.ForceNaturalsInSynchronousChords(staff);
                    }
                }
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
