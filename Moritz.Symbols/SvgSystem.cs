using System;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

using Moritz.Xml;
using Moritz.Spec;
using Moritz.Globals;

namespace Moritz.Symbols
{
    public class SvgSystem
    {
        /// <summary>
        /// Moritz defaults: tempo=30, InstrNotation=long, left indent=0, right indent = 0;
        /// </summary>
        /// <param name="score"></param>
        public SvgSystem(SvgScore score)
        {
            Score = score;
        }

        /// <summary>
        /// Writes out all the SVGSystem's staves. 
        /// </summary>
        /// <param name="w"></param>
        public void WriteSVG(SvgWriter w, int systemNumber, PageFormat pageFormat, List<CarryMsgs> carryMsgsPerChannel)
        {
            w.SvgStartGroup(this.Metrics.CSSObjectClass.ToString()); // "system"

            WriteLeftToRightElement(w);

            for(int staffIndex = 0; staffIndex < Staves.Count; staffIndex++)
            {
                Staves[staffIndex].WriteSVG(w, systemNumber, staffIndex + 1, carryMsgsPerChannel);
            }

            w.SvgStartGroup(CSSObjectClass.staffConnectors.ToString());

            WriteConnectors(w, systemNumber, pageFormat);

            w.SvgEndGroup(); // connectors

            w.SvgEndGroup(); // system
        }

        private void WriteLeftToRightElement(SvgWriter w)
        {
            w.WriteStartElement("score", "leftToRight", null);
            w.WriteAttributeString("systemTop", M.FloatToShortString(this.Metrics.NotesTop));
            w.WriteAttributeString("systemBottom", M.FloatToShortString(this.Metrics.NotesBottom));
            w.WriteEndElement(); // leftToRight
        }

