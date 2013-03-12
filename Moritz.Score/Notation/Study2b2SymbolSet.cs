using System.Drawing;

using Moritz.Globals;
using Moritz.Score.Midi;

namespace Moritz.Score.Notation
{
    public class Study2b2SymbolSet : SymbolSet
    {
        /// <summary>
        /// This constructor can be called with both parameters null,
        /// just to get the overridden properties.
        /// </summary>
        public Study2b2SymbolSet()
            :base()
        {
        }

        /// <summary>
        /// Writes this score's SVG defs element
        /// </summary>
        /// <param name="w"></param>
        public override void WriteSymbolDefinitions(SvgWriter w)
        {
            w.SvgStartDefs(null);
            #region staff1Rect
            w.SvgRect("staff1Rect", 0, 0, 56, 64, "grey", 2, null, null);
            #endregion


            string[] staff1Colours = {"#FF6060","#FFAA44","#FFFF00","#99FF33","#33CC33","#00FF99",
                                      "#44DDDD","#11AAFF","#4477FF","#AA77FF","#FF22FF","#FF44AA"};
            float[] staff1dx ={17.6F, 17.6F, 19F, 
                               17.6F, 17.6F, 17.6F,
                               17.6F, 17.6F, 17.6F,
                                6.9F,  7.5F,  6.9F};

            for(int i = 1; i < 13; i++)
            {
                WriteStaffSymbolDef(w, 1, i, "#staff1Rect", staff1Colours[i - 1], staff1dx[i - 1], 44.8F);
            }

            #region staff2Circle
            w.WriteStartElement("circle");
            w.WriteAttributeString("id", "staff2Circle");
            //w.WriteAttributeString("cx", "-30");
            //w.WriteAttributeString("cy", "30");
            w.WriteAttributeString("cx", "30");
            w.WriteAttributeString("cy", "30");
            w.WriteAttributeString("r", "30");
            w.WriteAttributeString("stroke", "grey");
            w.WriteAttributeString("stroke-width", "2");
            w.WriteEndElement(); // ellipse
            #endregion

            string[] staff2Colours = { "#FF9595", "#FFC682", "#FFFF55", "#BBFF77", "#77DD77", "#55FFBB",
                                       "#82E8E8", "#60C6FF", "#82A4FF", "#C6A4FF", "#FF6BFF", "#FF82C6",};

            //float[] staff2dx = {-9.8F, -9.8F, -9F,
            //                    -10.4F, -10F,   -10.4F,
            //                    -10.4F, -10.4F, -10F,
            //                    -19.8F, -20F,   -21F};
            float[] staff2dx = {20.2F, 20.2F, 21F,
                                19.6F, 20F,   19.6F,
                                19.6F, 19.6F, 20F,
                                10.2F, 10F,   9F};

            for(int i = 1; i < 13; i++)
            {
                WriteStaffSymbolDef(w, 2, i, "#staff2Circle", staff2Colours[i - 1], staff2dx[i - 1], 42.8F);
            }

            #region staff3Hexagon
            w.WriteStartElement("path");
            w.WriteAttributeString("id", "staff3Hexagon");
            w.WriteAttributeString("stroke", "grey");
            w.WriteAttributeString("stroke-width", "2");
            w.WriteAttributeString("d", "M 32.332 0 60.331 16.166 60.331 48.497 32.332 64.664 4.332 48.497 4.332 16.166z");
            w.WriteEndElement(); // path
            #endregion

            string[] staff3Colours = { "#FFCACA", "#FFE2C0", "#FFFFAA", "#DDFFBB", "#BBEEBB", "#AAFFDD",
                                     "#C0F3F3","#AFE2FF","#C0D1FF","#E2D1FF","#FFB4FF","#FFC0E2" };

            float[] staff3dx = {21.9F, 21.9F, 21.9F,
                                   21.9F, 21.9F, 21.9F,
                                   21.9F, 21.9F, 21.9F,
                                   12F, 12.7F, 12F};

            for(int i = 1; i < 13; i++)
            {
                WriteStaffSymbolDef(w, 3, i, "#staff3Hexagon", staff3Colours[i - 1], staff3dx[i - 1], 45.15F);
            }

            w.SvgEndDefs(); // end of defs
        }

        private void WriteStaffSymbolDef(SvgWriter svgw, int staffNumber, int symbolNumber,
            string symbolShape, string colour,
            float textDX, float textDY)
        {
            string symbName = "symb" + staffNumber.ToString() + "_" + symbolNumber.ToString();

            svgw.WriteStartElement("g");
            svgw.WriteAttributeString("id", symbName);

            svgw.WriteStartElement("use");
            svgw.WriteAttributeString("xlink", "href", null, symbolShape);
            svgw.WriteAttributeString("fill", colour);
            svgw.WriteEndElement(); // use

            svgw.WriteStartElement("text");
            svgw.WriteAttributeString("dx", textDX.ToString(M.En_USNumberFormat));
            svgw.WriteAttributeString("dy", textDY.ToString(M.En_USNumberFormat));
            svgw.WriteAttributeString("align", "center");
            svgw.WriteAttributeString("font", "Courier New");
            svgw.WriteAttributeString("font-size", "40");
            svgw.WriteString(symbolNumber.ToString());
            svgw.WriteEndElement(); // text
            svgw.WriteEndElement(); // g
        }

