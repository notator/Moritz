using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

using Moritz.Midi;
using Moritz.Xml;
using Moritz.Spec;

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
        public override void WriteSymbolDefinitions(SvgWriter w)
        {
            WriteTrebleClefSymbolDef(w);
            WriteTrebleClef8SymbolDef(w);
            WriteTrebleClefMulti8SymbolDef(w, 2);
            WriteTrebleClefMulti8SymbolDef(w, 3);
            WriteBassClefSymbolDef(w);
            WriteBassClef8SymbolDef(w);
            WriteBassClefMulti8SymbolDef(w, 2);
            WriteBassClefMulti8SymbolDef(w, 3);
            for(int i = 1; i < 6; i++)
            {
                WriteRightFlagBlock(w, i);
                WriteLeftFlagBlock(w, i);
            }
        }
        #region symbol definitions
        /// <summary>
        /// [g id="trebleClef"]
        ///   [text x="0" y="0" font-size="1px" font-family="CLicht"] &amp; [/text]
        /// [/g]
        /// </summary>
        private void WriteTrebleClefSymbolDef(SvgWriter svgw)
        {
            svgw.WriteStartElement("g");
            svgw.WriteAttributeString("id", "trebleClef");
            svgw.WriteStartElement("text");
            svgw.WriteAttributeString("x", "0");
            svgw.WriteAttributeString("y", "0");
            svgw.WriteAttributeString("font-size", "1px");
            svgw.WriteAttributeString("font-family", "CLicht");
            svgw.WriteString("&");
            svgw.WriteEndElement(); // text
            svgw.WriteEndElement(); // g
        }
        /// <summary>
        /// [g id="trebleClef8"]
        ///   [text x="0" y="0" font-size="1px" font-family="CLicht"] &amp; [/text]
        ///   [text x="0.28" y="-1.17" font-size="0.66667px" font-family="CLicht"]•[/text]
        /// [/g]
        /// </summary>
        private void WriteTrebleClef8SymbolDef(SvgWriter svgw)
        {
            svgw.WriteStartElement("g");
            svgw.WriteAttributeString("id", "trebleClef8");
            svgw.WriteStartElement("text");
            svgw.WriteAttributeString("x", "0");
            svgw.WriteAttributeString("y", "0");
            svgw.WriteAttributeString("font-size", "1px");
            svgw.WriteAttributeString("font-family", "CLicht");
            svgw.WriteString("&");
            svgw.WriteEndElement(); // text
            svgw.WriteStartElement("text");
            svgw.WriteAttributeString("x", "0.28");
            svgw.WriteAttributeString("y", "-1.17");
            svgw.WriteAttributeString("font-size", "0.67px");
            svgw.WriteAttributeString("font-family", "CLicht");
            svgw.WriteString("•");
            svgw.WriteEndElement(); // text
            svgw.WriteEndElement(); // g
        }
        /// <summary>
		/// (the actual numbers have changed -- see function below)
        /// [g id="trebleClef2x8"]
        ///     [text x="0" y="0" font-size="1px" font-family="CLicht"]&amp;[/text]
        ///     [text x="0.037" y="-1.17" font-size="0.67px" font-family="CLicht"]™[/text]
        ///     [text x="0.252" y="-1.17" font-size="0.4px" font-family="Arial"]x[/text]
        ///     [text x="0.441" y="-1.17" font-size="0.67px" font-family="CLicht"]•[/text]
        /// [/g]
        /// and
		/// [g id="trebleClef3x8"]
        ///     [text x="0" y="0" font-size="1px" font-family="CLicht"]&amp;[/text]
        ///     [text x="0.037" y="-1.17" font-size="0.67px" font-family="CLicht"]£[/text]
        ///     [text x="0.252" y="-1.17" font-size="0.4px" font-family="Arial"]x[/text]
        ///     [text x="0.441" y="-1.17" font-size="0.67px" font-family="CLicht"]•[/text]
        /// [/g]
        /// </summary>
        private void WriteTrebleClefMulti8SymbolDef(SvgWriter svgw, int octaveShift)
        {
            svgw.WriteStartElement("g");
            svgw.WriteAttributeString("id", "trebleClef" + octaveShift.ToString() + "x8");

            svgw.WriteStartElement("text");
            svgw.WriteAttributeString("x", "0");
            svgw.WriteAttributeString("y", "0");
            svgw.WriteAttributeString("font-size", "1px");
            svgw.WriteAttributeString("font-family", "CLicht");
            svgw.WriteString("&");
            svgw.WriteEndElement(); // text

            svgw.WriteStartElement("text");
            svgw.WriteAttributeString("x", "0.036");
            svgw.WriteAttributeString("y", "-1.17");
            svgw.WriteAttributeString("font-size", "0.67px");
            svgw.WriteAttributeString("font-family", "CLicht");
            switch(octaveShift)
            {
                case 2:
                    svgw.WriteString("™");
                    break;
                case 3:
                    svgw.WriteString("£");
                    break;
            }
            svgw.WriteEndElement(); // text

            svgw.WriteStartElement("text");
            svgw.WriteAttributeString("x", "0.252");
            svgw.WriteAttributeString("y", "-1.17");
            svgw.WriteAttributeString("font-size", "0.4px");
            svgw.WriteAttributeString("font-family", "Arial");
            svgw.WriteString("x");
            svgw.WriteEndElement(); // text

            svgw.WriteStartElement("text");
            svgw.WriteAttributeString("x", "0.48");
            svgw.WriteAttributeString("y", "-1.17");
            svgw.WriteAttributeString("font-size", "0.67px");
            svgw.WriteAttributeString("font-family", "CLicht");
            svgw.WriteString("•");
            svgw.WriteEndElement(); // text

            svgw.WriteEndElement(); // g
        }
        /// <summary>
        /// [g id="bassClef"]
        ///   [text x="0" y="0" font-size="1px" font-family="CLicht"]?[/text]
        /// [/g]
        /// </summary>
        private void WriteBassClefSymbolDef(SvgWriter svgw)
        {
            svgw.WriteStartElement("g");
            svgw.WriteAttributeString("id", "bassClef");
            svgw.WriteStartElement("text");
            svgw.WriteAttributeString("x", "0");
            svgw.WriteAttributeString("y", "0");
            svgw.WriteAttributeString("font-size", "1px");
            svgw.WriteAttributeString("font-family", "CLicht");
            svgw.WriteString("?");
            svgw.WriteEndElement(); // text
            svgw.WriteEndElement(); // g
        }
        /// <summary>
        /// [g id="bassClef8"]
        ///    [text x="0" y="0" font-size="1px" font-family="CLicht"]?[/text]
        ///    [text x="0.16" y="1.1" font-size="0.67px" font-family="CLicht"]•[/text]
        /// [/g]
        /// </summary>
        private void WriteBassClef8SymbolDef(SvgWriter svgw)
        {
            svgw.WriteStartElement("g");
            svgw.WriteAttributeString("id", "bassClef8");
            svgw.WriteStartElement("text");
            svgw.WriteAttributeString("x", "0");
            svgw.WriteAttributeString("y", "0");
            svgw.WriteAttributeString("font-size", "1px");
            svgw.WriteAttributeString("font-family", "CLicht");
            svgw.WriteString("?");
            svgw.WriteEndElement(); // text
            svgw.WriteStartElement("text");
            svgw.WriteAttributeString("x", "0.16");
            svgw.WriteAttributeString("y", "1.1");
            svgw.WriteAttributeString("font-size", "0.67px");
            svgw.WriteAttributeString("font-family", "CLicht");
            svgw.WriteString("•");
            svgw.WriteEndElement(); // text
            svgw.WriteEndElement(); // g
        }
        /// <summary>
		/// (The actual numbers have changed -- see function below.)
        /// [g id="bassClef2x8"]
        ///     [text x="0" y="0" font-size="1px" font-family="CLicht"]?[/text]
        ///     [text x="0" y="1.1" font-size="0.67px" font-family="CLicht"]™[/text]
        ///     [text x="0.215" y="1.1" font-size="0.4px" font-family="Arial"]x[/text]
        ///     [text x="0.404" y="1.1" font-size="0.67px" font-family="CLicht"]•[/text]
        /// [/g]
        /// and
        /// [g id="bassClef3x8"]
        ///     [text x="0" y="0" font-size="1px" font-family="CLicht"]?[/text]
        ///     [text x="0" y="1.1" font-size="0.67px" font-family="CLicht"]£[/text]
        ///     [text x="0.194" y="1.1" font-size="0.4px" font-family="Arial"]x[/text]
        ///     [text x="0.383" y="1.1" font-size="0.67px" font-family="CLicht"]•[/text]
        /// [/g]
        /// </summary>
        private void WriteBassClefMulti8SymbolDef(SvgWriter svgw, int octaveShift)
        {
            svgw.WriteStartElement("g");
            svgw.WriteAttributeString("id", "bassClef" + octaveShift.ToString() + "x8");

            svgw.WriteStartElement("text");
            svgw.WriteAttributeString("x", "0");
            svgw.WriteAttributeString("y", "0");
            svgw.WriteAttributeString("font-size", "1px");
            svgw.WriteAttributeString("font-family", "CLicht");
            svgw.WriteString("?");
            svgw.WriteEndElement(); // text

            svgw.WriteStartElement("text");
            svgw.WriteAttributeString("x", "0");
            svgw.WriteAttributeString("y", "1.1");
            svgw.WriteAttributeString("font-size", "0.67px");
            svgw.WriteAttributeString("font-family", "CLicht");
            switch(octaveShift)
            {
                case 2:
                    svgw.WriteString("™");
                    break;
                case 3:
                    svgw.WriteString("£");
                    break;
            }
            svgw.WriteEndElement(); // text

            svgw.WriteStartElement("text");
            svgw.WriteAttributeString("x", "0.215");
            svgw.WriteAttributeString("y", "1.1");
            svgw.WriteAttributeString("font-size", "0.4px");
            svgw.WriteAttributeString("font-family", "Arial");
            svgw.WriteString("x");
            svgw.WriteEndElement(); // text

            svgw.WriteStartElement("text");
            svgw.WriteAttributeString("x", "0.435");
            svgw.WriteAttributeString("y", "1.1");
            svgw.WriteAttributeString("font-size", "0.67px");
            svgw.WriteAttributeString("font-family", "CLicht");
            svgw.WriteString("•");
            svgw.WriteEndElement(); // text

            svgw.WriteEndElement(); // g
        }
        private void WriteRightFlagBlock(SvgWriter svgw, int nFlags)
        {
            svgw.WriteFlagBlock(nFlags, true);
        }
        private void WriteLeftFlagBlock(SvgWriter svgw, int nFlags)
        {
            svgw.WriteFlagBlock(nFlags, false);
        }
        #endregion symbol definitions

        public override Metrics NoteObjectMetrics(Graphics graphics, NoteObject noteObject, VerticalDir voiceStemDirection, float gap, float strokeWidth)
        {
            Metrics returnMetrics = null;
            Clef clef = noteObject as Clef;
            Barline barline = noteObject as Barline;
            CautionaryOutputChordSymbol cautionaryOutputChordSymbol = noteObject as CautionaryOutputChordSymbol;
            CautionaryInputChordSymbol cautionaryInputChordSymbol = noteObject as CautionaryInputChordSymbol;
            ChordSymbol chord = noteObject as ChordSymbol;
            RestSymbol rest = noteObject as RestSymbol;
            if(barline != null)
            {
                returnMetrics = new BarlineMetrics(graphics, barline, gap);
            }
            else if(clef != null)
            {
                if(clef.ClefType != "n")
                    returnMetrics = new ClefMetrics(clef, gap);                    
            }
            else if(cautionaryOutputChordSymbol != null)
            {
                returnMetrics = new ChordMetrics(graphics, cautionaryOutputChordSymbol, voiceStemDirection, gap, strokeWidth);
            }
            else if(cautionaryInputChordSymbol != null)
            {
                returnMetrics = new ChordMetrics(graphics, cautionaryInputChordSymbol, voiceStemDirection, gap, strokeWidth);
            }
            else if(chord != null)
            {
                returnMetrics = new ChordMetrics(graphics, chord, voiceStemDirection, gap, strokeWidth);
            }
            else if(rest != null)
            {
                // All rests are originally created on the centre line.
                // They are moved vertically later, if they are on a 2-Voice staff.
                returnMetrics = new RestMetrics(graphics, rest, gap, noteObject.Voice.Staff.NumberOfStafflines, strokeWidth);
            }

            return returnMetrics;
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
            float cautionaryFontHeight = pageFormat.CautionaryNoteheadsFontHeight;
            int minimumCrotchetDuration = pageFormat.MinimumCrotchetDuration;
 
            if(cautionaryChordDef != null && firstDefInVoice)
            {
                if(cautionaryChordDef.NotatedMidiVelocities != null)
                {
                    CautionaryOutputChordSymbol cautionaryOutputChordSymbol = new CautionaryOutputChordSymbol(voice, cautionaryChordDef, absMsPosition, cautionaryFontHeight);
                    noteObject = cautionaryOutputChordSymbol;
                }
                else
                {
                    CautionaryInputChordSymbol cautionaryInputChordSymbol = new CautionaryInputChordSymbol(voice, cautionaryChordDef, absMsPosition, cautionaryFontHeight);
                    noteObject = cautionaryInputChordSymbol;
                }
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
                        CautionaryInputChordSymbol cautionaryInputChordSymbol = null;
                        CautionaryOutputChordSymbol cautionaryOutputChordSymbol = null;
                        ChordSymbol firstChord = null;
                        RestSymbol firstRest = null;
                        for(int index = 0; index < noteObjects.Count; ++index)
                        {
                            if(firstClef == null)
                                firstClef = noteObjects[index] as Clef;
                            if(cautionaryInputChordSymbol == null)
                                cautionaryChordSymbol = noteObjects[index] as CautionaryInputChordSymbol;
                            if(cautionaryOutputChordSymbol == null)
                                cautionaryChordSymbol = noteObjects[index] as CautionaryOutputChordSymbol;
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
                && (noteObject is CautionaryOutputChordSymbol || noteObject is CautionaryInputChordSymbol))
                {
                    firstCautionaryChordSymbolFound = true;
                    continue;
                }

                if(firstCautionaryChordSymbolFound)
                {
                    CautionaryOutputChordSymbol followingOutputCautionary = noteObject as CautionaryOutputChordSymbol;
                    if(followingOutputCautionary != null)
                    {
                        followingOutputCautionary.Visible = false;
                        continue;
                    }
                    CautionaryInputChordSymbol followingInputCautionary = noteObject as CautionaryInputChordSymbol;
                    if(followingInputCautionary != null)
                    {
                        followingInputCautionary.Visible = false;
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
                                        CautionaryOutputChordSymbol cautionaryOutputChordSymbol = noteObjects[index] as CautionaryOutputChordSymbol;
                                        CautionaryInputChordSymbol cautionaryInputChordSymbol = noteObjects[index] as CautionaryInputChordSymbol;
                                        ChordSymbol chord2 = noteObjects[index] as ChordSymbol;
                                        RestSymbol rest2 = noteObjects[index] as RestSymbol;
                                        if(cautionaryOutputChordSymbol != null)
                                        {
                                            cautionaryOutputChordSymbol.Visible = false;
                                        }
                                        else if(cautionaryInputChordSymbol != null)
                                        {
                                            cautionaryInputChordSymbol.Visible = false;
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
                        CautionaryOutputChordSymbol cautionaryOutputChordsymbol = null;
                        CautionaryInputChordSymbol cautionaryInputChordsymbol = null;
                        for(int index = noteObjects.Count - 1; index >= 0; --index)
                        {
                            lastChord = noteObjects[index] as ChordSymbol;
                            lastRest = noteObjects[index] as RestSymbol;
                            cautionaryOutputChordsymbol = noteObjects[index] as CautionaryOutputChordSymbol;
                            cautionaryInputChordsymbol = noteObjects[index] as CautionaryInputChordSymbol;
                            if(cautionaryOutputChordsymbol != null)
                            {
                                cautionaryOutputChordsymbol.Visible = false;
                                // a CautionaryChordSymbol is a ChordSymbol, but we have not found a real one yet. 
                            }
                            else if(cautionaryInputChordsymbol != null)
                            {
                                cautionaryInputChordsymbol.Visible = false;
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
                if(noteObject is CautionaryOutputChordSymbol || noteObject is CautionaryInputChordSymbol)
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
                        new NoteheadExtenderMetrics(x1s[i], x2s[i], ys[i], headMetrics[i].ColorAttribute, extenderStrokeWidth, gap, drawExtender);

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
