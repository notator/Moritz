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
		public GroupMetrics()
			: base()
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

		public virtual void ResetBoundary()
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
			WriteSVG(w, null);
		}

		public void WriteSVG(SvgWriter w, string id)
		{
			w.SvgStartGroup(null);
			foreach(Metrics metrics in MetricsList)
			{
				metrics.WriteSVG(w);
			}
			w.SvgEndGroup();
		}

		public readonly List<Metrics> MetricsList = new List<Metrics>();
	}
}
