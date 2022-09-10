using Moritz.Globals;
using Moritz.Xml;

using System;
using System.Collections.Generic;
using System.Diagnostics;

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

        public void WriteSVG(SvgWriter w, int channel, int msDuration, CarryMsgs carryMsgs)
        {
            if(carryMsgs.IsStartOfEnvs)
            {
                PanMsbs = GetDefault(PanMsbs, 64);
                ModulationWheelMsbs = GetDefault(ModulationWheelMsbs, 0);
                ExpressionMsbs = GetDefault(ExpressionMsbs, 127);
                PitchWheelMsbs = GetDefault(PitchWheelMsbs, 64);

                carryMsgs.IsStartOfEnvs = false;
            }

            if(DoWriteControl(PanMsbs, carryMsgs.PanState)
            || DoWriteControl(ModulationWheelMsbs, carryMsgs.ModWheelState)
            || DoWriteControl(ExpressionMsbs, carryMsgs.ExpressionState)
            || DoWriteControl(PitchWheelMsbs, carryMsgs.PitchWheelState))
            {
                w.WriteStartElement("envs"); // envelopes

                if(DoWriteControl(PanMsbs, carryMsgs.PanState))
                {
                    carryMsgs.PanState = WriteCCEnv(w, channel, M.CTL_PAN_10, PanMsbs, msDuration);
                }

                if(DoWriteControl(ModulationWheelMsbs, carryMsgs.ModWheelState))
                {
                    carryMsgs.ModWheelState = WriteCCEnv(w, channel, M.CTL_MODWHEEL_1, ModulationWheelMsbs, msDuration);
                }

                if(DoWriteControl(ExpressionMsbs, carryMsgs.ExpressionState))
                {
                    carryMsgs.ExpressionState = WriteCCEnv(w, channel, M.CTL_EXPRESSION_11, ExpressionMsbs, msDuration);
                }

                if(DoWriteControl(PitchWheelMsbs, carryMsgs.PitchWheelState))
                {
                    string statusString = null;
                    w.WriteStartElement("env"); // envelope

                    statusString = $"0x{(M.CMD_PITCH_WHEEL_0xE0 + channel).ToString("X")}";
                    w.WriteAttributeString("s", statusString);

                    carryMsgs.PitchWheelState = WriteD1AndD2VTs(w, PitchWheelMsbs, PitchWheelMsbs, msDuration);

                    w.WriteEndElement(); // end env
                }

                w.WriteEndElement(); // end envs
            }
        }

        /// <summary>
        /// If the ctlValues are null or empty,
        /// returns a List of ctlValues containg the single defaultCtlState.
        /// </summary>
        /// <returns></returns>
        private List<byte> GetDefault(List<byte> ctlValues, int defaultCtlState)
        {
            if(ctlValues == null || ctlValues.Count == 0)
            {
                ctlValues = new List<byte>
                {
                    (byte)defaultCtlState
                };
            }
            return ctlValues;
        }

        private bool DoWriteControl(List<byte> ctlValues, byte currentCtlState)
        {
            if(ctlValues != null && ((ctlValues.Count == 1 && ctlValues[0] != currentCtlState) || ctlValues.Count > 1))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// writes the env element for a normal continuous controller
        /// </summary>
        /// <param name="w">SvgWriter</param>
        /// <param name="channel">The channel</param>
        /// <param name="d1">The controller number</param>
        /// <param name="d2s">The controller values</param>
        /// <param name="msDuration">The total duration of the envelope</param>
        /// <returns>The last controller value</returns>
        private byte WriteCCEnv(SvgWriter w, int channel, int d1, List<byte> d2s, int msDuration)
        {
            string statusString = $"0x{(M.CMD_CONTROL_CHANGE_0xB0 + channel).ToString("X")}"; ;
            w.WriteStartElement("env"); // envelope
            w.WriteAttributeString("s", statusString);
            w.WriteAttributeString("d1", d1.ToString());
            byte lastControllerValue = WriteD2VTs(w, d2s, msDuration);
            w.WriteEndElement(); // end env

            return lastControllerValue;
        }

        /// <summary>
        /// Moritz envelopes distribute the d2s over the whole msDuration,
        /// With the first d2 at position 0, the last d2 at msduration - 1.
        /// </summary>
        /// <param name="d2s"></param>
        /// <param name="msDuration"></param>
        /// <returns>The last controller value</returns>
        private byte WriteD2VTs(SvgWriter w, List<byte> d2s, int msDuration)
        {
            byte lastControllerValue = 0; // will always be changed
            List<int> msDurs = GetMsDurs(d2s.Count, msDuration);
            Tuple<List<int>, List<byte>> rval = Agglommerate(msDurs, d2s);
            msDurs = rval.Item1;
            List<byte> agglommeratedD2s = rval.Item2;
            for(int i = 0; i < msDurs.Count; ++i)
            {
                Debug.Assert(msDurs[i] > 0, "Moritz never writes controller values that would have to be carried to the next moment.");
                w.WriteStartElement("vt"); // envelope
                w.WriteAttributeString("d2", agglommeratedD2s[i].ToString());
                w.WriteAttributeString("msDur", msDurs[i].ToString());
                w.WriteEndElement(); // end vt

                lastControllerValue = d2s[i];
            }
            return lastControllerValue;
        }

        private Tuple<List<int>, List<byte>> Agglommerate(List<int> msDurs, List<byte> d2s)
        {
            Debug.Assert(msDurs.Count == d2s.Count);

            List<byte> rD2s = new List<byte>(d2s);

            for(int i = rD2s.Count - 1; i > 0; --i)
            {
                if(rD2s[i] == rD2s[i - 1])
                {
                    msDurs[i - 1] += msDurs[i];
                    msDurs.RemoveAt(i);
                    rD2s.RemoveAt(i);
                }
            }
            return new Tuple<List<int>, List<byte>>(msDurs, rD2s);
        }

        private Tuple<List<int>, List<byte>, List<byte>> Agglommerate(List<int> msDurs, List<byte> d1s, List<byte> d2s)
        {
            Debug.Assert(msDurs.Count == d1s.Count && msDurs.Count == d2s.Count);

            List<byte> rD1s = new List<byte>(d1s);
            List<byte> rD2s = new List<byte>(d2s);

            for(int i = rD1s.Count - 1; i > 0; --i)
            {
                if(rD1s[i] == rD1s[i - 1] && rD2s[i] == rD2s[i - 1])
                {
                    msDurs[i - 1] += msDurs[i];
                    msDurs.RemoveAt(i);
                    rD1s.RemoveAt(i);
                    rD2s.RemoveAt(i);
                }
            }
            return new Tuple<List<int>, List<byte>, List<byte>>(msDurs, rD1s, rD2s);
        }

        /// <summary>
        /// Moritz envelopes distribute the d2s over the whole msDuration,
        /// With the first d2 at position 0, the last d2 at msduration - 1.
        /// This function writes both d1 and d2
        /// </summary>
        /// <returns>The last controller value</returns>
        private byte WriteD1AndD2VTs(SvgWriter w, List<byte> d1s, List<byte> d2s, int msDuration)
        {
            Debug.Assert(d1s.Count == d2s.Count);
            byte lastControllerValue = 0; // will always be changed
            List<int> msDurs = GetMsDurs(d1s.Count, msDuration);
            Tuple<List<int>, List<byte>, List<byte>> rVals = Agglommerate(msDurs, d1s, d2s);
            msDurs = rVals.Item1;
            List<byte> rD1s = rVals.Item2;
            List<byte> rD2s = rVals.Item3;
            for(int i = 0; i < msDurs.Count; ++i)
            {
                Debug.Assert(msDurs[i] > 0, "Moritz never writes controller values that would have to be carried to the next moment.");
                w.WriteStartElement("vt"); // envelope
                w.WriteAttributeString("d1", rD1s[i].ToString());
                w.WriteAttributeString("d2", rD2s[i].ToString());
                w.WriteAttributeString("msDur", msDurs[i].ToString());
                w.WriteEndElement(); // end vt

                lastControllerValue = d2s[i];
            }

            return lastControllerValue;
        }

        private List<int> GetMsDurs(int count, int msDuration)
        {
            var msDurs = new List<int>();
            switch(count)
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
