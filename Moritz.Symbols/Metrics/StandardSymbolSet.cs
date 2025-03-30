using Moritz.Globals;
using Moritz.Spec;
using Moritz.Xml;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;

namespace Moritz.Symbols
{
    public class StandardSymbolSet : SymbolSet
    {
        public StandardSymbolSet(bool coloredVelocities)
            : base()
        {
            _coloredVelocities = coloredVelocities;
        }

        /// <summary>
        /// Writes this score's SVG defs element
        /// </summary>
        /// <param name="w"></param>
        public override void WriteSymbolDefinitions(SvgWriter w, PageFormat pageFormat)
        {
            WriteClefDefinitions(w, pageFormat);
            WriteFlagDefinitions(w, pageFormat);
        }

        private void WriteClefDefinitions(SvgWriter w, PageFormat pageFormat)
        {
            List<ClefID> uc = new List<ClefID>(ClefMetrics.UsedClefIDs) as List<ClefID>;

            float normalHeight = pageFormat.MusicFontHeight;
            float smallHeight = normalHeight * pageFormat.SmallSizeFactor;

            // treble clefs
            if(uc.Contains(ClefID.trebleClef))
                WriteClefSymbolDef(w, true, false, false); // treble, normal, output
            if(uc.Contains(ClefID.smallTrebleClef))
                WriteClefSymbolDef(w, true, true, false); // treble, small, output

            if(uc.Contains(ClefID.trebleClef8))
                WriteClef8SymbolDef(w, true, false, false, normalHeight); // treble, normal, output
            if(uc.Contains(ClefID.smallTrebleClef8))
                WriteClef8SymbolDef(w, true, true, false, smallHeight); // treble, small, output

            if(uc.Contains(ClefID.trebleClef2x8))
                WriteClefMulti8SymbolDef(w, true, false, false, 2, normalHeight); // treble, normal, output, 2
            if(uc.Contains(ClefID.smallTrebleClef2x8))
                WriteClefMulti8SymbolDef(w, true, true, false, 2, smallHeight); // treble, small, output, 2

            if(uc.Contains(ClefID.trebleClef3x8))
                WriteClefMulti8SymbolDef(w, true, false, false, 3, normalHeight); // treble, normal, output, 3
            if(uc.Contains(ClefID.smallTrebleClef3x8))
                WriteClefMulti8SymbolDef(w, true, true, false, 3, smallHeight); // treble, small, output, 3


            // bass clefs
            if(uc.Contains(ClefID.bassClef))
                WriteClefSymbolDef(w, false, false, false); // bass, normal, output
            if(uc.Contains(ClefID.smallBassClef))
                WriteClefSymbolDef(w, false, true, false); // bass, small, output

            if(uc.Contains(ClefID.bassClef8))
                WriteClef8SymbolDef(w, false, false, false, normalHeight); // bass, normal, output
            if(uc.Contains(ClefID.smallBassClef8))
                WriteClef8SymbolDef(w, false, true, false, smallHeight); // bass, small, output

            if(uc.Contains(ClefID.bassClef2x8))
                WriteClefMulti8SymbolDef(w, false, false, false, 2, normalHeight); // bass, normal, output, 2
            if(uc.Contains(ClefID.smallBassClef2x8))
                WriteClefMulti8SymbolDef(w, false, true, false, 2, smallHeight); // bass, small, output, 2

            if(uc.Contains(ClefID.bassClef3x8))
                WriteClefMulti8SymbolDef(w, false, false, false, 3, normalHeight); // bass, normal, output, 3
            if(uc.Contains(ClefID.smallBassClef3x8))
                WriteClefMulti8SymbolDef(w, false, true, false, 3, smallHeight); // bass, small, output, 3
        }
        #region symbol definitions

