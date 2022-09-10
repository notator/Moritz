using Krystals5ObjectLibrary;

using Moritz.Algorithm;
using Moritz.Palettes;
using Moritz.Symbols;

using System.Collections.Generic;

namespace Moritz.Composer
{
    public class KrystalPaletteScore : ComposableScore
    {

        public KrystalPaletteScore(string scoreFolderName, CompositionAlgorithm algorithm, PageFormat pageFormat,
            List<Krystal> krystals, List<Palette> palettes, string folder,
            string keywords, string comment)
            : base(folder, scoreFolderName, algorithm, keywords, comment, pageFormat)
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

