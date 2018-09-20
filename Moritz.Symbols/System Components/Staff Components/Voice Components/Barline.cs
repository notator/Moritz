
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Moritz.Globals;
using Moritz.Xml;

namespace Moritz.Symbols
{
	/// <summary>
	/// Barlines maintain their line and drawObjects metrics separately.
	/// The lines are drawn using implementations of an abstract function,
	/// The drawObjects are drawn by calling BarlineDrawObjectsMetrics.WriteSVG().  
	/// </summary>
	public abstract class Barline : AnchorageSymbol
	{
		protected Barline (Voice voice)
            : base(voice)
        {
		}


		/// <summary>
		/// This function should not be called.
		/// Call the other WriteSVG(...) function to write the barline's vertical line(s),
		/// and WriteDrawObjectsSVG(...) to write any DrawObjects. 
		/// </summary>
		public override void WriteSVG(SvgWriter w, bool staffIsVisible)
		{
			throw new ApplicationException();
		}

		public abstract void WriteSVG(SvgWriter w, float topStafflineY, float bottomStafflineY, bool isEndOfSystem);

		public abstract void CreateMetrics(Graphics graphics);

		protected void SetCommonMetrics(Graphics graphics, List<DrawObject> drawObjects)
		{
			foreach(DrawObject drawObject in DrawObjects)
			{
				if(drawObject is StaffNameText staffNameText)
				{
					CSSObjectClass staffClass = (Voice is InputVoice) ? CSSObjectClass.inputStaffName : CSSObjectClass.staffName;
					StaffNameMetrics = new StaffNameMetrics(staffClass, graphics, staffNameText.TextInfo);
					//// move the staffname vertically to the middle of this staff
					//Staff staff = Voice.Staff;
					//float staffheight = staff.Gap * (staff.NumberOfStafflines - 1);
					//float dy = (staffheight * 0.5F) + (gap * 0.8F);
					//StaffnameMetrics.Move(0F, dy);
				}
				if(drawObject is FramedBarNumberText framedBarNumberText)
				{
					BarnumberMetrics = new BarnumberMetrics(graphics, framedBarNumberText.TextInfo, framedBarNumberText.FrameInfo);
					//// move the bar number above this barline
					//_barnumberMetrics.Move(0F, -minimumBarlineTopToBarnumberBottom);
				}
			}
		}

		public abstract void AddMetricsToEdge(HorizontalEdge horizontalEdge);
		protected void AddBasicMetricsToEdge(HorizontalEdge horizontalEdge)
		{
			if(StaffNameMetrics != null)
			{
				horizontalEdge.Add(StaffNameMetrics);
			}
			if(BarnumberMetrics != null)
			{
				horizontalEdge.Add(BarnumberMetrics);
			}
		}

		protected void MoveBarnumberAboveRegionBox(BarnumberMetrics barnumberMetrics, FramedRegionInfoMetrics regionInfoMetrics)
		{
			if(barnumberMetrics != null && regionInfoMetrics != null)
			{
				float overlap = barnumberMetrics.Bottom - regionInfoMetrics.Top;
				float shift = 0F;
				if(overlap > 0)
				{
					shift = overlap + Gap;
				}
				else if(overlap > -Gap)
				{
					shift = barnumberMetrics.Bottom - (regionInfoMetrics.Top - Gap);
				}
				if(shift > 0)
				{
					barnumberMetrics.Move(0, -shift);
				}
			}
		}

		protected void MoveFramedTextBottomToDefaultPosition(Metrics framedTextMetrics)
		{			
			float staffTop = this.Voice.Staff.Metrics.StafflinesTop;
			float defaultBottom = staffTop - (Gap * 3);
			if(framedTextMetrics != null)
			{
				framedTextMetrics.Move(0, defaultBottom - framedTextMetrics.Bottom);
			}
		}