        #region clefs
        private void WriteTextElement(SvgWriter w, string className, string innerText)
        {
            w.WriteStartElement("text");
            w.WriteAttributeString("class", className);
            w.WriteString(innerText);
            w.WriteEndElement();
        }
        private void WriteTextElement(SvgWriter w, string className, float x, float y, string innerText)
        {
            w.WriteStartElement("text");
            w.WriteAttributeString("class", className);
            w.WriteAttributeString("x", M.FloatToShortString(x));
            w.WriteAttributeString("y", M.FloatToShortString(y));
            w.WriteString(innerText);
            w.WriteEndElement();
        }
        private void WriteClefSymbolDef(SvgWriter w, bool isTreble, bool isSmall, bool isInput)
        {
            w.WriteStartElement("g");
            if(isSmall)
            {
                if(isTreble)
                {
                    if(isInput)
                    {
                        w.WriteAttributeString("id", "inputSmallTrebleClef");
                        WriteTextElement(w, "inputSmallClef", "&");
                    }
                    else
                    {
                        w.WriteAttributeString("id", "smallTrebleClef");
                        WriteTextElement(w, "smallClef", "&");
                    }
                }
                else
                {
                    if(isInput)
                    {
                        w.WriteAttributeString("id", "inputSmallBassClef");
                        WriteTextElement(w, "inputSmallClef", "?");
                    }
                    else
                    {
                        w.WriteAttributeString("id", "smallBassClef");
                        WriteTextElement(w, "smallClef", "?");
                    }
                }
            }
            else
            {
                if(isTreble)
                {
                    if(isInput)
                    {
                        w.WriteAttributeString("id", "inputTrebleClef");
                        WriteTextElement(w, "inputClef", "&");
                    }
                    else
                    {
                        w.WriteAttributeString("id", "trebleClef");
                        WriteTextElement(w, "clef", "&");
                    }
                }
                else
                {
                    if(isInput)
                    {
                        w.WriteAttributeString("id", "inputBassClef");
                        WriteTextElement(w, "inputClef", "?");
                    }
                    else
                    {
                        w.WriteAttributeString("id", "bassClef");
                        WriteTextElement(w, "clef", "?");
                    }
                }
            }
            w.WriteEndElement(); // g
        }
        private void WriteClef8SymbolDef(SvgWriter w, bool isTreble, bool isSmall, bool isInput, float fontHeight)
        {
            float x = isTreble ? (0.28F * fontHeight) : (0.16F * fontHeight);
            float y = isTreble ? (-1.17F * fontHeight) : (1.1F * fontHeight);

            string mainID = "";
            string clefClass = "";
            string clefChar = "";
            string numberClass = "";
            string prefix = "";

            if(isSmall)
            {
                if(isInput)
                {
                    prefix = "inputSmall";
                }
                else
                {
                    prefix = "small";
                }
            }
            else
            {
                if(isInput)
                {
                    prefix = "input";
                }
                else
                {
                    clefClass = "clef";
                    numberClass = "clefOctaveNumber";
                    if(isTreble)
                    {
                        mainID = "trebleClef8";
                        clefChar = "&";
                    }
                    else
                    {
                        mainID = "bassClef8";
                        clefChar = "?";
                    }
                }

            }
            if(!String.IsNullOrEmpty(prefix))
            {
                clefClass = prefix + "Clef";
                numberClass = prefix + "ClefOctaveNumber";
                if(isTreble)
                {
                    mainID = prefix + "TrebleClef8";
                    clefChar = "&";
                }
                else
                {
                    mainID = prefix + "BassClef8";
                    clefChar = "?";
                }
            }

            if(String.Compare(mainID, "smallBassClef8") == 0 || String.Compare(mainID, "inputSmallBassClef8") == 0)
            {
                y *= 1.2F; // lower the number
            }

            w.WriteStartElement("g");
            w.WriteAttributeString("id", mainID);
            WriteTextElement(w, clefClass, clefChar);
            WriteTextElement(w, numberClass, x, y, "•");
            w.WriteEndElement(); // g
        }

        private void WriteClefMulti8SymbolDef(SvgWriter w, bool isTreble, bool isSmall, bool isInput, int octaveShift, float fontHeight)
        {
            float x1 = isTreble ? (0.036F * fontHeight) : 0;
            float x2 = isTreble ? (0.252F * fontHeight) : (0.215F * fontHeight);
            float x3 = isTreble ? (0.48F * fontHeight) : (0.435F * fontHeight);
            float y = isTreble ? (-1.17F * fontHeight) : (1.1F * fontHeight);

            string numberStr = (octaveShift == 2) ? "™" : "£";

            string id;
            string clefStr;
            string clefChar = isTreble ? "&" : "?";
            string clefOctaveNumberStr;
            string clefXStr;
            #region make strings
            StringBuilder idSB = new StringBuilder();
            if(isSmall)
            {
                if(isInput)
                {
                    clefStr = "inputSmallClef";
                    clefOctaveNumberStr = "inputSmallClefOctaveNumber";
                    clefXStr = "inputSmallClefX";
                    if(isTreble)
                    {
                        idSB.Append("inputSmallTrebleClef");
                    }
                    else
                    {
                        y *= 1.2F;
                        idSB.Append("inputSmallBassClef");
                    }
                }
                else
                {
                    clefStr = "smallClef";
                    clefOctaveNumberStr = "smallClefOctaveNumber";
                    clefXStr = "smallClefX";
                    if(isTreble)
                    {
                        idSB.Append("smallTrebleClef");
                    }
                    else
                    {
                        y *= 1.2F;
                        idSB.Append("smallBassClef");
                    }
                }
            }
            else
            {
                if(isInput)
                {
                    clefStr = "inputClef";
                    clefOctaveNumberStr = "inputClefOctaveNumber";
                    clefXStr = "inputClefX";
                    if(isTreble)
                    {
                        idSB.Append("inputTrebleClef");
                    }
                    else
                    {
                        idSB.Append("inputBassClef");
                    }
                }
                else
                {
                    clefStr = "clef";
                    clefOctaveNumberStr = "clefOctaveNumber";
                    clefXStr = "clefX";
                    if(isTreble)
                    {
                        idSB.Append("trebleClef");
                    }
                    else
                    {
                        idSB.Append("bassClef");
                    }
                }
            }
            idSB.Append(octaveShift);
            idSB.Append("x8");
            id = idSB.ToString();
            #endregion            

            w.WriteStartElement("g");
            w.WriteAttributeString("id", id);
            WriteTextElement(w, clefStr, clefChar);
            WriteTextElement(w, clefOctaveNumberStr, x1, y, numberStr);
            WriteTextElement(w, clefXStr, x2, y, "x");
            WriteTextElement(w, clefOctaveNumberStr, x3, y, "•");
            w.WriteEndElement(); // g
        }
        #endregion

