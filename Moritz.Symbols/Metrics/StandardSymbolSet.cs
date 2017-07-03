using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

using Moritz.Midi;
using Moritz.Xml;
using Moritz.Spec;
using Moritz.Globals;
using System;

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
        public override void WriteSymbolDefinitions(SvgWriter w, float musicFontHeight, float cautionaryMusicFontHeight)
        {
            // treble clefs
            WriteClefSymbolDef(w, true, false); // treble, normal
            WriteClefSymbolDef(w, true, true); // treble, cautionary
            WriteClef8SymbolDef(w, true, false, musicFontHeight); // treble, normal
            WriteClef8SymbolDef(w, true, true, cautionaryMusicFontHeight); // treble, cautionary
            WriteClefMulti8SymbolDef(w, true, false, 2, musicFontHeight); // treble, normal
            WriteClefMulti8SymbolDef(w, true, true, 2, cautionaryMusicFontHeight);
            WriteClefMulti8SymbolDef(w, true, false, 3, musicFontHeight); // treble, normal
            WriteClefMulti8SymbolDef(w, true, true, 3, cautionaryMusicFontHeight); // treble, cautionary
            // bass clefs
            WriteClefSymbolDef(w, false, false); // bass, normal
            WriteClefSymbolDef(w, false, true); // bass, cautionary
            WriteClef8SymbolDef(w, false, false, musicFontHeight); // bass, normal
            WriteClef8SymbolDef(w, false, true, cautionaryMusicFontHeight); // bass, cautionary
            WriteClefMulti8SymbolDef(w, false, false, 2, musicFontHeight); // bass, normal
            WriteClefMulti8SymbolDef(w, false, true, 2, cautionaryMusicFontHeight); // bass, cautionary
            WriteClefMulti8SymbolDef(w, false, false, 3, musicFontHeight); // bass, normal
            WriteClefMulti8SymbolDef(w, false, true, 3, cautionaryMusicFontHeight); // bass, cautionary

            for(int i = 1; i < 6; i++)
            {
                WriteRightFlagBlock(w, i, musicFontHeight);
                WriteLeftFlagBlock(w, i, musicFontHeight);
            }
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
        private void WriteClefSymbolDef(SvgWriter w, bool isTreble, bool isCautionary)
        {
            w.WriteStartElement("g");
            if(isCautionary)
            {
                if(isTreble)
                {
                    w.WriteAttributeString("id", "smallTrebleClef");
                    WriteTextElement(w, "smallClef", "&");
                }
                else
                {
                    w.WriteAttributeString("id", "smallBassClef");
                    WriteTextElement(w, "smallClef", "?");
                }
            }
            else
            {
                if(isTreble)
                {
                    w.WriteAttributeString("id", "trebleClef");
                    WriteTextElement(w, "clef", "&");
                }
                else
                {
                    w.WriteAttributeString("id", "bassClef");
                    WriteTextElement(w, "clef", "?");
                }
            }
            w.WriteEndElement(); // g
        }
        private void WriteClef8SymbolDef(SvgWriter w, bool isTreble, bool isCautionary, float fontHeight)
        {
            float x = isTreble ? (0.28F * fontHeight) : (0.16F * fontHeight);
            float y = isTreble ? (-1.17F * fontHeight) : (1.1F * fontHeight);

            w.WriteStartElement("g");
            if(isCautionary)
            {
                if(isTreble)
                {
                    w.WriteAttributeString("id", "smallTrebleClef8");
                    WriteTextElement(w, "smallClef", "&");
                    WriteTextElement(w, "smallClefOctaveNumber", x, y, "•");
                }
                else
                {
                    y *= 1.2F;
                    w.WriteAttributeString("id", "smallBassClef8");
                    WriteTextElement(w, "smallClef", "?");
                    WriteTextElement(w, "smallClefOctaveNumber", x, y, "•");
                }
            }
            else
            {
                if(isTreble)
                {
                    w.WriteAttributeString("id", "trebleClef8");
                    WriteTextElement(w, "clef", "&");
                    WriteTextElement(w, "clefOctaveNumber", x, y, "•");
                }
                else
                {
                    w.WriteAttributeString("id", "bassClef8");
                    WriteTextElement(w, "clef", "?");
                    WriteTextElement(w, "clefOctaveNumber", x, y, "•");
                }
            }           
            w.WriteEndElement(); // g
        }
        private void WriteClefMulti8SymbolDef(SvgWriter w, bool isTreble, bool isCautionary, int octaveShift, float fontHeight)
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
            if(isCautionary)
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

        private void WriteRightFlagBlock(SvgWriter w, int nFlags, float fontHeight)
        {
            w.WriteFlagBlock(nFlags, true, fontHeight);
        }
        private void WriteLeftFlagBlock(SvgWriter w, int nFlags, float fontHeight)
        {
            w.WriteFlagBlock(nFlags, false, fontHeight);
        }
        #endregion symbol definitions

        public override Metrics NoteObjectMetrics(Graphics graphics, NoteObject noteObject, VerticalDir voiceStemDirection, float gap, float strokeWidth)
        {
            bool isInput = (noteObject.Voice is InputVoice);

            Metrics returnMetrics = null;
            SmallClef smallClef = noteObject as SmallClef;
            Clef clef = noteObject as Clef;
            Barline barline = noteObject as Barline;
            CautionaryChordSymbol cautionaryChordSymbol = noteObject as CautionaryChordSymbol;
            ChordSymbol chordSymbol = noteObject as ChordSymbol;
            InputChordSymbol inputChordSymbol = noteObject as InputChordSymbol;
            RestSymbol rest = noteObject as RestSymbol;
            if(barline != null)
            {
                returnMetrics = new BarlineMetrics(graphics, barline, gap);
            }
            else if(smallClef != null)
            {
                if(smallClef.ClefType != "n")
                {
                    CSSClass cssClass = isInput ? CSSClass.inputClef : CSSClass.clef;
                    ClefID smallClefID = GetSmallClefID(clef, isInput);
                    returnMetrics = new SmallClefMetrics(clef, gap, cssClass, smallClefID);
                }
            }
            else if(clef != null)
            {
                if(clef.ClefType != "n")
                {
                    CSSClass cssClass = isInput ? CSSClass.inputClef : CSSClass.clef;
                    ClefID clefID = GetClefID(clef, isInput);
                    returnMetrics = new ClefMetrics(clef, gap, cssClass, clefID);
                }
            }
            else if(cautionaryChordSymbol != null)
            {
                returnMetrics = new ChordMetrics(graphics, cautionaryChordSymbol, voiceStemDirection, gap, strokeWidth, CSSClass.cautionaryChord);
            }
            else if(inputChordSymbol != null)
            {
                returnMetrics = new ChordMetrics(graphics, inputChordSymbol, voiceStemDirection, gap, strokeWidth, CSSClass.inputChord);
            }
            else if(chordSymbol != null)
            {
                returnMetrics = new ChordMetrics(graphics, chordSymbol, voiceStemDirection, gap, strokeWidth, CSSClass.chord);
            }
            else if(rest != null)
            {
                CSSClass restClass = GetRestClass(rest);
                // All rests are originally created on the centre line.
                // They are moved vertically later, if they are on a 2-Voice staff.
                returnMetrics = new RestMetrics(graphics, rest, gap, noteObject.Voice.Staff.NumberOfStafflines, strokeWidth, restClass);
            }

            return returnMetrics;
        }

        private ClefID GetSmallClefID(Clef clef, bool isInput)
        {
            ClefID clefID = ClefID.none;

            switch(clef.ClefType)
            {
                case "t":
                    clefID = isInput ? ClefID.inputSmallTrebleClef : ClefID.smallTrebleClef;
                    break;
                case "t1": // trebleClef8
                    clefID = isInput ? ClefID.inputSmallTrebleClef8 : ClefID.smallTrebleClef8;
                    break;
                case "t2": // trebleClef2x8
                    clefID = isInput ? ClefID.inputSmallTrebleClef2x8 : ClefID.smallTrebleClef2x8;
                    break;
                case "t3": // trebleClef3x8
                    clefID = isInput ? ClefID.inputSmallTrebleClef3x8 : ClefID.smallTrebleClef3x8;
                    break;
                case "b":
                    clefID = isInput ? ClefID.inputSmallBassClef : ClefID.smallBassClef;
                    break;
                case "b1": // bassClef8
                    clefID = isInput ? ClefID.inputSmallBassClef8 : ClefID.smallBassClef8;
                    break;
                case "b2": // bassClef2x8
                    clefID = isInput ? ClefID.inputSmallBassClef2x8 : ClefID.smallBassClef2x8;
                    break;
                case "b3": // bassClef3x8
                    clefID = isInput ? ClefID.inputSmallBassClef3x8 : ClefID.smallBassClef3x8;
                    break;
                default:
                    Debug.Assert(false, "Unknown clef type.");
                    break;
            }

            return clefID;
        }

        private ClefID GetClefID(Clef clef, bool isInput)
        {
            ClefID clefID = ClefID.none;

            switch(clef.ClefType)
            {
                case "t":
                    clefID = isInput ? ClefID.inputTrebleClef : ClefID.trebleClef;
                    break;
                case "t1": // trebleClef8
                    clefID = isInput ? ClefID.inputTrebleClef8 : ClefID.trebleClef8;
                    break;
                case "t2": // trebleClef2x8
                    clefID = isInput ? ClefID.inputTrebleClef2x8 : ClefID.trebleClef2x8;
                    break;
                case "t3": // trebleClef3x8
                    clefID = isInput ? ClefID.inputTrebleClef3x8 : ClefID.trebleClef3x8;
                    break;
                case "b":
                    clefID = isInput ? ClefID.inputBassClef : ClefID.bassClef;
                    break;
                case "b1": // bassClef8
                    clefID = isInput ? ClefID.inputBassClef8 : ClefID.bassClef8;
                    break;
                case "b2": // bassClef2x8
                    clefID = isInput ? ClefID.inputBassClef2x8 : ClefID.bassClef2x8;
                    break;
                case "b3": // bassClef3x8
                    clefID = isInput ? ClefID.inputBassClef3x8 : ClefID.bassClef3x8;
                    break;
                default:
                    Debug.Assert(false, "Unknown clef type.");
                    break;
            }

            return clefID;
        }

        private CSSClass GetRestClass(RestSymbol rest)
        {
            CSSClass restClass = CSSClass.rest; // OutputChordSymbol
            if(rest is InputRestSymbol)
            {
                restClass = CSSClass.inputRest;
            }
            return restClass;
        }

        public override NoteObject GetNoteObject(Voice voice, int absMsPosition, IUniqueDef iud, bool firstDefInVoice,
            ref byte currentVelocity, float musicFontHeight)
        {
            NoteObject noteObject = null;
            CautionaryChordDef cautionaryChordDef = iud as CautionaryChordDef;
            MidiChordDef midiChordDef = iud as MidiChordDef;
            MidiRestDef midiRestDef = iud as MidiRestDef;
            InputChordDef inputChordDef = iud as InputChordDef;
            InputRestDef inputRestDef = iud as InputRestDef;
            ClefDef clefDef = iud as ClefDef;

            PageFormat pageFormat = voice.Staff.SVGSystem.Score.PageFormat;
            float cautionaryFontHeight = pageFormat.MusicFontHeight * pageFormat.SmallFactor;
            int minimumCrotchetDuration = pageFormat.MinimumCrotchetDuration;
 
            if(cautionaryChordDef != null && firstDefInVoice)
            {
                CautionaryChordSymbol cautionaryChordSymbol = new CautionaryChordSymbol(voice, cautionaryChordDef, absMsPosition, cautionaryFontHeight);
                noteObject = cautionaryChordSymbol;
            }                
            else if(midiChordDef != null)
            {
                OutputChordSymbol outputChordSymbol = new OutputChordSymbol(voice, midiChordDef, absMsPosition, minimumCrotchetDuration, musicFontHeight);

                if(this._coloredVelocities == true)
                {
                    outputChordSymbol.SetNoteheadColors();
                }
                else if(midiChordDef.NotatedMidiVelocities[0] != currentVelocity)
                {
                    outputChordSymbol.AddDynamic(midiChordDef.NotatedMidiVelocities[0], currentVelocity);
                    currentVelocity = midiChordDef.NotatedMidiVelocities[0];
                }
                noteObject = outputChordSymbol;
            }
            else if(midiRestDef != null || cautionaryChordDef != null)
            {
                OutputRestSymbol outputRestSymbol = new OutputRestSymbol(voice, iud, absMsPosition, minimumCrotchetDuration, musicFontHeight);
                noteObject = outputRestSymbol;
            }
            else if(inputChordDef != null)
            {
                InputChordSymbol inputChordSymbol = new InputChordSymbol(voice, inputChordDef, absMsPosition, minimumCrotchetDuration, musicFontHeight);
                noteObject = inputChordSymbol;
            }
            else if(inputRestDef != null)
            {
                InputRestSymbol inputRestSymbol = new InputRestSymbol(voice, inputRestDef, absMsPosition, minimumCrotchetDuration, musicFontHeight);
                noteObject = inputRestSymbol;
            }
            else if(clefDef != null)
            {
                SmallClef smallClef = new SmallClef(voice, clefDef.ClefType, absMsPosition, cautionaryFontHeight);
                noteObject = smallClef;
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
                if(!(staff is HiddenOutputStaff))
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
                if(!(staff is HiddenOutputStaff))
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
        }
        private List<float> GetCautionaryRightExtenderX2s(ChordSymbol cautionaryChordSymbol1,
            List<NoteObject> noteObjects, List<float> x1s, List<float> ys, float hairlinePadding)
        {
            List<float> x2s = new List<float>();
            NoteObject no2 = GetFollowingChordRestOrBarlineSymbol(noteObjects);
            Barline barline = no2 as Barline;
            ChordSymbol chord2 = no2 as ChordSymbol;
            RestSymbol rest2 = no2 as RestSymbol;
            if(barline != null)
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
                    CautionaryChordSymbol followingCautionary = noteObject as CautionaryChordSymbol;
                    if(followingCautionary != null)
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
                    Barline barline = noteObjects[i - 1] as Barline;
                    if(barline != null)
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
                if(!(staff is HiddenOutputStaff))
                {
                    foreach(Voice voice in staff.Voices)
                    {
                        List<NoteObject> noteObjects = voice.NoteObjects;
                        int index = 0;
                        while(index < noteObjects.Count - 1)
                        {
                            // noteObjects.Count - 1 because index is immediately incremented when a continuing 
                            // chord or rest is found, and it should always be less than noteObjects.Count.
                            ChordSymbol chord1 = noteObjects[index] as ChordSymbol;
                            if(chord1 != null)
                            {
                                List<float> x1s = GetX1sFromChord1(chord1.ChordMetrics, hairlinePadding);
                                List<float> x2s = null;
                                List<float> ys = null;
                                ++index;
                                if(chord1.MsDurationToNextBarline != null)
                                {
                                    while(index < noteObjects.Count)
                                    {
                                        CautionaryChordSymbol cautionaryChordSymbol = noteObjects[index] as CautionaryChordSymbol;
                                        ChordSymbol chord2 = noteObjects[index] as ChordSymbol;
                                        RestSymbol rest2 = noteObjects[index] as RestSymbol;
                                        if(cautionaryChordSymbol != null)
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

                                        Barline barline = noteObjects[index - 1] as Barline;
                                        if(barline != null)
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
        }
        private void AddExtendersAtTheEndsOfStaves(List<Staff> staves, float rightMarginPos, float gap, float extenderStrokeWidth,
            float hairlinePadding, SvgSystem nextSystem)
        {
            for(int staffIndex = 0; staffIndex < staves.Count; ++staffIndex)
            {
                Staff staff = staves[staffIndex];
                if(!(staff is HiddenOutputStaff))
                {
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
                    NoteheadExtenderMetrics nem =
                        new NoteheadExtenderMetrics(x1s[i], x2s[i], ys[i], extenderStrokeWidth, headMetrics[i].ColorAttribute, gap, drawExtender);

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
                if(!(staff is HiddenOutputStaff))
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
                if(!(staff is HiddenOutputStaff))
                {
                    foreach(Voice voice in staff.Voices)
                    {
                        voice.RemoveBeamBlockBeams();
                    }
                }
            }
        }

        private readonly bool _coloredVelocities;
    }
}
