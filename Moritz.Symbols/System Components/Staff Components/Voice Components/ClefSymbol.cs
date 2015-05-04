
using Moritz.Xml;

namespace Moritz.Symbols
{
	public class ClefSymbol : NoteObject
	{
        /// <summary>
        /// Creates a new clef, of the type described, belonging to the given voice.
        /// The clefType must be one of the following strings "t", "t1", "t2", "t3", "b", "b1", "b2", "b3"
        /// </summary>
        /// <param name="voice"></param>
        /// <param name="clefType"></param>
        public ClefSymbol(Voice voice, string clefType, float fontHeight)
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
                    w.SvgUseXY(null, m.ID_Type, m.OriginX, m.OriginY, m.FontHeight);
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
    /// A ClefChangeSymbol is a small clef symbol placed anywhere on a staff except at the beginning.
    /// </summary>
    public class ClefChangeSymbol : ClefSymbol
    {
        public ClefChangeSymbol(Voice voice, string clefType, float fontHeight, int msPosition)
            : base(voice, clefType, fontHeight)
        {
            _msPosition = msPosition;
            _isVisible = true;
        }

        public override string ToString()
        {
            return "ClefChangeSymbol: " + _clefType;
        }

        /// <summary>
        /// Writes this ClefChangeSymbol to the SVG file if both _isVisible is true.
        /// The character metrics have been set in SvgSystem.Justify()
        /// </summary>
        public override void WriteSVG(SvgWriter w, bool staffIsVisible)
        {
            if(_isVisible && staffIsVisible)
            {
				w.SvgStartGroup(null, "clefChange" + SvgScore.UniqueID_Number);
                base.WriteSVG(w, staffIsVisible);
                w.SvgEndGroup();
            }
        }

        public int MsPosition { get { return _msPosition; } }
        private int _msPosition;

        public bool IsVisible { get { return _isVisible; } set { _isVisible = value; } }
        private bool _isVisible;
    }
}