        private void WriteFlagDefinitions(SvgWriter w, PageFormat pageFormat)
        {
            List<FlagID> usedIDs = new List<FlagID>(FlagsMetrics.UsedFlagIDs);
            List<string> usedFlagIDs = new List<string>();
            foreach(FlagID flagID in usedIDs)
            {
                usedFlagIDs.Add(flagID.ToString());
            }

            float normalHeight = pageFormat.MusicFontHeight;

            for(int i = 1; i < 6; i++)
            {
                WriteLeftFlagBlock(w, i, normalHeight, usedFlagIDs);
            }
            for(int i = 1; i < 6; i++)
            {
                WriteRightFlagBlock(w, i, normalHeight, usedFlagIDs);
            }
        }

        private void WriteRightFlagBlock(SvgWriter w, int nFlags, float fontHeight, List<string> usedFlagIDs)
        {
            StringBuilder id = GetFlagID(true, nFlags);
            if(usedFlagIDs.Contains(id.ToString()))
            {
                w.WriteFlagBlock(id, nFlags, true, fontHeight);
            }
        }

        private void WriteLeftFlagBlock(SvgWriter w, int nFlags, float fontHeight, List<string> usedFlagIDs)
        {
            StringBuilder id = GetFlagID(false, nFlags);
            if(usedFlagIDs.Contains(id.ToString()))
            {
                w.WriteFlagBlock(id, nFlags, false, fontHeight);
            }
        }

        private StringBuilder GetFlagID(bool isRight, int nFlags)
        {
            StringBuilder type = new StringBuilder();
            if(isRight)
                    type.Append("right");
                else
                    type.Append("left");
            type.Append(nFlags);
            if(nFlags == 1)
                type.Append("Flag");
            else
                type.Append("Flags");

            return type;
        }
        #endregion symbol definitions

        public override Metrics NoteObjectMetrics(Graphics graphics, NoteObject noteObject, VerticalDir voiceStemDirection, float gap, PageFormat pageFormat)
        {
            float strokeWidth = pageFormat.StafflineStemStrokeWidth;

            Metrics returnMetrics = null;
            SmallClef smallClef = noteObject as SmallClef;
            Clef clef = noteObject as Clef;
            Barline barline = noteObject as Barline;
            CautionaryChordSymbol cautionaryChordSymbol = noteObject as CautionaryChordSymbol;
            ChordSymbol chordSymbol = noteObject as ChordSymbol;
            RestSymbol rest = noteObject as RestSymbol;
            if(barline != null)
            {
                barline.CreateMetrics(graphics);
                returnMetrics = barline.Barline_LineMetrics;
            }
            else if(smallClef != null)
            {
                if(smallClef.ClefType != "n")
                {
                    if(smallClef.IsVisible)
                    {
                        CSSObjectClass cssClass = CSSObjectClass.smallClef;
                        ClefID smallClefID = GetSmallClefID(clef);
                        returnMetrics = new SmallClefMetrics(clef, gap, cssClass, smallClefID);
                    }
                    else
                    {
                        returnMetrics = null;
                    }
                }
            }
            else if(clef != null)
            {
                if(clef.ClefType != "n")
                {
                    ClefID clefID = GetClefID(clef);
                    returnMetrics = new ClefMetrics(clef, gap, CSSObjectClass.clef, clefID);
                }
            }
            else if(cautionaryChordSymbol != null)
            {
                returnMetrics = new ChordMetrics(graphics, cautionaryChordSymbol, voiceStemDirection, gap, strokeWidth, CSSObjectClass.cautionaryChord);
            }
            else if(chordSymbol != null)
            {
                returnMetrics = new ChordMetrics(graphics, chordSymbol, voiceStemDirection, gap, strokeWidth, CSSObjectClass.chord);
            }
            else if(rest != null)
            {
                // All rests are originally created on the centre line.
                // They are moved vertically later, if they are on a 2-Voice staff.
                returnMetrics = new RestMetrics(graphics, rest, gap, noteObject.Voice.Staff.NumberOfStafflines, strokeWidth, CSSObjectClass.rest);
            }

            return returnMetrics;
        }

        private ClefID GetSmallClefID(Clef clef)
        {
            ClefID clefID = ClefID.none;

            switch(clef.ClefType)
            {
                case "t":
                    clefID = ClefID.smallTrebleClef;
                    break;
                case "t1": // trebleClef8
                    clefID = ClefID.smallTrebleClef8;
                    break;
                case "t2": // trebleClef2x8
                    clefID = ClefID.smallTrebleClef2x8;
                    break;
                case "t3": // trebleClef3x8
                    clefID = ClefID.smallTrebleClef3x8;
                    break;
                case "b":
                    clefID = ClefID.smallBassClef;
                    break;
                case "b1": // bassClef8
                    clefID = ClefID.smallBassClef8;
                    break;
                case "b2": // bassClef2x8
                    clefID = ClefID.smallBassClef2x8;
                    break;
                case "b3": // bassClef3x8
                    clefID = ClefID.smallBassClef3x8;
                    break;
                default:
                    Debug.Assert(false, "Unknown clef type.");
                    break;
            }

            return clefID;
        }