		protected void MoveFramedTextAboveDurationSymbols(Metrics drawObjectMetrics, List<NoteObject> fixedNoteObjects)
		{
			if(drawObjectMetrics != null)
			{
				foreach(NoteObject fixedNoteObject in fixedNoteObjects)
				{
					if(fixedNoteObject is DurationSymbol)
					{
						int overlaps = OverlapsHorizontally(drawObjectMetrics, fixedNoteObject);
						if(overlaps == 0)
						{
							MoveDrawObjectAboveDurationSymbol(drawObjectMetrics, fixedNoteObject);
						}
						else if(overlaps == 1)
						{
							break;
						}
					}
				}
			}
		}
		/// <summary>
		/// returns
		/// -1 if metrics is entirely to the left of the fixedNoteObject;
		/// 0 if metrics overlaps the fixedNoteObject;
		/// 1 if metrics is entirely to the right of the fixedNoteObject;
		/// </summary>
		/// <returns></returns>
		private int OverlapsHorizontally(Metrics metrics, NoteObject fixedNoteObject)
		{
			int rval = 0;
			Metrics fixedMetrics = fixedNoteObject.Metrics;
			if(metrics.Right < fixedMetrics.Left)
			{
				rval = -1;
			}
			else if(metrics.Left > fixedMetrics.Right)
			{
				rval = 1;
			}
			return rval;
		}
		/// <summary>
		/// Move the drawObject above the fixedNoteObject if it is not already.
		/// </summary>
		/// <param name="drawObjectMetrics"></param>
		/// <param name="durationSymbolMetrics"></param>
		/// <param name="gap"></param>
		private void MoveDrawObjectAboveDurationSymbol(Metrics drawObjectMetrics, NoteObject fixedNoteObject)
		{
			float verticalOverlap = 0F;
			if(fixedNoteObject.Metrics is ChordMetrics chordMetrics)
			{
				verticalOverlap = chordMetrics.OverlapHeight(drawObjectMetrics, Gap);
			}
			else if(fixedNoteObject.Metrics is RestMetrics restMetrics)
			{
				verticalOverlap = restMetrics.OverlapHeight(drawObjectMetrics, Gap);
				if(verticalOverlap > 0 && fixedNoteObject is ChordSymbol chordSymbol)
				{
					Debug.Assert(false, "Strange Code: Can a ChordSymbol have RestMetrics???");
					// fine tuning
					// compare with the extra padding given to these symbols in the RestMetrics constructor.
					switch(chordSymbol.DurationClass)
					{
						case DurationClass.breve:
						case DurationClass.semibreve:
						case DurationClass.minim:
						case DurationClass.crotchet:
						case DurationClass.quaver:
						case DurationClass.semiquaver:
							break;
						case DurationClass.threeFlags:
							verticalOverlap -= Gap;
							break;
						case DurationClass.fourFlags:
							verticalOverlap -= Gap * 2F;
							break;
						case DurationClass.fiveFlags:
							verticalOverlap -= Gap * 2.5F;
							break;
					}
				}
			}
			if(verticalOverlap > 0)
				drawObjectMetrics.Move(0F, -(verticalOverlap + Gap));
		}

		/// <summary>
		/// This function only writes the staff name, barnumber and region info to the SVG file (if they are present).
		/// The barline itself is drawn when the system (and staff edges) is complete.
		/// </summary>
		public void WriteDrawObjectsSVG(SvgWriter w)
		{
			if(StaffNameMetrics != null)
			{
				StaffNameMetrics.WriteSVG(w);
			}
			if(BarnumberMetrics != null)
			{
				BarnumberMetrics.WriteSVG(w);
			}
		}

		protected void SetDrawObjects(List<DrawObject> drawObjects)
		{
			DrawObjects.Clear();
			foreach(DrawObject drawObject in drawObjects)
			{
				drawObject.Container = this;
				DrawObjects.Add(drawObject);
			}
		}

		internal virtual void AddAncilliaryMetricsTo(StaffMetrics staffMetrics)
		{
			if(StaffNameMetrics != null)
			{
				staffMetrics.Add(StaffNameMetrics);
			}
			if(BarnumberMetrics != null)
			{
				staffMetrics.Add(BarnumberMetrics);
			}
		}

		internal abstract void AlignFramedTextsXY(List<NoteObject> noteObjects0);

