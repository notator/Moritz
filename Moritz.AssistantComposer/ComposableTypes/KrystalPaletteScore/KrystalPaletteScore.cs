using System.Collections.Generic;

using Krystals4ObjectLibrary;
using Moritz.Score;
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

