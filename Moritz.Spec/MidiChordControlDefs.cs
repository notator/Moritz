using Moritz.Globals;
using Moritz.Xml;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Spec
{
    public class MidiChordControlDefs
    {
        /// <summary>
        /// Used by the Assistant Composer's palettes
        /// </summary>
        public MidiChordControlDefs()
        {
        }

        internal MidiChordControlDefs Clone()
        {
            var rval = new MidiChordControlDefs
            {
                Preset = this.Preset,
                PitchWheel = this.PitchWheel,
                Bank = this.Bank,
                ModWheel = this.ModWheel,
                Volume = this.Volume,
                Pan = this.Pan,
                Expression = this.Expression,
                PitchWheelSensitivity = this.PitchWheelSensitivity,
                Mixture = this.Mixture,
                TuningGroup = this.TuningGroup,
                Tuning = this.Tuning,
                OrnamentGroup = this.OrnamentGroup,
                Ornament = this.Ornament,
                SemitoneOffset = this.SemitoneOffset,
                CentOffset = this.CentOffset,
                VelocityPitchSensitivity = this.VelocityPitchSensitivity,
                Reverberation = this.Reverberation,
                AllSoundOff = this.AllSoundOff,
                AllControllersOff = this.AllControllersOff
            };

            return rval;
        }

        public void WriteSVG(SvgWriter w, int channel)
        {
            w.WriteStartElement("controls");
            if(Bank != null)
            {
                MidiMsg msg = ControlMessage(channel, (int)M.CTL.BANK_0, (int)Bank);
                msg.WriteSVG(w);
            }
            if(Preset != null)
            {
                int status = (int)(int)M.CMD.PRESET_192 + channel;
                MidiMsg msg = new MidiMsg(status, (int)Preset);
                msg.WriteSVG(w);
            }
            if(PitchWheel != null)
            {
                int status = (int)M.CMD.PITCH_WHEEL_224 + channel;
                int value = (int)PitchWheel + 64;
                MidiMsg msg = new MidiMsg(status, value, value);
                msg.WriteSVG(w);
            }
            if(ModWheel != null)
            {
                MidiMsg msg = ControlMessage(channel, (int)M.CTL.MOD_WHEEL_1, (int)ModWheel);
                msg.WriteSVG(w);
            }
            if(Volume != null)
            {
                MidiMsg msg = ControlMessage(channel, (int)M.CTL.VOLUME_7, (int)Volume);
                msg.WriteSVG(w);
            }
            if(Pan != null)
            {
                MidiMsg msg = ControlMessage(channel, (int)M.CTL.PAN_10, (int)Pan);
                msg.WriteSVG(w);
            }
            if(Expression != null)
            {
                MidiMsg msg = ControlMessage(channel, (int)M.CTL.EXPRESSION_11, (int)Expression);
                msg.WriteSVG(w);
            }
            if(PitchWheelSensitivity != null)
            {
                MidiMsg msg = ControlMessage(channel, (int)M.CTL.PITCH_WHEEL_SENSITIVITY_16, (int)PitchWheelSensitivity);
                msg.WriteSVG(w);
            }
            if(Mixture != null)
            {
                MidiMsg msg = ControlMessage(channel, (int)M.CTL.MIXTURE_17, (int)Mixture);
                msg.WriteSVG(w);
            }
            if(TuningGroup != null)
            {
                MidiMsg msg = ControlMessage(channel, (int)M.CTL.TUNING_GROUP_18, (int)TuningGroup);
                msg.WriteSVG(w);
            }
            if(Tuning != null)
            {
                MidiMsg msg = ControlMessage(channel, (int)M.CTL.TUNING_19, (int)Tuning);
                msg.WriteSVG(w);
            }
            if(OrnamentGroup != null)
            {
                MidiMsg msg = ControlMessage(channel, (int)M.CTL.ORNAMENT_GROUP_75, (int)OrnamentGroup);
                msg.WriteSVG(w);
            }
            if(Ornament != null)
            {
                MidiMsg msg = ControlMessage(channel, (int)M.CTL.ORNAMENT_76, (int)Ornament);
                msg.WriteSVG(w);
            }
            if(SemitoneOffset != null)
            {
                MidiMsg msg = ControlMessage(channel, (int)M.CTL.SEMITONE_OFFSET_80, (int)SemitoneOffset + 64);
                msg.WriteSVG(w);
            }
            if(CentOffset != null)
            {
                MidiMsg msg = ControlMessage(channel, (int)M.CTL.SEMITONE_OFFSET_80, (int)CentOffset + 64);
                msg.WriteSVG(w);
            }
            if(VelocityPitchSensitivity != null)
            {
                MidiMsg msg = ControlMessage(channel, (int)M.CTL.SEMITONE_OFFSET_80, (int)VelocityPitchSensitivity);
                msg.WriteSVG(w);
            }
            if(Reverberation != null)
            {
                MidiMsg msg = ControlMessage(channel, (int)M.CTL.REVERBERATION_91, (int)Reverberation);
                msg.WriteSVG(w);
            }
            if(AllSoundOff)
            {
                MidiMsg msg = ControlMessage(channel, (int)M.CTL.ALL_SOUND_OFF_120, 0);
                msg.WriteSVG(w);
            }
            if(AllControllersOff)
            {
                MidiMsg msg = ControlMessage(channel, (int)M.CTL.ALL_CONTROLLERS_OFF_121, 0);
                msg.WriteSVG(w);
            }
            w.WriteEndElement(); // controls
        }

        private MidiMsg ControlMessage(int channel, int controlType, int value)
        {
            int status = (int)M.CMD.CONTROL_CHANGE_176 + channel;
            MidiMsg msg = new MidiMsg(status, controlType, value);

            return msg;
        }

        /// <summary>
        /// If the ctlValues are null or empty,
        /// returns a List of ctlValues containing the single defaultCtlState.
        /// </summary>
        /// <returns></returns>
        private List<byte> GetValues(List<byte> ctlValues, int defaultCtlState)
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

        /// <summary>
        /// If the ctlValue is null or empty, returns the defaultCtlValue.
        /// </summary>
        /// <returns></returns>
        private byte GetValue(byte? ctlValue, byte defaultCtlValue)
        {
            if(ctlValue == null)
            {
                return defaultCtlValue;
            }
            else
            {
                return (byte) ctlValue;
            }
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
            string statusString = $"0x{(M.CMD.CONTROL_CHANGE_176 + channel).ToString("X")}"; ;
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



        #region ResidentSynth controls
        // These messages are only sent if their value is not null.
        /// <summary>
        /// Range 0..127
        /// </summary>
        public sbyte? Preset { get; set; } = null;
        /// <summary>
        /// Range -64..63 
        /// </summary>
        public sbyte? PitchWheel { get; set; } = null;
        /// <summary>
        /// Range 0.. nBanks 
        /// </summary>
        public sbyte? Bank { get; set; } = null;
        /// <summary>
        /// Range 0..127
        /// </summary>
        public sbyte? ModWheel { get; set; } = null;
        /// <summary>
        /// Range 0..127 
        /// </summary>
        public sbyte? Volume { get; set; } = null;
        /// <summary>
        /// Range 0..127 
        /// </summary>
        public sbyte? Pan { get; set; } = null;
        /// <summary>
        /// Range 0..127
        /// </summary>
        public sbyte? Expression { get; set; } = null;
        /// <summary>
        /// Range 0..127 (non-standard midi control)
        /// </summary>
        public sbyte? PitchWheelSensitivity { get; set; } = null;
        /// <summary>
        /// Range 0..nMixtures (non-standard midi control)
        /// </summary>
        public sbyte? Mixture { get; set; } = null;
        /// <summary>
        /// Range 0..nTuningGroups (non-standard midi control) 
        /// </summary>
        public sbyte? TuningGroup { get; set; } = null;
        /// <summary>
        /// Range 0..nTunings (non-standard midi control)  
        /// </summary>
        public sbyte? Tuning { get; set; } = null;
        /// <summary>
        /// Range 0..nOrnamentGroups (non-standard midi control) 
        /// <para>(TODO in ResidentSynth and ResidentSynthHost)</para>
        /// </summary>
        public sbyte? OrnamentGroup { get; set; } = null;
        /// <summary>
        /// Range 0..nOrnaments (non-standard midi control) 
        /// </summary>
        public sbyte? Ornament { get; set; } = null;
        /// <summary>
        /// Range -64..63 (non-standard midi control)  
        /// </summary>
        public sbyte? SemitoneOffset { get; set; } = null;
        /// <summary>
        /// Range -64..63 (non-standard midi control)  
        /// </summary>
        public sbyte? CentOffset { get; set; } = null;
        /// <summary>
        /// Range 0..127 (non-standard midi control) 
        /// </summary>
        public sbyte? VelocityPitchSensitivity { get; set; } = null;
        /// <summary>
        /// Range 0..127 (non-standard midi control)  
        /// </summary>
        public sbyte? Reverberation { get; set; } = null; // non-standard control
        public bool AllSoundOff { get; set; } = false;
        public bool AllControllersOff { get; set; } = false;
        #endregion 
    }
}
