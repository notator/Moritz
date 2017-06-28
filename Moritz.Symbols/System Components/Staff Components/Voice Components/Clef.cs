
using Moritz.Xml;

namespace Moritz.Symbols
{
	public class Clef : NoteObject
	{
        /// <summary>
        /// Creates a new clef, of the type described, belonging to the given voice.
        /// The clefType must be one of the following strings "t", "t1", "t2", "t3", "b", "b1", "b2", "b3"
        /// </summary>
        /// <param name="voice"></param>
        /// <param name="clefType"></param>
        public Clef(Voice voice, string clefType, float fontHeight)
            : base(voice)
        {
            _clefType = clefType;
            _fontHeight = fontHeight;
            //CapellaColor = "000000"; -- default
        }

        /// <summary>
        /// Writes a clef to the SVG file.
        /// The Character metrics have been set in SvgSystem.Justify()
        /// </summary>
        public override void WriteSVG(SvgWriter w, bool staffIsVisible)
        {
            if(this.Metrics != null && staffIsVisible)
            {
                ClefMetrics m = Metrics as ClefMetrics;
                if(m != null)
                {
                    m.WriteSVG(w);
                }
            }
        }

		public override string ToString()
		{
			return "Clef: " + _clefType;
		}

		public string ClefType
		{
			get { return _clefType; }
			set
			{
                _clefType = value;
			}
		}
		public ColorString CapellaColor = new ColorString("000000");

		protected string _clefType;
	}

    /// <summary>
    /// A SmallClef is a small clef symbol placed anywhere on a staff except at the beginning.
    /// </summary>
    public class SmallClef : Clef
    {
        public SmallClef(Voice voice, string clefType, int absMsPosition, float fontHeight)
            : base(voice, clefType, fontHeight)
        {
            _absMsPosition = absMsPosition;
            _isVisible = true;
        }

        public override string ToString()
        {
            return "SmallClef: " + _clefType;
        }

        /// <summary>
        /// Writes a clef to the SVG file.
        /// The Character metrics have been set in SvgSystem.Justify()
        /// </summary>
        public override void WriteSVG(SvgWriter w, bool staffIsVisible)
        {
            if(this.Metrics != null && _isVisible && staffIsVisible)
            {
                SmallClefMetrics m = Metrics as SmallClefMetrics;
                if(m != null)
                     w.SvgUseXY(CSSClass.clef, m.UseID, m.OriginX, m.OriginY);
            }
        }

        public int AbsMsPosition { get { return _absMsPosition; } }
        private int _absMsPosition;

        public bool IsVisible { get { return _isVisible; } set { _isVisible = value; } }
        private bool _isVisible;
    }
}