        private ClefID GetClefID(Clef clef)
        {
            ClefID clefID = ClefID.none;

            switch(clef.ClefType)
            {
                case "t":
                    clefID = ClefID.trebleClef;
                    break;
                case "t1": // trebleClef8
                    clefID = ClefID.trebleClef8;
                    break;
                case "t2": // trebleClef2x8
                    clefID = ClefID.trebleClef2x8;
                    break;
                case "t3": // trebleClef3x8
                    clefID = ClefID.trebleClef3x8;
                    break;
                case "b":
                    clefID = ClefID.bassClef;
                    break;
                case "b1": // bassClef8
                    clefID = ClefID.bassClef8;
                    break;
                case "b2": // bassClef2x8
                    clefID = ClefID.bassClef2x8;
                    break;
                case "b3": // bassClef3x8
                    clefID = ClefID.bassClef3x8;
                    break;
                default:
                    Debug.Assert(false, "Unknown clef type.");
                    break;
            }

            return clefID;
        }

        public override NoteObject GetNoteObject(Voice voice, int absMsPosition, IUniqueDef iud, int iudIndex,
            ref byte currentVelocity, PageFormat pageFormat)
        {
            NoteObject noteObject = null;
            CautionaryChordDef cautionaryChordDef = iud as CautionaryChordDef;
            RestDef midiRestDef = iud as RestDef;

            if(cautionaryChordDef != null && iudIndex == 1)
            {
                CautionaryChordSymbol cautionaryChordSymbol = new CautionaryChordSymbol(voice, cautionaryChordDef, absMsPosition, pageFormat);
                noteObject = cautionaryChordSymbol;
            }
            else if(iud is MidiChordDef midiChordDef)
            {
                ChordSymbol chordSymbol = new ChordSymbol(voice, midiChordDef, absMsPosition, pageFormat);

                if(this._coloredVelocities == true)
                {
                    chordSymbol.SetNoteheadColorClasses();
                }
                else if(midiChordDef.Velocities[0] != currentVelocity)
                {
                    chordSymbol.AddDynamic(midiChordDef.Velocities[0], currentVelocity);
                    currentVelocity = midiChordDef.Velocities[0];
                }
                noteObject = chordSymbol;
            }
            else if(midiRestDef != null || cautionaryChordDef != null)
            {
                RestSymbol restSymbol = new RestSymbol(voice, iud, absMsPosition, pageFormat);
                noteObject = restSymbol;
            }
            else if(iud is ClefDef clefDef)
            {
                if(iudIndex == 0)
                {
                    Clef clef = new Clef(voice, clefDef.ClefType, pageFormat.MusicFontHeight);
                    noteObject = clef;
                }
                else
                {
                    SmallClef smallClef = new SmallClef(voice, clefDef.ClefType, absMsPosition, pageFormat);
                    noteObject = smallClef;
                }
            }

            return noteObject;
        }

        public void ForceNaturalsInSynchronousChords(Staff staff)
        {
            Debug.Assert(staff.Voices.Count == 2);
            foreach(ChordSymbol voice0chord in staff.Voices[0].ChordSymbols)
            {
                foreach(ChordSymbol voice1chord in staff.Voices[1].ChordSymbols)
                {
                    if(voice0chord.AbsMsPosition == voice1chord.AbsMsPosition)
                    {
                        ForceNaturals(voice0chord, voice1chord);
                        break;
                    }
                    if(voice0chord.AbsMsPosition < voice1chord.AbsMsPosition)
                        break;
                }
            }
        }

        /// <summary>
        /// Force the display of naturals where the synchronous chords share a diatonic pitch,
        /// and one of them is not natural.
        /// </summary>
        private void ForceNaturals(ChordSymbol synchChord1, ChordSymbol synchChord2)
        {
            Debug.Assert(synchChord1.AbsMsPosition == synchChord2.AbsMsPosition);
            foreach(Head head1 in synchChord1.HeadsTopDown)
            {
                foreach(Head head2 in synchChord2.HeadsTopDown)
                {
                    if(head1.Pitch == head2.Pitch)
                    {
                        if(head1.Alteration != 0)
                            head2.DisplayAccidental = DisplayAccidental.force;
                        if(head2.Alteration != 0)
                            head1.DisplayAccidental = DisplayAccidental.force;
                        break;
                    }
                }
            }
        }

        public override void AdjustRestsVertically(List<Staff> staves)
        {
            foreach(Staff staff in staves)
            {
                if(staff.Voices.Count == 2)
                {
                    staff.AdjustRestsVertically();
                }
            }
        }

