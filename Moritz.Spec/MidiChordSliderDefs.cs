using System.Collections.Generic;
using System.Xml;
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Globals;
using Moritz.Xml;
using System;

namespace Moritz.Spec
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

        public void WriteSVG(SvgWriter w, int channel, int msDuration)
        {
            w.WriteStartElement("envs"); // envelopes

            if(PanMsbs != null && PanMsbs.Count > 0)
            {
                WriteCCEnv(w, channel, M.CTL_PAN_10, PanMsbs, msDuration);
            }

            if(ModulationWheelMsbs != null && ModulationWheelMsbs.Count > 0)
            {
                WriteCCEnv(w, channel, M.CTL_MODWHEEL_1, ModulationWheelMsbs, msDuration);
            }
            if(ExpressionMsbs != null && ExpressionMsbs.Count > 0)
            {
                WriteCCEnv(w, channel, M.CTL_EXPRESSION_11, ExpressionMsbs, msDuration);
            }

            if(PitchWheelMsbs != null && PitchWheelMsbs.Count > 0)
            {
                string statusString = null;
                w.WriteStartElement("env"); // envelope

                statusString = $"0x{(M.CMD_PITCH_WHEEL_0xE0 + channel).ToString("X")}";
                w.WriteAttributeString("s", statusString);

                WriteD1AndD2VTs(w, PitchWheelMsbs, PitchWheelMsbs, msDuration);

                w.WriteEndElement(); // end env
            }

            w.WriteEndElement(); // end envs
        }

        /// <summary>
        /// writes the env element for a normal continuous controller
        /// </summary>
        /// <param name="w">SvgWriter</param>
        /// <param name="channel">The channel</param>
        /// <param name="d1">The controller number</param>
        /// <param name="d2s">The controller values</param>
        /// <param name="msDuration">The total duration of the envelope</param>
        private void WriteCCEnv(SvgWriter w, int channel, int d1, List<byte> d2s, int msDuration)
        {
            string statusString = $"0x{(M.CMD_CONTROL_CHANGE_0xB0 + channel).ToString("X")}"; ;
            w.WriteStartElement("env"); // envelope
            w.WriteAttributeString("s", statusString);
            w.WriteAttributeString("d1", d1.ToString());
            WriteD2VTs(w, d2s, msDuration);
            w.WriteEndElement(); // end env
        }

        /// <summary>
        /// Moritz envelopes disribute the d2s over the whole msDuration,
        /// With the first d2 at position 0, the last d2 at msduration - 1.
        /// </summary>
        /// <param name="d2s"></param>
        /// <param name="msDuration"></param>
        private void WriteD2VTs(SvgWriter w, List<byte> d2s, int msDuration)
        {
            List<int> msDurs = GetMsDurs(d2s.Count, msDuration);
            for(int i = 0; i < msDurs.Count; ++i)
            {
                w.WriteStartElement("vt"); // envelope
                w.WriteAttributeString("d2", d2s[i].ToString());
                w.WriteAttributeString("msDur", msDurs[i].ToString());
                w.WriteEndElement(); // end vt
            }
        }

        private void WriteD1AndD2VTs(SvgWriter w, List<byte> d1s, List<byte> d2s, int msDuration)
        {
            Debug.Assert(d1s.Count == d2s.Count);
            List<int> msDurs = GetMsDurs(d1s.Count, msDuration);
            for(int i = 0; i < msDurs.Count; ++i)
            {
                w.WriteStartElement("vt"); // envelope
                w.WriteAttributeString("d1", d1s[i].ToString());
                w.WriteAttributeString("d2", d2s[i].ToString());
                w.WriteAttributeString("msDur", msDurs[i].ToString());
                w.WriteEndElement(); // end vt
            }
        }

        private List<int> GetMsDurs(int count, int msDuration)
        {
            var msDurs = new List<int>();
            switch (count)
            {
                case 1:
                    msDurs.Add(msDuration);
                    break;
                case 2:
                    Debug.Assert(msDuration > 1);
                    msDurs.Add(msDuration - 1);
                    msDurs.Add(1);
                    break;
                default:
                    Debug.Assert(msDuration > 2);
                    float fDuration = ((float)msDuration - 1) / (count - 1);
                    List<int> iPositions = new List<int>();
                    float fPosition = 0;
                    for(int i = 0; i < (count - 1); i++)
                    {
                        iPositions.Add((int)Math.Round(fPosition));
                        fPosition += fDuration;
                    }
                    iPositions.Add(msDuration - 1);
                    iPositions.Add(msDuration);
                    for(int i = 1; i < iPositions.Count; ++i)
                    {
                        msDurs.Add(iPositions[i] - iPositions[i - 1]);
                    }
                    break;
            }
            return msDurs;
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
