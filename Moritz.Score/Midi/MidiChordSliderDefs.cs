using System.Collections.Generic;
using System.Xml;
using System.Diagnostics;

using Moritz.Globals;

namespace Moritz.Score.Midi
{
    public class MidiChordSliderDefs
    {
        /// <summary>
        /// Used by Assistant Composer
        /// </summary>
        public MidiChordSliderDefs(List<byte> pitchWheelMsbs, List<byte> panMsbs, List<byte> modulationWheelMsbs, List<byte> expressionMsbs)
        {
            PitchWheelMsbs = pitchWheelMsbs;
            PanMsbs = panMsbs;
            ModulationWheelMsbs = modulationWheelMsbs;
            ExpressionMsbs = expressionMsbs;
        }
        /// <summary>
        /// Contains values retrieved from an SVG file midiSliders element
        /// </summary>
        /// <param name="score"></param>
        public MidiChordSliderDefs(XmlReader r)
        {
            // The reader is at the beginning of a "score:sliders" element having attributes
            Debug.Assert((r.Name == "score:sliders") && r.IsStartElement() && r.AttributeCount > 0);
            int nAttributes = r.AttributeCount;
            for(int i = 0; i < nAttributes; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                     case "pitchWheel":
                        PitchWheelMsbs = M.StringToByteList(r.Value, ' ');
                        break;
                     case "pan":
                        PanMsbs = M.StringToByteList(r.Value, ' ');
                        break;
                     case "modulationWheel":
                        ModulationWheelMsbs = M.StringToByteList(r.Value, ' ');
                        break;
                     case "expressionSlider":
                        ExpressionMsbs = M.StringToByteList(r.Value, ' ');
                        break;
                }
            }
        }

        public void WriteSVG(SvgWriter w)
        {
            w.WriteStartElement("score", "sliders", null);

            if(PitchWheelMsbs != null && PitchWheelMsbs.Count > 0 
                && !(PitchWheelMsbs.Count == 1 && PitchWheelMsbs[0] == M.DefaultPitchWheel))
                w.WriteAttributeString("pitchWheel", M.ByteListToString(PitchWheelMsbs));
            if(PanMsbs != null && PanMsbs.Count > 0
                && !(PanMsbs.Count == 1 && PanMsbs[0] == M.DefaultPan))
                w.WriteAttributeString("pan", M.ByteListToString(PanMsbs));
            if(ModulationWheelMsbs != null && ModulationWheelMsbs.Count > 0
                && !(ModulationWheelMsbs.Count == 1 && ModulationWheelMsbs[0] == M.DefaultModulationWheel))
                w.WriteAttributeString("modulationWheel", M.ByteListToString(ModulationWheelMsbs));
            if(ExpressionMsbs != null && ExpressionMsbs.Count > 0
                && !(ExpressionMsbs.Count == 1 && ExpressionMsbs[0] == M.DefaultExpression))
                w.WriteAttributeString("expressionSlider", M.ByteListToString(ExpressionMsbs));

            w.WriteEndElement();
        }

        /// <summary>
        /// The PitchWheel Msb values corresonding to the percentages set in the panel.
        /// This can be set to a single PitchWheelSlider Msb value.
        /// </summary>
        public List<byte> PitchWheelMsbs = null;
        /// <summary>
        /// The Pan Msb values corresonding to the percentages set in the panel.
        /// This can be set to a single PanSlider Msb value.
        /// </summary>
        public List<byte> PanMsbs = null;
        /// <summary>
        /// The ModulationWheel Msb values corresonding to the percentages set in the panel.
        /// This can be set to a single ModulationWheelSlider Msb value.
        /// </summary>
        public List<byte> ModulationWheelMsbs = null;
        /// <summary>
        /// The Expression Msb values corresonding to the percentages set in the panel.
        /// This can be set to a single Expression Msb value.
        /// </summary>
        public List<byte> ExpressionMsbs = null;
    }
}