		protected void AlignBarnumberX()
		{
			if(BarnumberMetrics != null)
			{
				BarnumberMetrics.Move(Barline_LineMetrics.OriginX - BarnumberMetrics.OriginX, 0);
			}
		}

		public Metrics Barline_LineMetrics = null;
		public StaffNameMetrics StaffNameMetrics = null;
		public BarnumberMetrics BarnumberMetrics = null;

        /// <summary>
        /// Default is true
        /// </summary>
        public bool IsVisible = true;

		private PageFormat PageFormat { get { return Voice.Staff.SVGSystem.Score.PageFormat; } }
		protected float StafflineStrokeWidth { get { return PageFormat.StafflineStemStrokeWidth; } }
		protected float ThinStrokeWidth { get { return PageFormat.BarlineStrokeWidth; } }
		protected float ThickStrokeWidth { get { return PageFormat.ThickBarlineStrokeWidth; } }
		protected float Gap { get { return PageFormat.Gap; } }
		protected float TopY(float topStafflineY, bool isEndOfSystem)
		{
			float topY = topStafflineY;
			if(isEndOfSystem)
			{
				float halfStafflineWidth = (StafflineStrokeWidth / 2);
				topY -= halfStafflineWidth;
			}
			return topY;
		}
		protected float BottomY(float bottomStafflineY, bool isEndOfSystem)
		{
			float bottomY = bottomStafflineY;
			if(isEndOfSystem)
			{
				float halfStafflineWidth = (StafflineStrokeWidth / 2);
				bottomY += halfStafflineWidth;
			}
			return bottomY;
		}
	}

	/// <summary>
	/// A barline which is a single, thin line. OriginX is the line's x-coordinate.
	/// </summary>
	public class NormalBarline : Barline
	{
		public NormalBarline(Voice voice)
			: base(voice)
		{
		}

		/// <summary>
		/// Writes out the barline's vertical line(s).
		/// May be called twice per staff.barline:
		///     1. for the range between top and bottom stafflines (if Barline.Visible is true)
		///     2. for the range between the staff's lower edge and the next staff's upper edge
		///        (if the staff's lower neighbour is in the same group)
		/// </summary>
		/// <param name="w"></param>
		public override void WriteSVG(SvgWriter w, float topStafflineY, float bottomStafflineY, bool isEndOfSystem)
		{
			float topY = TopY(topStafflineY, isEndOfSystem);
			float bottomY = BottomY(bottomStafflineY, isEndOfSystem);

			w.SvgLine(CSSObjectClass.barline, this.Barline_LineMetrics.OriginX, topY, this.Barline_LineMetrics.OriginX, bottomY);
		}

		public override string ToString()
		{
			return "normalBarline: ";
		}

		public override void AddMetricsToEdge(HorizontalEdge horizontalEdge)
		{
			AddBasicMetricsToEdge(horizontalEdge);
		}

		internal override void AlignFramedTextsXY(List<NoteObject> fixedNoteObjects)
		{
			#region alignX
			base.AlignBarnumberX();
			#endregion
			MoveFramedTextAboveDurationSymbols(BarnumberMetrics, fixedNoteObjects);
		}

		public override void CreateMetrics(Graphics graphics)
		{
			Barline_LineMetrics = new Barline_LineMetrics(-(ThinStrokeWidth / 2F), (ThinStrokeWidth / 2F));
			SetCommonMetrics(graphics, DrawObjects);
		}

		internal override void AddAncilliaryMetricsTo(StaffMetrics metrics)
		{
			base.AddAncilliaryMetricsTo(metrics);
		}
	}

	/// <summary>
	/// A barline whose 2 lines are (left to right) thick then thin. OriginX is the thick line's x-coordinate.
	/// </summary>
	public class StartRegionBarline : Barline
	{
		public StartRegionBarline(Voice voice, List<DrawObject> drawObjects)
			: base(voice)
		{
			SetDrawObjects(drawObjects);
		}

