using Moritz.Xml;

namespace Moritz.Symbols
{
    internal class StaffMetrics : GroupMetrics
    {
        public StaffMetrics(float left, float right, float height, bool isInput)
            : base(isInput ? CSSObjectClass.inputStaff : CSSObjectClass.staff)
        {
            _top = 0F;
            _right = right;
            _bottom = height;
            _left = left;

            _stafflinesTop = _top;
            _stafflinesBottom = _bottom;
            _stafflinesLeft = _left;
            _stafflinesRight = _right;
        }

        public override void Move(float dx, float dy)
        {
            base.Move(dx, dy);
            _stafflinesTop += dy;
            _stafflinesBottom += dy;
        }

		public override void ResetBoundary()
		{
			base.ResetBoundary();
			foreach(Metrics metrics in MetricsList)
			{
				BarlineMetrics barlineMetrics = metrics as BarlineMetrics;
				if(barlineMetrics != null)
				{
					expandMetrics(barlineMetrics.BarnumberMetrics);
					expandMetrics(barlineMetrics.StaffNameMetrics);
				}
			}
		}

		private void expandMetrics(Metrics metrics)
		{
			if(metrics != null)
			{
				_top = _top < metrics.Top ? _top : metrics.Top;
				_right = _right > metrics.Right ? _right : metrics.Right;
				_bottom = _bottom > metrics.Bottom ? _bottom : metrics.Bottom;
				_left = _left < metrics.Left ? _left : metrics.Left;
			}
		}

        public float StafflinesTop { get { return _stafflinesTop; } }
        private float _stafflinesTop = 0F;

        public float StafflinesBottom { get { return _stafflinesBottom; } }
        private float _stafflinesBottom = 0F;

        public float StafflinesLeft { get { return _stafflinesLeft; } }
        private float _stafflinesLeft = 0F;

        public float StafflinesRight { get { return _stafflinesRight; } }
        private float _stafflinesRight = 0F;
    }
}
