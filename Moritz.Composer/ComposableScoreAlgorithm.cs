using Moritz.Algorithm;
using Moritz.Algorithm.ErratumMusical;
using Moritz.Algorithm.PianolaMusic;
using Moritz.Algorithm.Study1;
using Moritz.Algorithm.Study2;
using Moritz.Algorithm.Study3Sketch1;
using Moritz.Algorithm.Study3Sketch2;
using Moritz.Algorithm.Study4;
using Moritz.Algorithm.ThreeCrashes;
//using Moritz.Algorithm.SongSix;
using Moritz.Algorithm.Tombeau1;
using Moritz.Symbols;

using System.Windows.Forms;

namespace Moritz.Composer
{
    public partial class ComposableScore : SvgScore
    {
        /// <summary>
        /// The argument (title) will be printed as the title of the score on its first page.
        /// The title is the root name of an .mkss file containing the settings for a particular score of an algorithm.
        /// The .mkss file should be in a subfolder of the assistant performer's 'scores' folder. (This ensures that the CLicht
        /// font loads correctly.)
        /// The Assistant Composer creates the score in the same folder as its .mkss file. The folder can have any name.
        /// 
        /// Note that
        ///     1. Two scores can't have the same title but different algorithms.
        ///     2. Different scores that use the same algorithm can have the same title if they are created in different folders.
        ///        (Different .mkss files can have the same name if they are in different folders.)
        /// 
        /// To add a new score and/or algorithm, simply add a new case to the switch below.
        /// </summary>
        public static CompositionAlgorithm Algorithm(string title)
        {
            CompositionAlgorithm algorithm = null;
            switch(title)
            {
                case "Pianola Music":
                    algorithm = new PianolaMusicAlgorithm();
                    break;
                case "Study 1":
                    algorithm = new Study1Algorithm();
                    break;
                case "Study 2":
                    algorithm = new Study2Algorithm();
                    break;
                case "Study 3 sketch 1":
                    algorithm = new Study3Sketch1Algorithm();
                    break;
                case "Study 3 sketch 2":
                    algorithm = new Study3Sketch2Algorithm();
                    break;
                case "Study 4":
                    algorithm = new Study4Algorithm();
                    break;
                //case "Song Six":
                //	algorithm = new SongSixAlgorithm();
                //	break;
                case "Tombeau 1":
                    algorithm = new Tombeau1Algorithm();
                    break;
                case "Erratum Musical":
                    algorithm = new ErratumMusicalAlgorithm();
                    break;
                case "Three Crashes":
                    algorithm = new ThreeCrashesAlgorithm();
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