		/// <summary>
		/// Writes out the barline's vertical line(s).
		/// May be called twice per staff.barline:
		///     1. for the range between top and bottom stafflines (if Barline.Visible is true)
		///     2. for the range between the staff's lower edge and the next staff's upper edge
		///        (if the staff's lower neighbour is in the same group)
		/// </summary>
		/// <param name="w"></param>
		public override void WriteSVG(SvgWriter w, float topStafflineY, float bottomStafflineY, bool isEndOfSystem)
		{
			float topY = TopY(topStafflineY, isEndOfSystem);
			float bottomY = BottomY(bottomStafflineY, isEndOfSystem);

			float thickLeftLineOriginX = Barline_LineMetrics.OriginX;
			w.SvgStartGroup(CSSObjectClass.startRegionBarline.ToString());
			w.SvgLine(CSSObjectClass.thickBarline, thickLeftLineOriginX, topY, thickLeftLineOriginX, bottomY);

			float thinRightLineOriginX = thickLeftLineOriginX + (ThickStrokeWidth * 1.5F) + (ThinStrokeWidth / 2F);
			w.SvgLine(CSSObjectClass.barline, thinRightLineOriginX, topY, thinRightLineOriginX, bottomY);
			w.SvgEndGroup();
		}

		public override void AddMetricsToEdge(HorizontalEdge horizontalEdge)
		{
			if(FramedRegionStartTextMetrics != null)
			{
				horizontalEdge.Add(FramedRegionStartTextMetrics);
			}

			AddBasicMetricsToEdge(horizontalEdge);
		}

		public override string ToString()
		{
			return "startRegionBarline: ";
		}

		internal override void AlignFramedTextsXY(List<NoteObject> fixedNoteObjects)
		{
			#region alignX
			base.AlignBarnumberX();
			float originX = Barline_LineMetrics.OriginX;
			if(FramedRegionStartTextMetrics != null)
			{
				FramedRegionStartTextMetrics.Move(originX - FramedRegionStartTextMetrics.Left, 0);
			}
			#endregion

			MoveFramedTextBottomToDefaultPosition(FramedRegionStartTextMetrics);

			MoveFramedTextAboveDurationSymbols(FramedRegionStartTextMetrics, fixedNoteObjects);
			MoveFramedTextAboveDurationSymbols(BarnumberMetrics, fixedNoteObjects);

			MoveBarnumberAboveRegionBox(BarnumberMetrics, FramedRegionStartTextMetrics);
		}

		public override void CreateMetrics(Graphics graphics)
		{
			float leftEdge = -(ThickStrokeWidth / 2F);
			float rightEdge = (ThickStrokeWidth * 1.5F) + ThinStrokeWidth;
			Barline_LineMetrics = new Barline_LineMetrics(leftEdge, rightEdge);

			SetCommonMetrics(graphics, DrawObjects);

			foreach(DrawObject drawObject in DrawObjects)
			{
				if(drawObject is FramedRegionStartText frst)
				{
					FramedRegionStartTextMetrics = new FramedRegionInfoMetrics(graphics, frst.Texts, frst.FrameInfo);
					break;
				}
			}			
		}

		internal override void AddAncilliaryMetricsTo(StaffMetrics staffMetrics)
		{
			base.AddAncilliaryMetricsTo(staffMetrics);

			if(FramedRegionStartTextMetrics != null)
			{
				staffMetrics.Add(FramedRegionStartTextMetrics);
			}
		}

		public FramedRegionInfoMetrics FramedRegionStartTextMetrics = null;
	}

	/// <summary>
	/// A barline whose 2 lines are (left to right) thin then thick. OriginX is the thick line's x-coordinate.
	/// </summary>
	public class EndRegionBarline : Barline
	{
		public EndRegionBarline(Voice voice, List<DrawObject> drawObjects)
			: base(voice)
		{
			SetDrawObjects(drawObjects);
		}

