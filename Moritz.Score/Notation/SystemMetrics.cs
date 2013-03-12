

namespace Moritz.Score.Notation
{
    internal class SystemMetrics : GroupMetrics
    {
        public SystemMetrics()
            : base("system")
        {

        }

        public override void Move(float dx, float dy)
        {
            base.Move(dx, dy);
            _stafflinesTop += dy;
            _stafflinesBottom += dy;
            _notesTop += dy;
            _notesBottom += dy;
        }

        public override void Add(Metrics metrics)
        {
            base.Add(metrics);
            StaffMetrics staffMetrics = metrics as StaffMetrics;
            if(staffMetrics != null)
                SetTopAndBottomMetrics(staffMetrics);
        }

        private void SetTopAndBottomMetrics(StaffMetrics staffMetrics)
        {
            _notesTop =
                _notesTop < staffMetrics.Top ? _notesTop : staffMetrics.Top;

            _stafflinesTop =
                _stafflinesTop < staffMetrics.StafflinesTop ? _stafflinesTop : staffMetrics.StafflinesTop;

            _stafflinesBottom =
                _stafflinesBottom > staffMetrics.StafflinesBottom ? _stafflinesBottom : staffMetrics.StafflinesBottom;

            _notesBottom =
                _notesBottom > staffMetrics.Bottom ? _notesBottom : staffMetrics.Bottom;
        }


        public float StafflinesTop { get { return _stafflinesTop; } }
        private float _stafflinesTop = float.MaxValue;

        public float StafflinesBottom { get { return _stafflinesBottom; } set { _stafflinesBottom = value; } }
        private float _stafflinesBottom = float.MinValue;

        public float NotesTop { get { return _notesTop; } set { _notesTop = value; } }
        private float _notesTop = 0F;
        public float NotesBottom { get { return _notesBottom; } set { _notesBottom = value; } }
        private float _notesBottom = 0F;
    }

    internal class StaffMetrics : GroupMetrics
    {
        public StaffMetrics(float left, float right, float height)
            : base("staff")
        {
            _top = 0F;
            _right = right;
            _bottom = height;
            _left = left;

            _stafflinesTop = _top;
            _stafflinesBottom = _bottom;
            _stafflinesLeft = _left;
        }

        public override void Move(float dx, float dy)
        {
            base.Move(dx, dy);
            _stafflinesTop += dy;
            _stafflinesBottom += dy;
        }

        public void ResetMetrics()
        {
            ResetBoundary();
        }

        public float StafflinesTop { get { return _stafflinesTop; } }
        private float _stafflinesTop = 0F;

        public float StafflinesBottom { get { return _stafflinesBottom; } }
        private float _stafflinesBottom = 0F;

        public float StafflinesLeft { get { return _stafflinesLeft; } }
        private float _stafflinesLeft = 0F;
    }
}
