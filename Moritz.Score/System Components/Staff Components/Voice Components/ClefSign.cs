
using Moritz.Score.Notation;

namespace Moritz.Score
{
	public class ClefSign : NoteObject
	{
        /// <summary>
        /// Creates a new clef, of the type described, belonging to the given voice.
        /// </summary>
        /// <param name="voice"></param>
        /// <param name="clefSign"></param>
        public ClefSign(Voice voice, string clefSign, float fontHeight)
            : base(voice)
        {
            ClefName = clefSign;
            _fontHeight = fontHeight;
            //CapellaColor = "000000"; -- default
        }

        /// <summary>
        /// Writes a clef to the SVG file.
        /// The Character metrics have been set in SvgSystem.Justify()
        /// </summary>
        public override void WriteSVG(SvgWriter w)
        {
            if(this.Metrics != null)
            {
                ClefMetrics m = Metrics as ClefMetrics;
                if(m != null)
                    w.SvgUseXY(null, m.ID_Type, m.OriginX, m.OriginY, m.FontHeight);
            }
        }

		public override string ToString()
		{
			return "ClefSign: " + _clef;
		}

		public string ClefName
		{
			get { return _clef; }
			set
			{
                _clef = value;
			}
		}
		public ColorString CapellaColor = new ColorString("000000");

		private string _clef;
	}
}