        /// <summary>
        /// 20.01.2012 N.B. Neither this function nor ChordMetrics.MoveAuxilliariesToLyricHeight()
        /// have been thoroughly tested yet. 
        /// This function should align the lyrics in each voice, moving ornaments and dynamics 
        /// which are on the same side of the staff. (Lyrics are closest to the staff.)
        /// </summary>
        public override void AlignLyrics(List<Staff> staves)
        {
            foreach(Staff staff in staves)
            {
                for(int voiceIndex = 0; voiceIndex < staff.Voices.Count; ++voiceIndex)
                {
                    VerticalDir voiceStemDirection = VerticalDir.none;
                    if(staff.Voices.Count > 1)
                    {   // 2-Voice staff
                        if(voiceIndex == 0)
                            voiceStemDirection = VerticalDir.up; // top voice
                        else
                            voiceStemDirection = VerticalDir.down; // bottom voice
                    }

                    float lyricMaxTop = float.MinValue;
                    float lyricMinBottom = float.MaxValue;
                    foreach(ChordSymbol chordSymbol in staff.Voices[voiceIndex].ChordSymbols)
                    {
                        Metrics lyricMetrics = chordSymbol.ChordMetrics.LyricMetrics;
                        if(lyricMetrics != null)
                        {
                            lyricMaxTop = (lyricMaxTop > lyricMetrics.Top) ? lyricMaxTop : lyricMetrics.Top;
                            lyricMinBottom = (lyricMinBottom < lyricMetrics.Bottom) ? lyricMinBottom : lyricMetrics.Bottom;
                        }
                    }
                    if(lyricMaxTop != float.MinValue)
                    {   // the voice has lyrics
                        if(voiceStemDirection == VerticalDir.none || voiceStemDirection == VerticalDir.down)
                        {
                            // the lyrics are below the staff
                            float lyricMinTop = staff.Metrics.StafflinesBottom + (staff.SVGSystem.Score.PageFormat.Gap * 1.5F);
                            lyricMaxTop = lyricMaxTop > lyricMinTop ? lyricMaxTop : lyricMinTop;
                        }
                        foreach(ChordSymbol chordSymbol in staff.Voices[voiceIndex].ChordSymbols)
                        {
                            chordSymbol.ChordMetrics.MoveAuxilliariesToLyricHeight(voiceStemDirection, lyricMaxTop, lyricMinBottom);
                        }
                    }
                }
            }
        }

