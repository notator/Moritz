
using Moritz.Score.Notation;

namespace Moritz.Score
{
    public abstract class NoteObject
	{
        public NoteObject(Voice voice)
        {
            Voice = voice; // container
        }

        public NoteObject(Voice voice, float fontHeight)
        {
            Voice = voice; // container
            _fontHeight = fontHeight;
        }

        /// <summary>
        /// Writes objects to the SVG file (in front of the stafflines).
        /// The CharacterMetrics have been set by calling CreateMetrics() in SvgSystem.Justify()
        /// </summary>
        public abstract void WriteSVG(SvgWriter w);

        //protected string GetID(int pageNumber, int systemNumber, int staffNumber, string symbolType, int symbolNumber)
        //{
        //    return String.Format("{0}.{1}.{2} {3}{4}", pageNumber.ToString(), systemNumber.ToString(), staffNumber.ToString(), symbolType, symbolNumber.ToString());
        //}

        /// <summary>
        /// Contains the the object's absolute origin, boundary box and alignment positions (view box pixel units).
        /// This value is allocated by calling CreateMetrics() in System.Justify().
        /// The object is subsequently moved during Justification by calling Metrics.Move();
        /// </summary>
        public Metrics Metrics = null;
        /// <summary>
        /// The containing voice
        /// </summary>
        public Voice Voice = null;
        public float FontHeight { get { return _fontHeight; } set { _fontHeight = value; } }
        public float _fontHeight = 0F;
	}
}
