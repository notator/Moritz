using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;

using Krystals4ObjectLibrary;
using Moritz.Spec;
using Moritz.Symbols;
using Moritz.Palettes;
using Moritz.Algorithm;
using Moritz.Algorithm.PaletteDemo;
using Moritz.Algorithm.Study2c3_1;
using Moritz.Algorithm.SongSix;
using Moritz.Algorithm.Study3Sketch1;
using Moritz.Algorithm.Study3Sketch2;
using Moritz.Xml;

namespace Moritz.Composer
{
    public partial class ComposableSvgScore : SvgScore
    {
        /// <summary>
        /// This function is called twice per score. First with the krystals and palettes arguments set to null, so that certain
        /// algorithm properties can be retrieved, then with the arguments all set, so that the algorithm's DoAlgorithm() function
        /// can be called.
        /// The 'title' argument is the root name of an .mkss file containing the settings for a particular score of its algorithm.
        /// The 'title' argument will be printed as the title of the score on its first page.
        /// The score will be created in the same folder as its .mkss file.
        /// The score's folder (which can have any name) should be a subfolder of the assistant performer's 'scores' folder. This
        /// ensures that the CLicht font loads correctly.
        /// 
        /// Note that
        ///     1. Different scores that use the same algorithm can have the same title if they are created in different folders.
        ///     2. Two scores can't have the same title but different algorithms.
        ///     3. If there is more than one .mkss file in the same folder, they will all appear in the Assistant Composer's
        ///        'scores' pop-up menu.
        /// 
        /// To add a new score and/or algorithm, simply add a new case to the switch below.
        /// 
        /// </summary>
        public static CompositionAlgorithm Algorithm(string title, List<Krystal> krystals, List<Palette> palettes)
        {
            CompositionAlgorithm algorithm = null;
            switch(title)
            {
                case "Study 2c3.1":
                    algorithm = new Study2c3_1Algorithm(krystals, palettes);
                    break;
                case "Song Six":
                    algorithm = new SongSixAlgorithm(krystals, palettes);
                    break;
                case "Study 3 sketch 1":
                    algorithm = new Study3Sketch1Algorithm(krystals, palettes);
                    break;
                case "Study 3 sketch 2":
                    algorithm = new Study3Sketch2Algorithm(krystals, palettes);
                    break;
                case "Study 3":
                    algorithm = new Study3Sketch2Algorithm(krystals, palettes);
                    break;
                case "paletteDemo":
                    algorithm = new PaletteDemoAlgorithm(palettes);
                    break;
                default:
                    MessageBox.Show("Error in ComposableScoreAlgorithm.cs:\n\n" +
                                    "Score title not found in switch in ComposableSvgScore.Algorithm(...).\n" +
                                    "(Add a new case statement.)",
                                    "Program Error");
                    break;
            }
            return algorithm;
        }

    }
}

