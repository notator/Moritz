using Moritz.Xml;

using System;

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

        public override void WriteSVG(SvgWriter w)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Writes a clef or smallClef to the SVG file.
        /// The Character metrics have been set in SvgSystem.Justify()
        /// </summary>
        public void WriteSVG(SvgWriter w, ClefID clefOrSmallClefID, float originX, float originY)
        {
            w.SvgUseXY(CSSObjectClass.clef, clefOrSmallClefID.ToString(), originX, originY);
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
        public SmallClef(Voice voice, string clefType, int absMsPosition, PageFormat pageFormat)
            : base(voice, clefType, pageFormat.MusicFontHeight * pageFormat.SmallSizeFactor)
        {
            _absMsPosition = absMsPosition;
            _isVisible = true;
        }

        public override string ToString() => "SmallClef: " + _clefType + " absMsPos=" + _absMsPosition;

        public int AbsMsPosition { get { return _absMsPosition; } }
        private readonly int _absMsPosition;

        public bool IsVisible { get { return _isVisible; } set { _isVisible = value; } }
        private bool _isVisible;
    }


    /// <summary>
    /// An InvisibleSmallClef is a small, invisible clef symbol in the lower voice on a staff,
    /// at an absMsPosition of an ordinary SmallClef in the top voice.
    /// </summary>
    public class InvisibleSmallClef : Clef
    {
        public InvisibleSmallClef(Voice voice, string clefType, int absMsPosition)
            : base(voice, clefType, 0.01F)
        {
            _absMsPosition = absMsPosition;
            _isVisible = true;
        }

        public override string ToString() => "InvisibleSmallClef: " + _clefType + " absMsPos=" + _absMsPosition;

        public int AbsMsPosition { get { return _absMsPosition; } }
        private readonly int _absMsPosition;

        public bool IsVisible { get { return _isVisible; } set { _isVisible = value; } }
        private bool _isVisible;
    }
}