        public override Metrics NoteObjectMetrics(Graphics graphics, NoteObject noteObject, VerticalDir voiceStemDirection, float gap, float strokeWidth)
        {
            Metrics returnMetrics = null;
            Barline barline = noteObject as Barline;
            Study2b2ChordSymbol study2b2ChordSymbol = noteObject as Study2b2ChordSymbol;
            RestSymbol rest = noteObject as RestSymbol;
            if(barline != null)
            {
                returnMetrics = new BarlineMetrics(graphics, barline, gap);
            }
            else if(study2b2ChordSymbol != null)
            {
                study2b2ChordSymbol.BeamBlock = null;
                Study2b2ChordMetrics metrics = new Study2b2ChordMetrics(study2b2ChordSymbol);
                returnMetrics = metrics;
            }
            else if(rest != null)
            {
                returnMetrics = new RestMetrics(graphics, rest, gap, noteObject.Voice.Staff.NumberOfStafflines, strokeWidth);
            }

            return returnMetrics;
        }

        public override DurationSymbol GetDurationSymbol(Voice voice, LocalizedMidiDurationDef lmdd, bool firstLmddInVoice,
            ref byte currentVelocity)
        {
            DurationSymbol durationSymbol = null;
            MidiChordDef midiChordDef = lmdd.MidiChordDef;
            PageFormat pageFormat = voice.Staff.SVGSystem.Score.PageFormat;
            float musicFontHeight = pageFormat.MusicFontHeight;
            int minimumCrotchetDuration = pageFormat.MinimumCrotchetDuration;

            if(midiChordDef != null)
            {
                Study2b2ChordSymbol study2b2ChordSymbol = new Study2b2ChordSymbol(voice, lmdd, minimumCrotchetDuration, musicFontHeight);

                if(midiChordDef.MidiVelocitySymbol != currentVelocity)
                {
                    currentVelocity = midiChordDef.MidiVelocitySymbol;
                }

                study2b2ChordSymbol.FontHeight = study2b2ChordSymbol.FontHeight * (0.5F + (currentVelocity / 200F));

                durationSymbol = study2b2ChordSymbol;
            }
            else
            {
                RestSymbol restSymbol = new RestSymbol(voice, lmdd, minimumCrotchetDuration, musicFontHeight);
                durationSymbol = restSymbol;
            }

            return durationSymbol;
        }

        public override void WriteJavaScriptDefinitions(SvgWriter w)
        {
            //StringBuilder functions = new StringBuilder();
            //functions.Append(GetBBoxFromRect());
            //functions.Append("\n");
            //functions.Append(AddRectFrame());
            //w.WriteStartElement("script");
            //w.WriteAttributeString("type", "text/ecmascript");
            //w.WriteString(functions.ToString());
            //w.WriteEndElement();
        }
        #region JavaScript function definitions (commented out)
        //private StringBuilder GetBBoxFromRect()
        //{
        //    StringBuilder fn = new StringBuilder();
        //    fn.Append("function getBBoxFromRect (client_rect, hOffset_px, vOffset_px)\n");
        //    fn.Append("{\n");
        //    fn.Append("if (! hOffset_px) {hOffset_px=0;}\n");
        //    fn.Append("if (! vOffset_px) {vOffset_px=0;}\n");
        //    fn.Append("var box = document.createElementNS( document.rootElement.namespaceURI, 'rect' );\n");

        //    fn.Append("if (client_rect)\n");
        //    fn.Append("{\n");
        //    fn.Append("box.setAttribute('x', client_rect.left - hOffset_px);\\n");
        //    fn.Append("box.setAttribute('y', client_rect.top - vOffset_px);\n");
        //    fn.Append("box.setAttribute('width', client_rect.width + hOffset_px * 2);\n");
        //    fn.Append("box.setAttribute('height', client_rect.height + vOffset_px * 2);\n");
        //    fn.Append("}\n");
        //    fn.Append("return box;\n");
        //    fn.Append("}\n");
        //    return fn;
        //}
        //private StringBuilder AddRectFrame()
        //{
        //    StringBuilder fn = new StringBuilder();
        //    fn.Append("function addRectFrame (elem_id, strokeWidth, hPadding, vPadding)\n");
        //    fn.Append("{\n");
        //    fn.Append("var elem = document.getElementById(elem_id);\n");
        //    fn.Append("if (elem)\n");
        //    fn.Append("{\n");
        //    fn.Append("var f = elem.getClientRects();\n");
        //    fn.Append("if (f)\n");
        //    fn.Append("{\n");
        //    fn.Append("var bbox = getBBoxFromRect(f[0], hPadding, vPadding);\n");
        //    fn.Append("bbox.setAttribute(\n");
        //    fn.Append("'style',\n");
        //    fn.Append("'fill: none;'+\n");
        //    fn.Append("'stroke: black;'+\n");
        //    fn.Append("'stroke-width: ' + strokeWidth + 'px;'\n");
        //    fn.Append(");\n");
        //    fn.Append("elem.parentNode.appendChild(bbox);\n");
        //    fn.Append("}\n");
        //    fn.Append("}\n");
        //    fn.Append("}\n");
        //    return fn;
        //}
        #endregion JavaScript function definitions

    }
}
