using System.Collections.Generic;

using Krystals4ObjectLibrary;
using Moritz.Palettes;
using Moritz.Symbols;

namespace Moritz.Composer
{
    public class KrystalPaletteScore : ComposableSvgScore
    {
        public KrystalPaletteScore(string scoreTitleName, string algorithmName, PageFormat pageFormat,
            List<Krystal> krystals, List<Palette> palettes, string folder,
            string keywords, string comment)
            : base(folder, scoreTitleName, keywords, comment, pageFormat)
        {
            _krystals = krystals;
            
            _algorithm = Algorithm(algorithmName, krystals, palettes);

            Notator = new Notator(pageFormat);

            bool success = CreateScore();

            if(success == false)
            {
                this.Systems.Clear();
            }
        }

        private List<Krystal> _krystals = null;
    }
}