        public override void AddNoteheadExtenderLines(List<Staff> staves, float rightMarginPos, float gap, float extenderStrokeWidth, float hairlinePadding, SvgSystem nextSystem)
        {
            AddExtendersAtTheBeginningsofStaves(staves, rightMarginPos, gap, extenderStrokeWidth, hairlinePadding);
            AddExtendersInStaves(staves, extenderStrokeWidth, gap, hairlinePadding);
            AddExtendersAtTheEndsOfStaves(staves, rightMarginPos, gap, extenderStrokeWidth, hairlinePadding, nextSystem);
        }
        #region private to AddNoteheadExtenderLines()
        private void AddExtendersAtTheBeginningsofStaves(List<Staff> staves, float rightMarginPos, float gap, float extenderStrokeWidth, float hairlinePadding)
        {
            foreach(Staff staff in staves)
            {
                foreach(Voice voice in staff.Voices)
                {
                    List<NoteObject> noteObjects = voice.NoteObjects;
                    Clef firstClef = null;
                    ChordSymbol cautionaryChordSymbol = null;
                    CautionaryChordSymbol cautionaryOutputChordSymbol = null;
                    ChordSymbol firstChord = null;
                    RestSymbol firstRest = null;
                    for(int index = 0; index < noteObjects.Count; ++index)
                    {
                        if(firstClef == null)
                            firstClef = noteObjects[index] as Clef;
                        if(cautionaryOutputChordSymbol == null)
                            cautionaryChordSymbol = noteObjects[index] as CautionaryChordSymbol;
                        if(firstChord == null)
                            firstChord = noteObjects[index] as ChordSymbol;
                        if(firstRest == null)
                            firstRest = noteObjects[index] as RestSymbol;

                        if(firstClef != null
                        && (cautionaryChordSymbol != null || firstChord != null || firstRest != null))
                            break;
                    }

                    if(firstClef != null && cautionaryChordSymbol != null)
                    {
                        // create brackets
                        List<CautionaryBracketMetrics> cbMetrics = cautionaryChordSymbol.ChordMetrics.CautionaryBracketsMetrics;
                        Debug.Assert(cbMetrics.Count == 2);
                        Metrics clefMetrics = firstClef.Metrics;

                        // extender left of cautionary
                        List<float> ys = cautionaryChordSymbol.ChordMetrics.HeadsOriginYs;
                        List<float> x1s = GetEqualFloats(clefMetrics.Right - (hairlinePadding * 2), ys.Count);
                        List<float> x2s = GetEqualFloats(cbMetrics[0].Left, ys.Count);
                        for(int i = 0; i < x2s.Count; ++i)
                        {
                            if((x2s[i] - x1s[i]) < gap)
                            {
                                x1s[i] = x2s[i] - gap;
                            }
                        }
                        cautionaryChordSymbol.ChordMetrics.NoteheadExtendersMetricsBefore =
                            CreateExtenders(x1s, x2s, ys, cautionaryChordSymbol.ChordMetrics.HeadsMetrics, extenderStrokeWidth, gap, true);

                        // extender right of cautionary
                        x1s = GetEqualFloats(cbMetrics[1].Right, ys.Count);
                        x2s = GetCautionaryRightExtenderX2s(cautionaryChordSymbol, voice.NoteObjects, x1s, ys, hairlinePadding);
                        cautionaryChordSymbol.ChordMetrics.NoteheadExtendersMetrics =
                            CreateExtenders(x1s, x2s, ys, cautionaryChordSymbol.ChordMetrics.HeadsMetrics, extenderStrokeWidth, gap, true);
                    }
                }
            }
        }
        private List<float> GetCautionaryRightExtenderX2s(ChordSymbol cautionaryChordSymbol1,
            List<NoteObject> noteObjects, List<float> x1s, List<float> ys, float hairlinePadding)
        {
            List<float> x2s = new List<float>();
            NoteObject no2 = GetFollowingChordRestOrBarlineSymbol(noteObjects);
            ChordSymbol chord2 = no2 as ChordSymbol;
            RestSymbol rest2 = no2 as RestSymbol;
            if(no2 is Barline barline)
            {
                float x2 = barline.Metrics.OriginX;
                x2s = GetEqualFloats(x2, x1s.Count);
            }
            else if(chord2 != null)
            {
                x2s = GetX2sFromChord2(ys, chord2.ChordMetrics, hairlinePadding);
            }
            else if(rest2 != null)
            {
                float x2 = rest2.Metrics.Left - hairlinePadding;
                x2s = GetEqualFloats(x2, x1s.Count);
            }
            else // no2 == null
            {
                Debug.Assert(no2 == null);
                // This voice has no further chords or rests,
                // so draw extenders to the right margin.
                // extend to the right margin
                PageFormat pageFormat = cautionaryChordSymbol1.Voice.Staff.SVGSystem.Score.PageFormat;
                float rightMarginPos = pageFormat.RightMarginPos;
                float gap = pageFormat.Gap;
                x2s = GetEqualFloats(rightMarginPos + gap, ys.Count);
            }
            return x2s;
        }
        /// <summary>
        /// Returns the first chordSymbol or restSymbol after the first cautionaryChordSymbol.
        /// If there are cautionaryChordSymbols between the first and the returned chordSymbol or restSymbol, they are rendered invisible.
        /// If there is a barline immediately preceding the durationSymbol that would otherwise be returned, the barline is returned.
        /// Null is returned if no further chordSymbol or RestSymbol is found in the noteObjects.
        /// </summary>
        /// <param name="noteObjects"></param>
        /// <returns></returns>
        private NoteObject GetFollowingChordRestOrBarlineSymbol(List<NoteObject> noteObjects)
        {
            NoteObject noteObjectToReturn = null;
            bool firstCautionaryChordSymbolFound = false;
            for(int i = 0; i < noteObjects.Count; ++i)
            {
                NoteObject noteObject = noteObjects[i];
                if(firstCautionaryChordSymbolFound == false
                && (noteObject is CautionaryChordSymbol))
                {
                    firstCautionaryChordSymbolFound = true;
                    continue;
                }

                if(firstCautionaryChordSymbolFound)
                {
                    if(noteObject is CautionaryChordSymbol followingCautionary)
                    {
                        followingCautionary.Visible = false;
                        continue;
                    }

                    if(noteObject is ChordSymbol)
                        noteObjectToReturn = noteObject;

                    if(noteObject is RestSymbol)
                        noteObjectToReturn = noteObject;
                }

                if(noteObjectToReturn != null) // a ChordSymbol or a RestSymbol (not a CautionaryChordSymbol)
                {
                    if(noteObjects[i - 1] is Barline barline)
                        noteObjectToReturn = barline;
                    break;
                }
            }
            return noteObjectToReturn;
        }
        private void AddExtendersInStaves(List<Staff> staves, float extenderStrokeWidth, float gap, float hairlinePadding)
        {
            foreach(Staff staff in staves)
            {
                foreach(Voice voice in staff.Voices)
                {
                    List<NoteObject> noteObjects = voice.NoteObjects;
                    int index = 0;
                    while(index < noteObjects.Count - 1)
                    {
                        // noteObjects.Count - 1 because index is immediately incremented when a continuing 
                        // chord or rest is found, and it should always be less than noteObjects.Count.
                        if(noteObjects[index] is ChordSymbol chord1)
                        {
                            List<float> x1s = GetX1sFromChord1(chord1.ChordMetrics, hairlinePadding);
                            List<float> x2s = null;
                            List<float> ys = null;
                            ++index;
                            if(chord1.MsDurationToNextBarline != null)
                            {
                                while(index < noteObjects.Count)
                                {
                                    ChordSymbol chord2 = noteObjects[index] as ChordSymbol;
                                    RestSymbol rest2 = noteObjects[index] as RestSymbol;
                                    if(noteObjects[index] is CautionaryChordSymbol cautionaryChordSymbol)
                                    {
                                        cautionaryChordSymbol.Visible = false;
                                    }
                                    else if(chord2 != null)
                                    {
                                        ys = chord1.ChordMetrics.HeadsOriginYs;
                                        x2s = GetX2sFromChord2(ys, chord2.ChordMetrics, hairlinePadding);
                                        break;
                                    }
                                    else if(rest2 != null)
                                    {
                                        float x2 = rest2.Metrics.Left - hairlinePadding;
                                        ys = chord1.ChordMetrics.HeadsOriginYs;
                                        x2s = GetEqualFloats(x2, x1s.Count);
                                        break;
                                    }
                                    ++index;
                                }

                                if(x2s != null && ys != null)
                                {
                                    bool hasContinuingBeamBlock =
                                        ((chord1.BeamBlock != null) && (chord1.BeamBlock.Chords[chord1.BeamBlock.Chords.Count - 1] != chord1));
                                    if(hasContinuingBeamBlock)
                                        Debug.Assert(true);

                                    if(noteObjects[index - 1] is Barline barline)
                                    {
                                        float x2 = barline.Metrics.OriginX;
                                        x2s = GetEqualFloats(x2, x1s.Count);
                                    }
                                    bool drawExtender = false;
                                    if(chord1.DurationClass > DurationClass.semiquaver)
                                        drawExtender = true;
                                    if(chord1.DurationClass < DurationClass.crotchet && hasContinuingBeamBlock)
                                        drawExtender = false;

                                    chord1.ChordMetrics.NoteheadExtendersMetrics =
                                        CreateExtenders(x1s, x2s, ys, chord1.ChordMetrics.HeadsMetrics, extenderStrokeWidth, gap, drawExtender);
                                }
                            }
                        }
                        else
                        {
                            ++index;
                        }
                    }
                }
            }
        }
        private void AddExtendersAtTheEndsOfStaves(List<Staff> staves, float rightMarginPos, float gap, float extenderStrokeWidth,
            float hairlinePadding, SvgSystem nextSystem)
        {
            for(int staffIndex = 0; staffIndex < staves.Count; ++staffIndex)
            {
                Staff staff = staves[staffIndex];
                for(int voiceIndex = 0; voiceIndex < staff.Voices.Count; ++voiceIndex)
                {
                    Voice voice = staff.Voices[voiceIndex];
                    List<NoteObject> noteObjects = voice.NoteObjects;
                    ChordSymbol lastChord = null;
                    RestSymbol lastRest = null;
                    CautionaryChordSymbol cautionaryChordSymbol = null;
                    for(int index = noteObjects.Count - 1; index >= 0; --index)
                    {
                        lastChord = noteObjects[index] as ChordSymbol;
                        lastRest = noteObjects[index] as RestSymbol;
                        cautionaryChordSymbol = noteObjects[index] as CautionaryChordSymbol;
                        if(cautionaryChordSymbol != null)
                        {
                            cautionaryChordSymbol.Visible = false;
                            // a CautionaryChordSymbol is a ChordSymbol, but we have not found a real one yet. 
                        }
                        else if(lastChord != null || lastRest != null)
                            break;
                    }

                    if(lastChord != null && lastChord.MsDurationToNextBarline != null)
                    {
                        List<float> x1s = GetX1sFromChord1(lastChord.ChordMetrics, hairlinePadding);
                        List<float> x2s;
                        List<float> ys = lastChord.ChordMetrics.HeadsOriginYs;
                        if(nextSystem != null && FirstDurationSymbolOnNextSystemIsCautionary(nextSystem.Staves[staffIndex].Voices[voiceIndex]))
                        {
                            x2s = GetEqualFloats(rightMarginPos + gap, x1s.Count);
                        }
                        else
                        {
                            x2s = GetEqualFloats(rightMarginPos, x1s.Count);
                        }
                        lastChord.ChordMetrics.NoteheadExtendersMetrics =
                            CreateExtenders(x1s, x2s, ys, lastChord.ChordMetrics.HeadsMetrics, extenderStrokeWidth, gap, true);
                    }
                }
            }
        }

