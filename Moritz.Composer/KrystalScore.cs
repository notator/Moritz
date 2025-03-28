using Krystals5ObjectLibrary;

using Moritz.Algorithm;
using Moritz.Symbols;

using System.Collections.Generic;

namespace Moritz.Composer
{
    public class KrystalScore : ComposableScore
    {

        public KrystalScore(string scoreFolderName, CompositionAlgorithm algorithm, PageFormat pageFormat,
            List<Krystal> krystals, string folder, string keywords, string comment)
            : base(folder, scoreFolderName, algorithm, keywords, comment, pageFormat)
        {
            Notator = new Notator(pageFormat);

            bool success = CreateScore(krystals);

            if(success == false)
            {
                this.Systems.Clear();
            }
        }
    }
}

