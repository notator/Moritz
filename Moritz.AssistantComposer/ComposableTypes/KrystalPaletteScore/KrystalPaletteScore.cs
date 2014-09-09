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
            List<Krystal> krystals, List<Palette> palettes, string folder,
            string keywords, string comment)
            : base(folder, scoreTitleName, keywords, comment, pageFormat)
        {
            _krystals = krystals;
            
            _midiAlgorithm = Algorithm(algorithmName, krystals, palettes);

            Notator = new Notator(pageFormat);

            CreateScore();
        }

        private List<Krystal> _krystals = null;
    }
}