		/// <summary>
		/// Writes out the barline's vertical line(s).
		/// May be called twice per staff.barline:
		///     1. for the range between top and bottom stafflines (if Barline.Visible is true)
		///     2. for the range between the staff's lower edge and the next staff's upper edge
		///        (if the staff's lower neighbour is in the same group)
		/// </summary>
		/// <param name="w"></param>
		public override void WriteSVG(SvgWriter w, float topStafflineY, float bottomStafflineY, bool isEndOfSystem)
		{
			float topY = TopY(topStafflineY, isEndOfSystem);
			float bottomY = BottomY(bottomStafflineY, isEndOfSystem);

			float thinLeftLineOriginX = Barline_LineMetrics.OriginX - (ThickStrokeWidth * 1.5F) - (ThinStrokeWidth / 2F);
			w.SvgStartGroup(CSSObjectClass.endRegionBarline.ToString());
			w.SvgLine(CSSObjectClass.barline, thinLeftLineOriginX, topY, thinLeftLineOriginX, bottomY);

			float thickRightLineOriginX = Barline_LineMetrics.OriginX;
			w.SvgLine(CSSObjectClass.thickBarline, thickRightLineOriginX, topY, thickRightLineOriginX, bottomY);
			w.SvgEndGroup();
		}

		public override void AddMetricsToEdge(HorizontalEdge horizontalEdge)
		{
			if(FramedRegionEndTextMetrics != null)
			{
				horizontalEdge.Add(FramedRegionEndTextMetrics);
			}

			AddBasicMetricsToEdge(horizontalEdge);
		}

		public override string ToString()
		{
			return "endRegionBarline: ";
		}

		internal override void AlignFramedTextsXY(List<NoteObject> fixedNoteObjects)
		{
			#region alignX
			base.AlignBarnumberX();
			float originX = Barline_LineMetrics.OriginX;
			if(FramedRegionEndTextMetrics != null)
			{
				FramedRegionEndTextMetrics.Move(originX - FramedRegionEndTextMetrics.Right, 0);
			}
			#endregion
			MoveFramedTextAboveDurationSymbols(FramedRegionEndTextMetrics, fixedNoteObjects);
			MoveFramedTextAboveDurationSymbols(BarnumberMetrics, fixedNoteObjects);

			MoveBarnumberAboveRegionBox(BarnumberMetrics, FramedRegionEndTextMetrics);
		}

		public override void CreateMetrics(Graphics graphics)
		{
			float leftEdge = -((ThickStrokeWidth * 1.5F) + ThinStrokeWidth);
			float rightEdge = (ThickStrokeWidth / 2F);
			Barline_LineMetrics = new Barline_LineMetrics(leftEdge, rightEdge);

			SetCommonMetrics(graphics, DrawObjects);

			foreach(DrawObject drawObject in DrawObjects)
			{
				if(drawObject is FramedRegionEndText frst)
				{
					FramedRegionEndTextMetrics = new FramedRegionInfoMetrics(graphics, frst.Texts, frst.FrameInfo);
					break;
				}
			}
		}

		internal override void AddAncilliaryMetricsTo(StaffMetrics staffMetrics)
		{
			base.AddAncilliaryMetricsTo(staffMetrics);
			if(FramedRegionEndTextMetrics != null)
			{
				staffMetrics.Add(FramedRegionEndTextMetrics);
			}
		}

		public FramedRegionInfoMetrics FramedRegionEndTextMetrics = null;
	}

	/// <summary>_
	/// A barline whose 3 lines are (left to right) thin, thick, thin. OriginX is the thick line's x-coordinate.
	/// </summary>
	public class EndAndStartRegionBarline : Barline
	{
		public EndAndStartRegionBarline(Voice voice, List<DrawObject> drawObjects)
			: base(voice)
		{
			SetDrawObjects(drawObjects);
		}

