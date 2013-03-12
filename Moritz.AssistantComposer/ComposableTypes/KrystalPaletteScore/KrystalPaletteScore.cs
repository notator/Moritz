using System;
using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;

using Moritz.Score;
using Moritz.Score.Midi;
using Moritz.Score.Notation;

namespace Moritz.AssistantComposer
{
    public class KrystalPaletteScore : ComposableSvgScore
    {
        public KrystalPaletteScore(string scoreTitleName, string algorithmName, PageFormat pageFormat,
            List<Krystal> krystals, List<Palette> palettes, string folder,
            string keywords, string comment)
            : base(folder, scoreTitleName, keywords, comment, pageFormat)
        {
            _krystals = krystals;
            _paletteDefs = null;
            if(palettes != null)
                _paletteDefs = GetPaletteDefs(palettes);
            
            _midiAlgorithm = Algorithm(algorithmName, krystals, _paletteDefs);

            Notator = new Notator(pageFormat);

            CreateScore();
        }

        public override void WriteMidiChordDefinitions(SvgWriter w)
        {
            if(_paletteDefs != null && _paletteDefs.Count > 1)
            {
                w.WriteStartElement("score", "midiDefs", null);
                foreach(PaletteDef paletteDef in _paletteDefs)
                {
                    foreach(MidiDurationDef midiDurationDef in paletteDef)
                    {
                        MidiChordDef midiChordDef = midiDurationDef as MidiChordDef;
                        MidiRestDef midiRestDef = midiDurationDef as MidiRestDef;
                        if(midiChordDef != null)
                        {
                            w.WriteStartElement("score", "midiChord", null);
                            midiChordDef.WriteSvg(w);
                            w.WriteEndElement();
                        }
                        else if(midiRestDef != null)
                        {
                            w.WriteStartElement("score", "midiRest", null);
                            if(!String.IsNullOrEmpty(midiRestDef.ID) && !midiRestDef.ID.Contains("localRest"))
                                w.WriteAttributeString("id", midiRestDef.ID); // the definition ID, not the local ID of a midiRest
                            w.WriteAttributeString("msDuration", midiRestDef.MsDuration.ToString());
                            w.WriteEndElement();
                        }
                    }
                }
                w.WriteEndElement(); // score:midiDefs
            }
        }

        private List<Krystal> _krystals = null;
    }
}