        private bool FirstDurationSymbolOnNextSystemIsCautionary(Voice voiceOnNextSystem)
        {
            bool firstDurationSymbolIsCautionary = false;
            foreach(NoteObject noteObject in voiceOnNextSystem.NoteObjects)
            {
                if(noteObject is CautionaryChordSymbol)
                {
                    firstDurationSymbolIsCautionary = true;
                    break;
                }
                else if(noteObject is ChordSymbol || noteObject is RestSymbol)
                {
                    break;
                }
            }
            return firstDurationSymbolIsCautionary;
        }
        /// <summary>
        /// Extenders are created for chords of all duration classes, but only displayed on crotchets or greater.
        /// This is so that extenders become part of the staff's edge, which is used when shifting staves and drawing barlines.
        /// Extenders shorter than a gap are not created.
        /// </summary>
        private List<NoteheadExtenderMetrics> CreateExtenders(List<float> x1s, List<float> x2s, List<float> ys, List<HeadMetrics> headMetrics, float extenderStrokeWidth, float gap, bool drawExtender)
        {
            Debug.Assert(ys.Count == x1s.Count);
            Debug.Assert(ys.Count == x2s.Count);
            Debug.Assert(ys.Count > 0);

            List<NoteheadExtenderMetrics> noteheadExtendersMetrics = new List<NoteheadExtenderMetrics>();
            for(int i = 0; i < ys.Count; ++i)
            {
                if((x2s[i] - x1s[i]) > (gap / 2))
                {
                    string strokeColorString = M.GetEnumDescription(headMetrics[i].CSSColorClass);

                    NoteheadExtenderMetrics nem =
                        new NoteheadExtenderMetrics(x1s[i], x2s[i], ys[i], extenderStrokeWidth, strokeColorString, gap, drawExtender);

                    noteheadExtendersMetrics.Add(nem);
                }
            }
            return noteheadExtendersMetrics;
        }
        private List<float> GetX1sFromChord1(ChordMetrics chord1Metrics, float hairlinePadding)
        {
            List<float> x1s = new List<float>();
            LedgerlineBlockMetrics upperLedgerlineMetrics = chord1Metrics.UpperLedgerlineBlockMetrics;
            LedgerlineBlockMetrics lowerLedgerlineMetrics = chord1Metrics.LowerLedgerlineBlockMetrics;
            List<HeadMetrics> headsMetrics = chord1Metrics.HeadsMetrics;
            Debug.Assert(headsMetrics.Count > 0);

            foreach(HeadMetrics headmetrics in headsMetrics)
            {
                float x1 = headmetrics.Right;
                if(upperLedgerlineMetrics != null
                && headmetrics.OriginY >= upperLedgerlineMetrics.Top && headmetrics.OriginY <= upperLedgerlineMetrics.Bottom)
                    x1 = upperLedgerlineMetrics.Right;
                if(lowerLedgerlineMetrics != null
                && headmetrics.OriginY >= lowerLedgerlineMetrics.Top && headmetrics.OriginY <= lowerLedgerlineMetrics.Bottom)
                    x1 = lowerLedgerlineMetrics.Right;

                x1s.Add(x1 + hairlinePadding);
            }
            return x1s;
        }
        private List<float> GetX2sFromChord2(List<float> ys, ChordMetrics chord2Metrics, float hairlinePadding)
        {
            List<float> x2s = new List<float>();
            LedgerlineBlockMetrics c2UpperLedgerlineMetrics = chord2Metrics.UpperLedgerlineBlockMetrics;
            LedgerlineBlockMetrics c2LowerLedgerlineMetrics = chord2Metrics.LowerLedgerlineBlockMetrics;
            List<HeadMetrics> c2headsMetrics = chord2Metrics.HeadsMetrics;
            Debug.Assert(c2headsMetrics.Count > 0);
            List<AccidentalMetrics> c2AccidentalsMetrics = chord2Metrics.TopDownAccidentalsMetrics;

            float verticalPadding = hairlinePadding * 4.0f;
            foreach(float y in ys)
            {
                float x2 = float.MaxValue;
                if(c2UpperLedgerlineMetrics != null)
                {
                    if(y >= (c2UpperLedgerlineMetrics.Top - verticalPadding)
                    && y <= (c2UpperLedgerlineMetrics.Bottom + verticalPadding))
                        x2 = x2 < c2UpperLedgerlineMetrics.Left ? x2 : c2UpperLedgerlineMetrics.Left;
                }
                if(c2LowerLedgerlineMetrics != null)
                {
                    if(y >= (c2LowerLedgerlineMetrics.Top - verticalPadding)
                    && y <= (c2LowerLedgerlineMetrics.Bottom + verticalPadding))
                        x2 = x2 < c2LowerLedgerlineMetrics.Left ? x2 : c2LowerLedgerlineMetrics.Left;
                }
                foreach(HeadMetrics headMetrics in c2headsMetrics)
                {
                    if(y >= (headMetrics.Top - verticalPadding)
                    && y <= (headMetrics.Bottom + verticalPadding))
                        x2 = x2 < headMetrics.Left ? x2 : headMetrics.Left;
                }
                foreach(AccidentalMetrics accidentalMetrics in c2AccidentalsMetrics)
                {
                    if(y >= (accidentalMetrics.Top - verticalPadding)
                    && y <= (accidentalMetrics.Bottom + verticalPadding))
                        x2 = x2 < accidentalMetrics.Left ? x2 : accidentalMetrics.Left;
                }
                x2 = x2 < float.MaxValue ? x2 : chord2Metrics.Left;
                x2s.Add(x2 - hairlinePadding);
            }

            float minX = float.MaxValue;
            foreach(float x in x2s)
            {
                minX = minX < x ? minX : x;
            }
            List<float> x2sMinimum = new List<float>();
            foreach(float x in x2s)
                x2sMinimum.Add(minX);

            return x2sMinimum;
        }
        private List<float> GetEqualFloats(float x2, int count)
        {
            List<float> x2s = new List<float>();
            for(int i = 0; i < count; ++i)
            {
                x2s.Add(x2);
            }
            return x2s;
        }
        #endregion

