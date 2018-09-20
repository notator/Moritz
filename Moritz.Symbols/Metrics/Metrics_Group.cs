using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using Moritz.Globals;
using Moritz.Xml;

namespace Moritz.Symbols
{
    /// <summary>
    /// The base class for just SystemMetrics and StaffMetrics
    /// </summary>
	public class GroupMetrics : Metrics
	{
        public GroupMetrics(CSSObjectClass cssGroupClass)
            : base(cssGroupClass)
        {
        }

        /// <summary>
        /// Adds the metrics to the MetricsList and includes it in this object's boundary.
        /// The boundary is used for collision checking. All objects that should move together with this object
        /// must be added to the MetricsList.
        /// </summary>
        /// <param name="metrics"></param>
        public virtual void Add(Metrics metrics)
		{
			MetricsList.Add(metrics);
			ResetBoundary();
		}

		public void ResetBoundary()
		{
			_top = float.MaxValue;
			_right = float.MinValue;
			_bottom = float.MinValue;
			_left = float.MaxValue;
			foreach(Metrics metrics in MetricsList)
			{
				_top = _top < metrics.Top ? _top : metrics.Top;
				_right = _right > metrics.Right ? _right : metrics.Right;
				_bottom = _bottom > metrics.Bottom ? _bottom : metrics.Bottom;
				_left = _left < metrics.Left ? _left : metrics.Left;
			}
		}

		public override void Move(float dx, float dy)
		{
			base.Move(dx, dy);
			foreach(Metrics metrics in MetricsList)
			{
				metrics.Move(dx, dy);
			}
		}

		public override void WriteSVG(SvgWriter w)
		{
			w.SvgStartGroup(CSSObjectClass.ToString());
			foreach(Metrics metrics in MetricsList)
			{
				metrics.WriteSVG(w);
			}
			w.SvgEndGroup();
		}

		public readonly List<Metrics> MetricsList = new List<Metrics>();
	}

	//public class Barline_DrawObjectsMetrics : GroupMetrics
	//{
	//	public Barline_DrawObjectsMetrics(Graphics graphics, NormalBarline barline, float strokeWidth, float gap)
	//		: base(CSSObjectClass.drawObjects)
	//	{
	//		_originX = 0F;
	//		_left = 0F;
	//		_right = 0F;

	//		if(graphics != null && barline != null)
	//		{
	//			float minimumBarlineTopToRegionInfoBottom = gap * 3F;
	//			float minimumBarlineTopToBarnumberBottom = gap * 6F;

	//			foreach(DrawObject drawObject in barline.DrawObjects)
	//			{
	//				if(drawObject is Text text)
	//				{
	//					Debug.Assert(text.TextInfo != null
	//					&& (text is StaffNameText || text is FramedBarNumberText));

	//					if(text is StaffNameText)
	//					{
	//						CSSObjectClass staffClass = (barline.Voice is InputVoice) ? CSSObjectClass.inputStaffName : CSSObjectClass.staffName;
	//						_staffNameMetrics = new TextMetrics(staffClass, graphics, text.TextInfo);
	//						// move the staffname vertically to the middle of this staff
	//						Staff staff = barline.Voice.Staff;
	//						float staffheight = staff.Gap * (staff.NumberOfStafflines - 1);
	//						float dy = (staffheight * 0.5F) + (gap * 0.8F);
	//						_staffNameMetrics.Move(0F, dy);
	//					}
	//					else if(text is FramedBarNumberText)
	//					{
	//						_barnumberMetrics = new BarnumberMetrics(graphics, text.TextInfo, text.FrameInfo);
	//						// move the bar number above this barline
	//						_barnumberMetrics.Move(0F, -minimumBarlineTopToBarnumberBottom);
	//					}
	//				}
	//				else if(drawObject is TextList textList)
	//				{
	//					Debug.Assert(textList is FramedRegionStartText || textList is FramedRegionEndText);

	//					if(textList is FramedRegionStartText framedRegionStartText)
	//					{
	//						_framedRegionStartTextMetrics = new FramedRegionInfoMetrics(graphics, framedRegionStartText.Texts, framedRegionStartText.FrameInfo);
	//						// move the regionInfo above this barline, and align its left edge with the barline
	//						FramedRegionInfoMetrics frstm = _framedRegionStartTextMetrics;
	//						frstm.Move(0, -minimumBarlineTopToRegionInfoBottom);
	//					}
	//					else if(textList is FramedRegionEndText framedRegionEndText)
	//					{
	//						_framedRegionEndTextMetrics = new FramedRegionInfoMetrics(graphics, framedRegionEndText.Texts, framedRegionEndText.FrameInfo);
	//						// move the regionInfo above this barline, and align its right edge with the barline
	//						FramedRegionInfoMetrics fretm = _framedRegionEndTextMetrics;
	//						fretm.Move(0, -minimumBarlineTopToRegionInfoBottom);
	//					}
	//				}
	//			}
	//		}
	//	}

	//	public override void Move(float dx, float dy)
	//	{
	//		base.Move(dx, dy);
	//		if(_staffControlsGroupMetrics != null)
	//			_staffControlsGroupMetrics.Move(dx, dy);
	//		if(_barnumberMetrics != null)
	//			_barnumberMetrics.Move(dx, dy);
	//		if(_staffNameMetrics != null)
	//			_staffNameMetrics.Move(dx, dy);
	//		if(this._framedRegionStartTextMetrics != null)
	//			_framedRegionStartTextMetrics.Move(dx, dy);
	//		if(this._framedRegionEndTextMetrics != null)
	//			_framedRegionEndTextMetrics.Move(dx, dy);
	//	}

	//	/// <summary>
	//	/// Writes any DrawObjects attached to the barline to the SVG file.
	//	/// </summary>
	//	public override void WriteSVG(SvgWriter w)
	//	{
	//		if(_staffNameMetrics != null)
	//		{
	//			_staffNameMetrics.WriteSVG(w);
	//		}
	//		if(_barnumberMetrics != null)
	//		{
	//			_barnumberMetrics.WriteSVG(w);
	//		}
	//		if(_framedRegionStartTextMetrics != null)
	//		{
	//			_framedRegionStartTextMetrics.WriteSVG(w);
	//		}
	//		if(_framedRegionEndTextMetrics != null)
	//		{
	//			_framedRegionEndTextMetrics.WriteSVG(w);
	//		}
	//	}

	//	private GroupMetrics _staffControlsGroupMetrics = null;
	//	internal FramedRegionInfoMetrics FramedRegionStartTextMetrics { get { return _framedRegionStartTextMetrics; } }
	//	private FramedRegionInfoMetrics _framedRegionStartTextMetrics = null;
	//	internal FramedRegionInfoMetrics FramedRegionEndTextMetrics { get { return _framedRegionEndTextMetrics; } }
	//	private FramedRegionInfoMetrics _framedRegionEndTextMetrics = null;

	//	internal BarnumberMetrics BarnumberMetrics { get { return _barnumberMetrics; } }
	//	private BarnumberMetrics _barnumberMetrics = null;
	//	internal TextMetrics StaffNameMetrics { get { return _staffNameMetrics; } }
	//	private TextMetrics _staffNameMetrics = null;
	//}
}
