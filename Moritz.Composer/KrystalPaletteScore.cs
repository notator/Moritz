using System.Collections.Generic;

using Krystals4ObjectLibrary;

using Moritz.Algorithm;
using Moritz.Palettes;
using Moritz.Symbols;

namespace Moritz.Composer
{
    public class KrystalPaletteScore : ComposableSvgScore
    {

        public KrystalPaletteScore(string scoreTitleName, CompositionAlgorithm algorithm, PageFormat pageFormat,
            List<Krystal> krystals, List<Palette> palettes, string folder,
            string keywords, string comment)
            : base(folder, scoreTitleName, algorithm, keywords, comment, pageFormat)
        {
            Notator = new Notator(pageFormat);

            bool success = CreateScore(krystals, palettes);



            if(success == false)

            {

                this.Systems.Clear();

            }
        }
    }
}