        public override void FinalizeBeamBlocks(List<Staff> staves)
        {
            foreach(Staff staff in staves)
            {
                foreach(Voice voice in staff.Voices)
                {
                    voice.FinalizeBeamBlocks();
                }
                if(staff.Voices.Count == 2)
                {
                    staff.AdjustStemAndBeamBlockHeights(0);
                    staff.AdjustStemAndBeamBlockHeights(1);
                }
            }
        }

        /// <summary>
        /// This function sets the lengths of beamed stems (including the positions of their attached dynamics etc.
        /// so that collision checking can be done as accurately as possible in JustifyHorizontally().
        /// It does this by calling FinalizeBeamBlocks(), which is called again after JustifyHorizontally(),
        /// and then deleting the beams that that function adds.
        /// At the time this function is called, chords are distributed proportionally to their duration, so the 
        /// beams which are constructed here are not exactly correct. The outer stem tips of each beam should, 
        /// however, be fairly close to their final positions.
        /// </summary>
        public override void SetBeamedStemLengths(List<Staff> staves)
        {
            FinalizeBeamBlocks(staves);
            foreach(Staff staff in staves)
            {
                foreach(Voice voice in staff.Voices)
                {
                    voice.RemoveBeamBlockBeams();
                }
            }
        }

        private readonly bool _coloredVelocities;
    }
}