        private void WriteConnectors(SvgWriter w, int systemNumber, PageFormat pageFormat)
        {
            List<bool> barlineContinuesDownList = pageFormat.BarlineContinuesDownList;
            Debug.Assert(barlineContinuesDownList[barlineContinuesDownList.Count - 1] == false);
            EndBarline endBarline = null;
            Barline barline = null;
            bool isFirstBarline = true;

            for(int staffIndex = 0; staffIndex < Staves.Count; staffIndex++)
            {
                Staff staff = Staves[staffIndex];
                if(staff.Metrics != null)
                {
                    Voice voice = staff.Voices[0];
                    float barlinesTop = staff.Metrics.StafflinesTop;
                    float barlinesBottom = staff.Metrics.StafflinesBottom;

                    #region set barlinesTop, barlinesBottom
                    switch(staff.NumberOfStafflines)
                    {
                        case 1:
                            barlinesTop -= (staff.Gap * 1.5F);
                            barlinesBottom += (staff.Gap * 1.5F);
                            break;
                        case 2:
                        case 3:
                        case 4:
                            barlinesTop -= staff.Gap;
                            barlinesBottom += staff.Gap;
                            break;
                        default:
                            break;
                    }
                    #endregion set barlinesTop, barlinesBottom

                    #region draw barlines down from staves
                    if(staffIndex < Staves.Count - 1)
                    {
                        //TopEdge topEdge = new TopEdge(Staves[staffIndex + 1], 0F, pageFormat.Right);
                        TopEdge topEdge = GetTopEdge(staffIndex + 1, pageFormat.Right);
                        if(topEdge != null)
                        {
                            BottomEdge bottomEdge = new BottomEdge(staff, 0F, pageFormat.Right, pageFormat.Gap);
                            isFirstBarline = true;

                            for(int i = 0; i < voice.NoteObjects.Count; ++i)
                            {
                                NoteObject noteObject = voice.NoteObjects[i];
                                barline = noteObject as Barline; // EndBarline is also a Barline
                                if(barline != null || endBarline != null)
                                {
                                    // draw grouping barlines between staves
                                    if(barlineContinuesDownList[staffIndex] || isFirstBarline)
                                    {
                                        float top = bottomEdge.YatX(barline.Metrics.OriginX) - pageFormat.StafflineStemStrokeWidth;
                                        float bottom = topEdge.YatX(barline.Metrics.OriginX);
                                        bool isLastNoteObject = (i == (voice.NoteObjects.Count - 1));
                                        barline.WriteSVG(w, top, bottom, pageFormat.StafflineStemStrokeWidth, isLastNoteObject);
                                        isFirstBarline = false;
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }
            }
        }

        #region make graphics
        /// <summary>
        /// All the objects in this SvgSystem are given Metrics which are then moved to their
        /// final positions within the SvgSystem.
        /// When this function returns, all the contained, drawable objects have their final
        /// relative positions within the sysrtem. They are actually drawn when the SvgSystem
		/// has been moved to its final position on the page.
		/// If there are any remaining overlapping symbols in the system, they are reported in the
		/// returned list of Tuples (this list can be empty). The Tuple members are:
		///     item1: system number
		///     item2: the number of remaining overlaps (in top or lower voices)
		///     item3: "top" or "lower", saying in which voices the overlaps are.
        /// </summary>
        public List<Tuple<int, int, string>> MakeGraphics(Graphics graphics, int systemNumber, PageFormat pageFormat, float leftMargin)
        {
            if(Metrics == null)
                CreateMetrics(graphics, pageFormat, leftMargin);

            // All noteObject metrics are now on the left edge of the page.
            // Chords are aligned on the left edge of the page, with accidentals etc further to 
            // the left. If two standard chords are synchronous in two voices of the same staff,
            // and the noteheads would overlap, the lower chord will have been been moved slightly
            // left or right. The two chords are at their final positions relative to each other.
			// Barnumbers are aligned centred at a default position just above the first barline

            MoveClefsAndBarlines(pageFormat.StafflineStemStrokeWidth);

            List<NoteObjectMoment> moments = MomentSymbols(pageFormat.Gap);

			SymbolSet symbolSet = Score.Notator.SymbolSet;

			// barlineWidths:  Key is a moment's msPosition. Value is the distance between the left edge 
			// of the barline and the AlignmentX of the moment which immediately follows it.
			Dictionary<int, float> barlineWidths = GetBarlineWidths(moments, pageFormat);

			DistributeProportionally(moments, barlineWidths, pageFormat, leftMargin);

			// The moments have now been distributed proportionally within each bar, but no checking has
			// been done for overlapping noteObject Metrics.

			// SymbolSet is an abstract root class, and the functions called on symbolSet are virtual.
			// Usually they only do something when symbolSet is a StandardSymbolSet.
			symbolSet.AdjustRestsVertically(Staves);
			symbolSet.SetBeamedStemLengths(Staves); // see the comment next to the function

			List<Tuple<int, int, string>> overlapsInfoList = JustifyHorizontally(systemNumber, moments, barlineWidths, pageFormat.StafflineStemStrokeWidth);

			symbolSet.FinalizeBeamBlocks(Staves);
            symbolSet.AlignLyrics(Staves);
            SvgSystem nextSystem = null;
            if(systemNumber < this.Score.Systems.Count)
            {
                nextSystem = this.Score.Systems[systemNumber];
            }
            symbolSet.AddNoteheadExtenderLines(Staves, pageFormat.RightMarginPos, pageFormat.Gap,
                pageFormat.NoteheadExtenderStrokeWidth, pageFormat.StafflineStemStrokeWidth, nextSystem);

            SetBarlineVisibility(pageFormat.BarlineContinuesDownList);

            JustifyVertically(pageFormat.Right, pageFormat.Gap);

            AdjustBarnumberVertically(pageFormat.Gap);

			AlignStaffnamesInLeftMargin(leftMargin, pageFormat.Gap);

			ResetStaffMetricsBoundaries();

			return overlapsInfoList;
        }

        private float CreateMetrics(Graphics graphics, PageFormat pageFormat, float leftMarginPos)
        {           
            this.Metrics = new SystemMetrics();
            List<NoteObject> NoteObjectsToRemove = new List<NoteObject>();
            for(int staffIndex = 0; staffIndex < Staves.Count; ++staffIndex)
            {
                Staff staff = Staves[staffIndex];

                float staffHeight = staff.Gap * (staff.NumberOfStafflines - 1);
                staff.Metrics = new StaffMetrics(leftMarginPos, pageFormat.RightMarginPos, staffHeight, staff is InputStaff);

                for(int voiceIndex = 0; voiceIndex < staff.Voices.Count; ++voiceIndex)
                {
                    Voice voice = staff.Voices[voiceIndex];

                    voice.SetChordStemDirectionsAndCreateBeamBlocks(pageFormat);

                    for(int nIndex = 0; nIndex < staff.Voices[voiceIndex].NoteObjects.Count; nIndex++)
                    {
                        NoteObject noteObject = staff.Voices[voiceIndex].NoteObjects[nIndex];
                        noteObject.Metrics = Score.Notator.SymbolSet.NoteObjectMetrics(graphics, noteObject, voice.StemDirection, staff.Gap, pageFormat);

                        if(noteObject.Metrics != null)
                            staff.Metrics.Add(noteObject.Metrics);
                        else
                            NoteObjectsToRemove.Add(noteObject);
                    }

                    foreach(NoteObject noteObject in NoteObjectsToRemove)
                        staff.Voices[voiceIndex].NoteObjects.Remove(noteObject);
                    NoteObjectsToRemove.Clear();
                }

                if(staff.Voices.Count > 1)
                {
                    Debug.Assert(Score.Notator.SymbolSet is StandardSymbolSet);
                    // Other symbol sets do not support multi voice staves.
                    staff.AdjustTwoPartChords();
                }

                staff.Metrics.Move(0f, pageFormat.DefaultDistanceBetweenStaves * staffIndex);
                this.Metrics.Add(staff.Metrics);
            }

            return (this.Metrics.Bottom - this.Metrics.Top);
        }

		/// <summary> 
		/// The horizontal Metrics of all the NoteObjects are set to their final display positions.
		/// Returns a list of Tuples, which is empty if there were no remaining overlapping symbols.
		/// If there are Tuples in the list, they contain:
		///     item1: system number
		///     item2: the number of remaining overlaps (in top or lower voices)
		///     item3: "top" or "lower", saying in which voices the overlaps are.
		/// </summary>
		/// <param name="pageFormat"></param>
		private List<Tuple<int, int, string>> JustifyHorizontally(int systemNumber, List<NoteObjectMoment> systemMoments, Dictionary<int, float> barlineWidths, float hairline)
        {
			var overlaps = new Dictionary<int, float>();
			var rval = new List<Tuple<int, int, string>>();

			HashSet<int> nonCompressibleSystemMomentPositions = new HashSet<int>();
            bool lowerVoiceMoved = false;
            do
            {
                overlaps = JustifyTopVoicesHorizontally(systemMoments, barlineWidths, nonCompressibleSystemMomentPositions, hairline);
				if(overlaps.Count > 0)
				{
					rval.Add(Tuple.Create(systemNumber, overlaps.Count, "top"));
				}

				if(LowerVoicesExist)
                {
                    lowerVoiceMoved = false;
                    overlaps = JustifyLowerVoicesHorizontally(ref lowerVoiceMoved, systemMoments, barlineWidths, nonCompressibleSystemMomentPositions, hairline);
					if(overlaps.Count > 0)
					{
						rval.Add(Tuple.Create(systemNumber, overlaps.Count, "lower"));
					}
				}

            } while(lowerVoiceMoved);

			return rval;
		}

		private Dictionary<int, float> JustifyTopVoicesHorizontally(List<NoteObjectMoment> systemMoments, Dictionary<int, float> barlineWidths, 
            HashSet<int> nonCompressibleSystemMomentPositions, float hairline)
        {
            Dictionary<int, float> overlaps = new Dictionary<int, float>();
            List<NoteObjectMoment> moments = null;
			bool allStavesRedistributed = true;
			bool staffRedistributed = false;
			do
            {
                foreach(Staff staff in this.Staves)
                {
                    moments = GetVoiceMoments(staff.Voices[0], systemMoments);
                    if(moments.Count > 1)
                        overlaps = GetOverlaps(moments, hairline);
                    if(overlaps.Count == 0)
                        continue;

                    staffRedistributed = RedistributeMoments(systemMoments, barlineWidths, nonCompressibleSystemMomentPositions, moments, overlaps);
					allStavesRedistributed = (staffRedistributed == false) ? false : allStavesRedistributed;
                }

            } while(overlaps.Count > 0 && allStavesRedistributed);

            return overlaps;
        }

        private bool LowerVoicesExist
        {
            get
            {
                bool lowerVoicesExist = false;
                foreach(Staff staff in this.Staves)
                {
                    if(staff.Voices.Count > 1)
                    {
                        lowerVoicesExist = true;
                        break;
                    }
                }
                return lowerVoicesExist;
            }
        }

		private Dictionary<int, float> JustifyLowerVoicesHorizontally(ref bool lowerVoiceMoved, 
            List<NoteObjectMoment> systemMoments, Dictionary<int, float> barlineWidths,
            HashSet<int> nonCompressibleSystemMomentPositions, float hairline)
        {
            Dictionary<int, float> overlaps = new Dictionary<int, float>();
            List<NoteObjectMoment> moments = null;
			bool redistributed = false;
			do
            {
                foreach(Staff staff in this.Staves)
                {
                    if(staff.Voices.Count > 1)
                    {
                        moments = GetVoiceMoments(staff.Voices[1], systemMoments);
                        if(moments.Count > 1)
                            overlaps = GetOverlaps(moments, hairline);
                        if(overlaps.Count == 0)
                        {
                            moments = GetStaffMoments(staff, systemMoments);
                            if(moments.Count > 1)
                                overlaps = GetOverlaps(moments, hairline);
                        }

                        if(overlaps.Count == 0)
                            continue;

                        lowerVoiceMoved = true;
						redistributed = RedistributeMoments(systemMoments, barlineWidths, nonCompressibleSystemMomentPositions, moments, overlaps);
                    }
                }

            } while(overlaps.Count > 0 && redistributed);

            return overlaps;
        }

        private List<NoteObjectMoment> GetVoiceMoments(Voice voice, List<NoteObjectMoment> systemMoments)
        {
            List<NoteObjectMoment> voiceMoments = new List<NoteObjectMoment>();
            NoteObjectMoment voiceNOM = null;
            foreach(NoteObjectMoment systemNOM in systemMoments)
            {
                voiceNOM = null;
                foreach(NoteObject noteObject in systemNOM.NoteObjects)
                {
                    if(noteObject.Voice == voice)
                    {
                        if(voiceNOM == null)
                        {
                            // noteObject in voice 1
                            voiceNOM = new NoteObjectMoment(systemNOM.AbsMsPosition);
                            voiceNOM.Add(noteObject);
                            voiceNOM.AlignmentX = systemNOM.AlignmentX;
                        }
                        else // noteObject in voice 2
                        {
                            voiceNOM.Add(noteObject);
                        }
                    }
                }
                if(voiceNOM != null)
                    voiceMoments.Add(voiceNOM);
            }
            return voiceMoments;
        }

        private List<NoteObjectMoment> GetStaffMoments(Staff staff, List<NoteObjectMoment> systemMoments)
        {
            List<NoteObjectMoment> staffMoments = new List<NoteObjectMoment>();
            NoteObjectMoment staffNOM = null;
            foreach(NoteObjectMoment systemNOM in systemMoments)
            {
                staffNOM = null;
                foreach(NoteObject noteObject in systemNOM.NoteObjects)
                {
                    if(noteObject.Voice.Staff == staff)
                    {
                        if(staffNOM == null)
                        {
                            // noteObject in voice 1
                            staffNOM = new NoteObjectMoment(systemNOM.AbsMsPosition);
                            staffNOM.Add(noteObject);
                            staffNOM.AlignmentX = systemNOM.AlignmentX;
                        }
                        else // noteObject in voice 2
                        {
                            staffNOM.Add(noteObject);
                        }
                    }
                }
                if(staffNOM != null)
                    staffMoments.Add(staffNOM);
            }
            return staffMoments;
        }

        /// <summary>
        /// the returned barlineWidths dictionary contains
        ///     key: the msPosition of a system moment
        ///     value: the distance between the left edge of the barline and the moment's AlignmentX.
        /// </summary>
        /// <param name="moments"></param>
        /// <param name="gap"></param>
        /// <returns></returns>
        private Dictionary<int, float> GetBarlineWidths(List<NoteObjectMoment> moments, PageFormat pageFormat)
        {
            Dictionary<int, float> barlineWidths = new Dictionary<int, float>();

            Barline barline = null;
            int absMsPos = 0;
            for(int i = 1; i < moments.Count; i++)
            {
                absMsPos = moments[i].AbsMsPosition;
                float barlineWidth = 0F;
                Debug.Assert(moments.Count > 1);

                barline = moments[i].Barline;
                if(barline != null && barline.Metrics != null)
                {
                    barlineWidth = moments[i].AlignmentX - barline.Metrics.Left;
                    barlineWidths.Add(absMsPos, barlineWidth);
                }
            }
            return barlineWidths;
        }
        /// <summary>
        /// The MomentSymbols are in order of msPosition.
        /// The contained symbols are in order of voice (top-bottom of this system).
        /// Barlines and ClefSigns have been added to the NoteObjectMomentSymbol containing the following
        /// DurationSymbol.
        /// When this function returns, moments are in order of msPosition,
        /// and aligned internally at AlignmentX = 0;
        /// </summary>
        /// <typeparam name="Type">DurationSymbol, ChordSymbol, RestSymbol</typeparam>
        private List<NoteObjectMoment> MomentSymbols(float gap)
        {
            SortedDictionary<int, NoteObjectMoment> dict = new SortedDictionary<int, NoteObjectMoment>();
            Barline barline = null;
            Clef clef = null;
            foreach(Staff staff in Staves)
            {
                foreach(Voice voice in staff.Voices)
                {
                    int key = -1;
                    #region foreach noteObject
                    foreach(NoteObject noteObject in voice.NoteObjects)
                    {
						if(!(noteObject is DurationSymbol durationSymbol))
						{
							if(noteObject is Clef)
								clef = noteObject as Clef;
							if(noteObject is Barline)
								barline = noteObject as Barline;
						}
						else
						{
							key = durationSymbol.AbsMsPosition;

							if(!dict.ContainsKey(key))
							{
								dict.Add(key, new NoteObjectMoment(durationSymbol.AbsMsPosition));
							}

							if(clef != null)
							{
								dict[key].Add(clef);
								clef = null;
							}
							if(barline != null)
							{
								dict[key].Add(barline);
								barline = null;
							}

							dict[key].Add(durationSymbol);
						}
					}
                    #endregion

                    if(clef != null) // final clef
                    {
                        if(dict.ContainsKey(this.AbsEndMsPosition))
                            dict[this.AbsEndMsPosition].Add(clef);
                        else
                        {
                            NoteObjectMoment nom = new NoteObjectMoment(this.AbsEndMsPosition);
                            nom.Add(clef);
                            dict.Add(this.AbsEndMsPosition, nom);
                        }
                    }
                    if(barline != null) // final barline
                    {
                        if(dict.ContainsKey(this.AbsEndMsPosition))
                            dict[this.AbsEndMsPosition].Add(barline);
                        else
                        {
                            NoteObjectMoment nom = new NoteObjectMoment(this.AbsEndMsPosition);
                            nom.Add(barline);
                            dict.Add(this.AbsEndMsPosition, nom);
                        }
                    }
                }
            }

            List<NoteObjectMoment> momentSymbols = new List<NoteObjectMoment>();
            Debug.Assert(dict.Count > 0);
            foreach(int key in dict.Keys)
                momentSymbols.Add(dict[key]);

            foreach(NoteObjectMoment momentSymbol in momentSymbols)
            {
                momentSymbol.AlignBarlineAndClefGlyphs(gap);
            }

            #region debug
            // moments are currently in order of msPosition.
            float prevMsPos = -1;
            foreach(NoteObjectMoment moment in momentSymbols)
            {
                Debug.Assert(moment.AbsMsPosition > prevMsPos);
                prevMsPos = moment.AbsMsPosition;
            }
            #endregion

            return momentSymbols;
        }

        /// <summary>
        /// Moves clefs and barlines to the left of the following duration symbols, leaving a hairline gap between the symbols.
        /// If the first durationSymbol at the start of the staff is a cautionary, the distance from the clef to the barline is gap.
        /// </summary>
        private void MoveClefsAndBarlines(float hairline)
        {
            Clef clef = null;
            Barline barline = null;

            foreach(Staff staff in Staves)
            {
                if(staff.Metrics != null)
                {
                    foreach(Voice voice in staff.Voices)
                    {
                        foreach(NoteObject noteObject in voice.NoteObjects)
                        {
                            if(noteObject is Clef)
                                clef = noteObject as Clef;
                            if(noteObject is Barline)
                                barline = noteObject as Barline;

                            if(noteObject is DurationSymbol durationSymbol)
                            {
                                if(barline != null)
                                {
                                    barline.Metrics.Move(durationSymbol.Metrics.Left - barline.Metrics.Right - hairline, 0F);

                                    if(clef != null)
                                    {
                                        clef.Metrics.Move(barline.Metrics.OriginX - clef.Metrics.Right - hairline, 0F); // clefs have a space on the right
                                    }
                                }
                                else if(clef != null)
                                {
                                    clef.Metrics.Move(durationSymbol.Metrics.Left - clef.Metrics.Right - hairline, 0F);
                                }
                                clef = null;
                                barline = null;
                                durationSymbol = null;
                            }
                            else if(barline != null && clef != null)
                            {
                                clef.Metrics.Move(barline.Metrics.OriginX - clef.Metrics.Right - hairline, 0F); // clefs have a space on the right
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// When this function returns, the moments have been distributed proportionally within each bar.
        /// Symbols are at their correct positions, except that no checking has been done for overlapping noteObject Metrics.
        /// </summary>
        private void DistributeProportionally(List<NoteObjectMoment> moments, Dictionary<int, float> barlineWidths, PageFormat pageFormat,
            float leftMarginPos)
        {
            List<float> momentWidths = new List<float>();

            float momentWidth = 0;
            for(int i = 1; i < moments.Count; i++)
            {
                momentWidth = (moments[i].AbsMsPosition - moments[i - 1].AbsMsPosition) * 10000F;
                momentWidths.Add(momentWidth);
            }
            momentWidths.Add(0F); // final barline

            float totalMomentWidths = 0F;
            foreach(float width in momentWidths)
                totalMomentWidths += width;

            float totalBarlineWidths = 0F;
            foreach(float width in barlineWidths.Values)
            {
                totalBarlineWidths += width;
            }

            float leftEdgeToFirstAlignment = moments[0].LeftEdgeToAlignment();

            float spreadWidth = pageFormat.RightMarginPos - leftMarginPos - leftEdgeToFirstAlignment - totalBarlineWidths;

            float factor = spreadWidth / totalMomentWidths;

            float currentPosition = leftMarginPos + leftEdgeToFirstAlignment;
            for(int i = 0; i < momentWidths.Count; i++)
            {
                if(barlineWidths.ContainsKey(moments[i].AbsMsPosition))
                {
                    currentPosition += barlineWidths[moments[i].AbsMsPosition];
                }
                moments[i].MoveToAlignmentX(currentPosition);
                currentPosition += momentWidths[i] * factor;
            }
        }
        /// <summary>
        /// Returns a dictionary containing moment msPositions and the overlapWidth they contain.
        /// The msPositions are of the moments whose width will have to be increased because they contain one or more
        /// anchorage symbols which overlap the next symbol on the same staff.
        /// </summary>
        private Dictionary<int, float> GetOverlaps(List<NoteObjectMoment> staffMoments, float hairline)
        {
            Dictionary<int, float> overlaps = new Dictionary<int, float>();

            NoteObjectMoment previousNOM = null;
            float overlapWidth = 0;
            int absMsPos = 0;
            int previousMsPos = 0;

            foreach(NoteObjectMoment nom in staffMoments)
            {
                if(previousNOM != null)
                {
                    foreach(NoteObject noteObject in nom.NoteObjects)
                    {
                        overlapWidth = noteObject.OverlapWidth(previousNOM);
                        if(overlapWidth >= 0)
                        {
                            absMsPos = nom.AbsMsPosition;
                            previousMsPos = previousNOM.AbsMsPosition;

                            overlapWidth += hairline;
                            if(overlaps.ContainsKey(previousMsPos))
                                overlaps[previousMsPos] = overlaps[previousMsPos] > overlapWidth ? overlaps[previousMsPos] : overlapWidth;
                            else
                                overlaps.Add(previousMsPos, overlapWidth);
                        }
                    }
                }
                previousNOM = nom;
            }

            return overlaps;
        }

        /// <summary>
        /// Redistributes the moments horizontally (regardless of any overlaps).
        /// </summary>
        private bool RedistributeMoments(List<NoteObjectMoment> systemMoments, Dictionary<int, float> barlineWidths,
            HashSet<int> nonCompressibleSystemMomentPositions,
            List<NoteObjectMoment> staffMoments, // the NoteObjectMoments used by the current staff (contains their msPositions)
            Dictionary<int, float> staffOverlaps) // msPosition, overlap.
        {
            Debug.Assert(systemMoments.Count > 1 && staffMoments.Count > 1);

            SetNonCompressible(nonCompressibleSystemMomentPositions, systemMoments, staffMoments, staffOverlaps);

            Dictionary<int, float> systemMomentWidthsWithoutBarlines = GetExistingWidthsWithoutBarlines(systemMoments, barlineWidths);

			/// The factor by which to compress all those moment widths which are to be compressed.
			float compressionFactor = CompressionFactor(systemMomentWidthsWithoutBarlines, nonCompressibleSystemMomentPositions, staffOverlaps);

			/// If compressionFactor is less than or equal to 0, the moments will overlap
			/// in the resulting score (and a warning message will be displayed).
			if(compressionFactor > 0)
			{
				/// widthFactors contains the factor by which to multiply the existing width of each system moment.
				Dictionary<int, float> widthFactors =
					WidthFactors(systemMoments, staffMoments, staffOverlaps, barlineWidths, nonCompressibleSystemMomentPositions, compressionFactor);

				for(int i = 1; i < systemMoments.Count; ++i)
				{
					int sysMomentMsPos = systemMoments[i - 1].AbsMsPosition;
					float existingWidth = systemMomentWidthsWithoutBarlines[sysMomentMsPos];
					float alignmentX = systemMoments[i - 1].AlignmentX + (existingWidth * widthFactors[sysMomentMsPos]);

					if(barlineWidths.ContainsKey(systemMoments[i].AbsMsPosition))
						alignmentX += barlineWidths[systemMoments[i].AbsMsPosition];

					systemMoments[i].MoveToAlignmentX(alignmentX);
				}
				return true;
			}
			else
			{
				return false;
			}
        }

        private Dictionary<int, float> GetExistingWidthsWithoutBarlines(
            List<NoteObjectMoment> moments,
            Dictionary<int, float> barlineWidths)
        {
            float originalWidth = 0;
            Dictionary<int, float> originalWidthsWithoutBarlines = new Dictionary<int, float>();

            for(int i = 1; i < moments.Count; i++)
            {
                originalWidth = moments[i].AlignmentX - moments[i - 1].AlignmentX;
                if(barlineWidths.ContainsKey(moments[i].AbsMsPosition))
                {
                    originalWidth -= barlineWidths[moments[i].AbsMsPosition];
                }

                originalWidthsWithoutBarlines.Add(moments[i - 1].AbsMsPosition, originalWidth);
            }

            return originalWidthsWithoutBarlines;
        }

        /// <summary>
        /// systemMoments which are about to be widened, are set to be non-compressible.
        /// In other words, this function adds the MsPositions of all systemMoments in range of
        /// the staffMoments having staffOverlaps to the nonCompressibleSystemMomentPositions hash set.
        /// </summary>
        private void SetNonCompressible(HashSet<int> nonCompressibleSystemMomentPositions, 
            List<NoteObjectMoment> systemMoments,
            List<NoteObjectMoment> staffMoments, 
            Dictionary<int, float> staffOverlaps)
        {
            Debug.Assert(staffMoments.Count > 1);
            for(int stmIndex = 1; stmIndex < staffMoments.Count; ++stmIndex)
            {
                int prevMPos = staffMoments[stmIndex - 1].AbsMsPosition;
                int mPos = staffMoments[stmIndex].AbsMsPosition;
                if(staffOverlaps.ContainsKey(prevMPos))
                {
                    int startIndex = 0;
                    int endIndex = 0;
                    for(int i = 0; i < systemMoments.Count; ++i)
                    {
                        if(systemMoments[i].AbsMsPosition == prevMPos)
                        {
                            startIndex = i;
                        }
                        if(systemMoments[i].AbsMsPosition == mPos)
                        {
                            endIndex = i;
                            break;
                        }
                    }
                    for(int i = startIndex; i < endIndex; ++i)
                    {
                        nonCompressibleSystemMomentPositions.Add(systemMoments[i].AbsMsPosition);
                    }
                }
            }
        }

        /// <summary>
        /// The factor by which to compress those moment widths which are to be compressed.
		/// If the returned value is less than or equal to zero, it will not be used (see
		/// the calling function).
        /// </summary>
        private float CompressionFactor(Dictionary<int, float> systemMomentWidthsWithoutBarlines, HashSet<int> nonCompressibleSystemMomentPositions, Dictionary<int, float> staffOverlaps)
        {
            float totalCompressibleWidth = TotalCompressibleWidth(systemMomentWidthsWithoutBarlines, nonCompressibleSystemMomentPositions);
            float totalOverlaps = TotalOverlaps(staffOverlaps);
            float compressionFactor = (totalCompressibleWidth - totalOverlaps) / totalCompressibleWidth;

            return compressionFactor;
        }

        private float TotalCompressibleWidth(Dictionary<int, float> originalSystemMomentWidths,
                                            HashSet<int> nonCompressibleSystemMomentPositions)
        {
            float totalCompressibleWidth = 0F;
            foreach(int msPos in originalSystemMomentWidths.Keys)
            {
                if(!nonCompressibleSystemMomentPositions.Contains(msPos))
                    totalCompressibleWidth += originalSystemMomentWidths[msPos];
            }
            return totalCompressibleWidth;
        }

        private float TotalOverlaps(Dictionary<int, float> staffOverlaps)
        {
            float total = 0;
            foreach(float overlap in staffOverlaps.Values)
            {
                total += overlap;
            }
            return total;
        }

        /// <summary>
        /// Returns a dictionary having an entry for each staff moment, whose key/value pairs are
        ///     key: the msPosition of a staffMoment
        ///     value: the factor by which the staffMoment will be expanded in order to remove a staffOverlap.
        /// Staff moments can span more than one systemMoment...
        /// </summary>
        private Dictionary<int, float> GetStaffExpansionFactors(
            Dictionary<int, float> staffMomentWidthsWithoutBarlines,
            Dictionary<int, float> staffOverlaps)
        {
            Dictionary<int, float> staffExpansionFactors = new Dictionary<int, float>();
            float originalWidth;
            float factor;
            foreach(int msPosition in staffMomentWidthsWithoutBarlines.Keys)
            {
                if(staffOverlaps.ContainsKey(msPosition))
                {
                    originalWidth = staffMomentWidthsWithoutBarlines[msPosition];
                    factor = (originalWidth + staffOverlaps[msPosition]) / originalWidth;
                    staffExpansionFactors.Add(msPosition, factor);
                }
            }

            return staffExpansionFactors;
        }

        /// <summary>
        /// Returns an [msPos, changeFactor] pair for each moment in the system.
        /// The change factor can be 1, compressionFactor, or taken from the staffExpansionFactors.
        /// </summary>
        private Dictionary<int, float> WidthFactors(
            List<NoteObjectMoment> systemMoments, 
            List<NoteObjectMoment> staffMoments,
            Dictionary<int, float> staffOverlaps,
            Dictionary<int, float> barlineWidths,
            HashSet<int> nonCompressibleSystemMomentPositions,
            float compressionFactor)
        {
            Dictionary<int, float> staffMomentWidthsWithoutBarlines =
                            GetExistingWidthsWithoutBarlines(staffMoments, barlineWidths);

            // contains an <msPos, expansionFactor> pair for each staffMoment that will be expanded.
            Dictionary<int, float> staffExpansionFactors =
                            GetStaffExpansionFactors(staffMomentWidthsWithoutBarlines, staffOverlaps);

            List<int> systemMomentKeys = new List<int>();
            foreach(NoteObjectMoment nom in systemMoments)
            {
                systemMomentKeys.Add(nom.AbsMsPosition);
            }

            Dictionary<int, float> widthFactors = new Dictionary<int, float>();
            foreach(int msPos in systemMomentKeys)
            {
                if(nonCompressibleSystemMomentPositions.Contains(msPos))
                    widthFactors.Add(msPos, 1F); // default factor is 1 (moments which are nonCompressible)
                else
                    widthFactors.Add(msPos, compressionFactor);
            }
            // widthFactors now contains an entry for each system Moment
            // Now set the expansionFactors from the staff.

            float expFactor = 0;
            for(int i = 1; i < staffMoments.Count; ++i)
            {
                int startMsPos = staffMoments[i - 1].AbsMsPosition;
                int endMsPos = staffMoments[i].AbsMsPosition;
                if(staffExpansionFactors.ContainsKey(startMsPos))
                {
                    expFactor = staffExpansionFactors[startMsPos];
                    foreach(int msPos in systemMomentKeys)
                    {
                        if(msPos >= startMsPos && msPos < endMsPos)
                        {
                            widthFactors[msPos] = expFactor;
                        }
                        if(msPos >= endMsPos)
                            break;
                    }
                }
            }
            return widthFactors;
        }



        /// <summary>
        /// The barnumber is currently at its default position.
        /// Now, if it currently collides with part of the following chord,
        /// (or, if the first duration symbol is a rest, the chord after that)
        /// move it vertically away from the system until it does not.
        /// </summary>
        private void AdjustBarnumberVertically(float gap)
        {
			Voice barnumberVoice = this.Staves[0].Voices[0];
            Barline barline = null;
            DurationSymbol firstDSInVoice0 = null;
            DurationSymbol secondDSInVoice0 = null;
            foreach(NoteObject noteObject in barnumberVoice.NoteObjects)
            {
                if(barline == null)
                    barline = noteObject as Barline;
                if(firstDSInVoice0 == null)
                    firstDSInVoice0 = noteObject as DurationSymbol;
                else if(firstDSInVoice0 != null && secondDSInVoice0 == null)
                    secondDSInVoice0 = noteObject as DurationSymbol;
                if(barline != null && firstDSInVoice0 != null && secondDSInVoice0 != null)
                    break;
            }
            Debug.Assert(barline != null && firstDSInVoice0 != null); // secondDSInVoice0 can be null if there is only one DS!

            DurationSymbol firstDSInVoice1 = null;
            DurationSymbol secondDSInVoice1 = null;
            Staff topVisibleStaff = barnumberVoice.Staff;
            if(topVisibleStaff.Voices.Count == 2)
            {
                foreach(NoteObject noteObject in topVisibleStaff.Voices[1].NoteObjects)
                {
                    if(firstDSInVoice1 == null)
                        firstDSInVoice1 = noteObject as DurationSymbol;
                    else if(firstDSInVoice1 != null && secondDSInVoice1 == null)
                        secondDSInVoice1 = noteObject as DurationSymbol;

                    if(firstDSInVoice1 != null && secondDSInVoice1 != null)
                        break;
                }
                Debug.Assert(firstDSInVoice1 != null); // secondDSInVoice0 can be null if there is only one DS!
            }

            BarlineMetrics barlineMetrics = barline.Metrics as BarlineMetrics;
            BarnumberMetrics barnumberMetrics = barlineMetrics.BarnumberMetrics;

            if(barnumberMetrics != null)
            {
                RemoveCollision(barnumberMetrics, firstDSInVoice0, gap);
                if(firstDSInVoice0 is RestSymbol && secondDSInVoice0 != null)
                    RemoveCollision(barnumberMetrics, secondDSInVoice0, gap);

                if(topVisibleStaff.Voices.Count == 2)
                {
                    RemoveCollision(barnumberMetrics, firstDSInVoice1, gap);
                    if(firstDSInVoice1 is RestSymbol && secondDSInVoice1 != null)
                        RemoveCollision(barnumberMetrics, secondDSInVoice1, gap);
                    // the barnumber may now collide with the first DurationSymbol in Voice0...
                    RemoveCollision(barnumberMetrics, firstDSInVoice0, gap);
                    if(firstDSInVoice0 is RestSymbol && secondDSInVoice0 != null)
                        RemoveCollision(barnumberMetrics, secondDSInVoice0, gap);
                }
            }
        }

        /// <summary>
        /// If there is a collision between them, move the barnumber above the first duration symbol in the voice.
        /// </summary>
        /// <param name="barnumberMetrics"></param>
        /// <param name="durationSymbolMetrics"></param>
        /// <param name="gap"></param>
        private void RemoveCollision(BarnumberMetrics barnumberMetrics, DurationSymbol durationSymbol, float gap)
        {
			float verticalOverlap = 0F;
			if(durationSymbol.Metrics is ChordMetrics chordMetrics)
			{
				verticalOverlap = chordMetrics.OverlapHeight(barnumberMetrics, gap);
			}

			if(durationSymbol.Metrics is RestMetrics restMetrics)
            {
                verticalOverlap = restMetrics.OverlapHeight(barnumberMetrics, gap);
                if(verticalOverlap > 0)
                {
                    // fine tuning
                    // compare with the extra padding given to these symbols in the RestMetrics constructor.
                    switch(durationSymbol.DurationClass)
                    {
                        case DurationClass.breve:
                        case DurationClass.semibreve:
                        case DurationClass.minim:
                        case DurationClass.crotchet:
                        case DurationClass.quaver:
                        case DurationClass.semiquaver:
                            break;
                        case DurationClass.threeFlags:
                            verticalOverlap -= gap;
                            break;
                        case DurationClass.fourFlags:
                            verticalOverlap -= gap * 2F;
                            break;
                        case DurationClass.fiveFlags:
                            verticalOverlap -= gap * 2.5F;
                            break;
                    }
                }
            }
            if(verticalOverlap > 0)
                barnumberMetrics.Move(0F, -(verticalOverlap + gap));
        }

        private void AlignStaffnamesInLeftMargin(float leftMarginPos, float gap)
        {
            foreach(Staff staff in Staves)
            {
                foreach(NoteObject noteObject in staff.Voices[0].NoteObjects)
                {
                    if(noteObject is Barline firstBarline)
                    {
                        if(firstBarline.Metrics is BarlineMetrics barlineMetrics)
                        {
                            TextMetrics staffNameMetrics = barlineMetrics.StaffNameMetrics;
                            if(staffNameMetrics != null)
                            {
                                // The staffName is currently centred on the barline.
                                // Now move it into the centre of the left margin.
                                float alignX = leftMarginPos / 2F;
                                float deltaX = alignX - barlineMetrics.OriginX + gap;
                                staffNameMetrics.Move(deltaX, 0F);
                            }
						}
                        break;
                    }
                }
            }
        }

        private void ResetStaffMetricsBoundaries()
        {
            foreach(Staff staff in Staves)
            {
                if(staff.Metrics != null)
                {
                    staff.Metrics.ResetBoundary();
                }
            }
        }

        private void SetBarlineVisibility(List<bool> barlineContinuesDownList)
        {
            // set the visibility of all but the last barline
            for(int staffIndex = 0; staffIndex < Staves.Count; ++staffIndex)
            {
                Staff staff = Staves[staffIndex];
                Voice voice = staff.Voices[0];
                List<NoteObject> noteObjects = voice.NoteObjects;
                for(int i = 0; i < noteObjects.Count; ++i)
                {
                    bool isSingleStaffGroup =
                        (((staffIndex == 0) || !barlineContinuesDownList[staffIndex - 1]) // there is no grouped staff above
                            && (!barlineContinuesDownList[staffIndex])); // there is no grouped staff below 

                    if((noteObjects[i] is CautionaryChordSymbol)
                    && !isSingleStaffGroup)
                    {                           
                        Barline barline = noteObjects[i - 1] as Barline;
                        Debug.Assert(barline != null);
                        barline.IsVisible = false;
                    }
                }
            }
            // set the visibility of the last barline on this system
            SvgScore score = this.Score;
            SvgSystem nextSystem = null;
            for(int sysindex = 0; sysindex < this.Score.Systems.Count - 1; ++sysindex)
            {
                if(this == this.Score.Systems[sysindex])
                {
                    nextSystem = this.Score.Systems[sysindex + 1];
                    break;
                }
            }
            if(nextSystem != null)
            {
                for(int staffIndex = 0; staffIndex < Staves.Count; ++staffIndex)
                {
                    List<NoteObject> noteObjects = Staves[staffIndex].Voices[0].NoteObjects;
					DurationSymbol durationSymbol = nextSystem.Staves[staffIndex].Voices[0].FirstDurationSymbol;
					if (noteObjects[noteObjects.Count - 1] is Barline barline
					&& (durationSymbol is CautionaryChordSymbol))
					{
						barline.IsVisible = false;
					}
				}
            }
        }
        private void JustifyVertically(float pageWidth, float gap)
        {
            float stafflinesTop = Staves[0].Metrics.StafflinesTop;
            if(stafflinesTop != 0)
            {
                for(int i = 0; i < Staves.Count; ++i)
                {
                    Staff staff = Staves[i];
                    if(staff.Metrics != null)
                    {
                        staff.Metrics.Move(0, stafflinesTop * -1);
                    }
                }
            }

            Debug.Assert(Staves[0].Metrics.StafflinesTop == 0);
            for(int i = 1; i < Staves.Count; ++i)
            {
                if(Staves[i].Metrics != null)
                {
                    //BottomEdge bottomEdge = new BottomEdge(Staves[i - 1], 0F, pageWidth, gap);
                    BottomEdge bottomEdge = GetBottomEdge(i - 1, 0, pageWidth, gap);
                    if(bottomEdge != null)
                    {
                        TopEdge topEdge = new TopEdge(Staves[i], 0F, pageWidth);
                        float separation = topEdge.DistanceToEdgeAbove(bottomEdge);
                        float dy = gap - separation;
                        // limit stafflineHeight to multiples of pageFormat.Gap so that stafflines
                        // are not displayed as thick grey lines.
                        dy = dy - (dy % gap) + gap; // the minimum space bewteen stafflines is gap pixels.
                        if(dy > 0F)
                        {
                            for(int j = i; j < Staves.Count; ++j)
                            {
                                if(Staves[j].Metrics != null)
                                {
                                    Staves[j].Metrics.Move(0F, dy);
                                }
                            }
                            this.Metrics.StafflinesBottom += dy;
                        }
                    }
                }
            }

            this.Metrics = new SystemMetrics();
            foreach(Staff staff in Staves)
            {
                this.Metrics.Add(staff.Metrics);
            }
            Debug.Assert(this.Metrics.StafflinesTop == 0);
        }

        private BottomEdge GetBottomEdge(int upperStaffIndex, int topVisibleStaffIndex, float pageWidth, float gap)
        {
            BottomEdge bottomEdge = null;
            for(int i = upperStaffIndex; i >= topVisibleStaffIndex; --i)
            {
                Staff staff = Staves[i];
                if(staff.Metrics != null)
                {
                    bottomEdge = new BottomEdge(staff, 0F, pageWidth, gap);
                    break;
                }
            }
            return bottomEdge;
        }

        private TopEdge GetTopEdge(int lowerStaffIndex, float pageWidth)
        {
            TopEdge topEdge = null;
            for(int i = lowerStaffIndex; i < Staves.Count; ++i)
            {
                Staff staff = Staves[i];
                if(staff.Metrics != null)
                {
                    topEdge = new TopEdge(staff, 0F, pageWidth);
                    break;
                }
            }
            return topEdge;
        }
        #endregion

        public int AbsStartMsPosition;
        public int AbsEndMsPosition;

        public List<Staff> Staves = new List<Staff>();
        internal SystemMetrics Metrics = null;

		public SvgScore Score; // the containing score

	}
}
