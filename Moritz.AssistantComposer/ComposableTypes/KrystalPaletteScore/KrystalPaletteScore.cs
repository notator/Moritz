using System;
using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Globals;
using Moritz.Krystals;
using Moritz.Score;
using Moritz.Score.Midi;
using Moritz.Score.Notation;

namespace Moritz.AssistantComposer
{
    public class KrystalPaletteScore : ComposableSvgScore
    {
        public KrystalPaletteScore(string scoreTitleName, string algorithmName, PageFormat pageFormat,
            List<Krystal> krystals, List<PaletteFormContent> palettes, string folder,
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

        private List<Krystal> _krystals = null;
    }
}