		/// <summary>
		/// Writes out the barline's vertical line(s).
		/// May be called twice per staff.barline:
		///     1. for the range between top and bottom stafflines (if Barline.Visible is true)
		///     2. for the range between the staff's lower edge and the next staff's upper edge
		///        (if the staff's lower neighbour is in the same group)
		/// </summary>
		/// <param name="w"></param>
		public override void WriteSVG(SvgWriter w, float topStafflineY, float bottomStafflineY, bool isEndOfSystem)
		{
			float topY = TopY(topStafflineY, isEndOfSystem);
			float bottomY = BottomY(bottomStafflineY, isEndOfSystem);
			
			w.SvgStartGroup(CSSObjectClass.endAndStartRegionBarline.ToString());

			float thinLeftLineOriginX = Barline_LineMetrics.OriginX - (ThickStrokeWidth * 1.5F) - (ThinStrokeWidth / 2F);
			w.SvgLine(CSSObjectClass.barline, thinLeftLineOriginX, topY, thinLeftLineOriginX, bottomY);

			float thickCentreLineOriginX = Barline_LineMetrics.OriginX;
			w.SvgLine(CSSObjectClass.thickBarline, thickCentreLineOriginX, topY, thickCentreLineOriginX, bottomY);

			float thinRightLineOriginX = thickCentreLineOriginX + (ThickStrokeWidth * 1.5F) + (ThinStrokeWidth / 2F);
			w.SvgLine(CSSObjectClass.barline, thinRightLineOriginX, topY, thinRightLineOriginX, bottomY);

			w.SvgEndGroup();
		}

		public override void AddMetricsToEdge(HorizontalEdge horizontalEdge)
		{
			if(FramedRegionEndTextMetrics != null)
			{
				horizontalEdge.Add(FramedRegionEndTextMetrics);
			}

			if(FramedRegionStartTextMetrics != null)
			{
				horizontalEdge.Add(FramedRegionStartTextMetrics);
			}

			AddBasicMetricsToEdge(horizontalEdge);
		}

		public override string ToString()
		{
			return "endAndStartRegionBarline: ";
		}

		internal override void AlignFramedTextsXY(List<NoteObject> fixedNoteObjects)
		{
			#region alignX
			base.AlignBarnumberX();
			float originX = Barline_LineMetrics.OriginX;
			if(FramedRegionEndTextMetrics != null)
			{
				FramedRegionEndTextMetrics.Move(originX - FramedRegionEndTextMetrics.Right, 0);
			}
			if(FramedRegionStartTextMetrics != null)
			{
				FramedRegionStartTextMetrics.Move(originX - FramedRegionStartTextMetrics.Left, 0);
			}
			#endregion

			MoveFramedTextAboveDurationSymbols(FramedRegionStartTextMetrics, fixedNoteObjects);
			MoveFramedTextAboveDurationSymbols(FramedRegionEndTextMetrics, fixedNoteObjects);
			MoveFramedTextAboveDurationSymbols(BarnumberMetrics, fixedNoteObjects);

			MoveBarnumberAboveRegionBox(BarnumberMetrics, FramedRegionStartTextMetrics);
			MoveBarnumberAboveRegionBox(BarnumberMetrics, FramedRegionEndTextMetrics);
		}

		public override void CreateMetrics(Graphics graphics)
		{
			float rightEdge = (ThickStrokeWidth * 1.5F) + ThinStrokeWidth;
			float leftEdge = -rightEdge;
			Barline_LineMetrics = new Barline_LineMetrics(leftEdge, rightEdge);

			SetCommonMetrics(graphics, DrawObjects);

			foreach(DrawObject drawObject in DrawObjects)
			{
				if(drawObject is FramedRegionStartText frst)
				{
					FramedRegionStartTextMetrics = new FramedRegionInfoMetrics(graphics, frst.Texts, frst.FrameInfo);
				}
				if(drawObject is FramedRegionEndText fret)
				{
					FramedRegionEndTextMetrics = new FramedRegionInfoMetrics(graphics, fret.Texts, fret.FrameInfo);
				}
			}
		}

		internal override void AddAncilliaryMetricsTo(StaffMetrics staffMetrics)
		{
			base.AddAncilliaryMetricsTo(staffMetrics);

			if(FramedRegionStartTextMetrics != null)
			{
				staffMetrics.Add(FramedRegionStartTextMetrics);
			}
			if(FramedRegionEndTextMetrics != null)
			{
				staffMetrics.Add(FramedRegionEndTextMetrics);
			}
		}

		public FramedRegionInfoMetrics FramedRegionStartTextMetrics = null;
		public FramedRegionInfoMetrics FramedRegionEndTextMetrics = null;
	}

}
