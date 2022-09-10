using Moritz.Xml;

using System.Diagnostics;
using System.Text;

namespace Moritz.Symbols
{
    /// <summary>
    /// DurationSymbols are NoteObjects which have a logical width (and symbolize a duration)
    /// </summary>
    public abstract class DurationSymbol : AnchorageSymbol
    {
        /// <summary>
        /// Used by the Assistant Composer
        /// </summary>
        public DurationSymbol(Voice voice, int msDuration, int absMsPosition, int minimumCrotchetDuration, float fontHeight)
            : base(voice, fontHeight)
        {
            _msDuration = msDuration;
            _absMsPosition = absMsPosition;
            this.SetDurationClass(MsDuration, minimumCrotchetDuration);
        }

        /// <summary>
        /// The duration class is DurationClass.cautionary if the duration is zero
        /// The duration class is DurationClass.breve if the duration is >= (minimumCrotchetDuration * 8).
        /// The minimumCrotchetDuration will usually be set to something like 1200ms.
        /// </summary>
        private void SetDurationClass(int msDuration, int minimumCrotchetDuration)
        {
            //_msDuration = durationMS;
            _minimumCrotchetDuration = minimumCrotchetDuration;
            if(msDuration == 0)
                _durationClass = DurationClass.cautionary;
            else if(msDuration < (_minimumCrotchetDuration / 16))
                _durationClass = DurationClass.fiveFlags;
            else if(msDuration < (_minimumCrotchetDuration / 8))
                _durationClass = DurationClass.fourFlags;
            else if(msDuration < (_minimumCrotchetDuration / 4))
                _durationClass = DurationClass.threeFlags;
            else if(msDuration < (_minimumCrotchetDuration / 2))
                _durationClass = DurationClass.semiquaver;
            else if(msDuration < _minimumCrotchetDuration)
                _durationClass = DurationClass.quaver;
            else if(msDuration < (_minimumCrotchetDuration * 2))
                _durationClass = DurationClass.crotchet;
            else if(msDuration < (_minimumCrotchetDuration * 4))
                _durationClass = DurationClass.minim;
            else if(msDuration < (_minimumCrotchetDuration * 8))
                _durationClass = DurationClass.semibreve;
            else _durationClass = DurationClass.breve;
        }

        //public virtual void WriteSVG(SvgWriter w, int msPos) {}

        protected string InfoString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("staff=" + Voice.Staff.Staffname + " ");
                sb.Append("absMsPos=" + this.AbsMsPosition.ToString() + " ");
                sb.Append("msDur=" + this.MsDuration.ToString());
                return sb.ToString();
            }
        }

        /// <summary>
        /// The position from the beginning of the piece.
        /// </summary>
        public int AbsMsPosition
        {
            get { return _absMsPosition; }
            set { _absMsPosition = value; }
        }
        protected int _absMsPosition = 0;

        public virtual int MsDuration
        {
            get { return _msDuration; }
            set { Debug.Assert(false, "This property should only be set when agglomerating RestSymbols."); }
        }
        protected int _msDuration = 0;

        // these fields are readonly
        public DurationClass DurationClass { get { return _durationClass; } }
        public int MinimumCrotchetDuration { get { return _minimumCrotchetDuration; } }

        protected DurationClass _durationClass = DurationClass.none;
        private int _minimumCrotchetDuration;
    }
}
