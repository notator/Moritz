using System;
using System.Collections.Generic;
using System.Xml;
using System.Diagnostics;

using Moritz.Globals;

namespace Moritz.Spec
{
    public class BasicMidiChordDef
    {
        public BasicMidiChordDef()
        {
        }

        public BasicMidiChordDef(int msDuration, byte? bank, byte? patch, bool hasChordOff, List<byte> pitches, List<byte> velocities)
        {
            _msDuration = msDuration; // read-only!
            BankIndex = bank;
            PatchIndex = patch;
            HasChordOff = hasChordOff;
            Pitches = new List<byte>(pitches);
            Velocities = new List<byte>(velocities);

            #region check values
            Debug.Assert(_msDuration > 0, "msDuration out of range");
            Debug.Assert(BankIndex == null || BankIndex == M.MidiValue((int)BankIndex), "Bank out of range.");
            Debug.Assert(PatchIndex == null || PatchIndex == M.MidiValue((int)PatchIndex), "Patch out of range.");
            Debug.Assert(Pitches.Count == Velocities.Count, "There must be the same number of pitches and velocities.");
            foreach(byte pitch in Pitches)
                Debug.Assert(pitch >= 0 && pitch <= 127);
            foreach(byte velocity in Velocities)
                AssertIsVelocityValue(velocity);     
            #endregion
        }

        /// <summary>
        /// A BasicMidiChordDef having density notes. Absent fields are set to 0 or null.
        /// Note that the number of pitches returned can be less than nPitches. Pitches that would be higher than 127 are
        /// simply not added to the returned list.
        /// All pitches are given velocity = 127.
        /// The pitches are found using the function gamut.GetChord(rootPitch, density). See that function for further documentation.
        /// </summary>
        /// <param name="msDuration">The duration</param>
        /// <param name="gamut"></param>
        /// <param name="rootPitch">The lowest pitch</param>
        /// <param name="density">The number of pitches. The actual number created can be smaller.</param>
        public BasicMidiChordDef(int msDuration, Gamut gamut, int rootPitch, int density)
        {
            #region conditions
            Debug.Assert(density > 0 && density <= 12);
            Debug.Assert(rootPitch >= 0 && rootPitch <= 127);
            Debug.Assert(msDuration > 0);
            #endregion conditions

            _msDuration = msDuration; // read-only!

            Pitches = gamut.GetChord(rootPitch, density);
            var newVelocities = new List<byte>();
            foreach(byte pitch in Pitches) // can be less than nPitchesPerChord
            {
                newVelocities.Add(127);
            }
            Velocities = newVelocities;
        }

        #region Inversion
        /// <summary>
        /// Creates a BasicMidiChordDef having the original base pitch,
        /// but in which the top-bottom order of the prime intervals is reversed. 
        /// Velocities remain in the same order, bottom to top. They are not inverted. 
        /// </summary>
        /// <returns></returns>
        public BasicMidiChordDef Inversion()
        {
            List<byte> pitches = Pitches; // default if Pitches.Count == 1

            if(Pitches.Count > 1)
            {
                List<byte> intervals = new List<byte>();

                for(int i = 1; i < Pitches.Count; ++i)
                {
                    intervals.Add((byte)(Pitches[i] - Pitches[i - 1]));
                }
                intervals.Reverse();
                pitches = new List<byte>() { Pitches[0] };
                for(int i = 0; i < intervals.Count; ++i)
                {
                    byte interval = intervals[i];
                    pitches.Add((byte)(pitches[pitches.Count - 1] + interval));
                }
            }

            BasicMidiChordDef invertedBMCD = new BasicMidiChordDef(_msDuration, BankIndex, PatchIndex, HasChordOff, pitches, Velocities);

            return invertedBMCD;
        }
        #endregion Inversion

        /// <summary>
        /// Writes a single moment element which may contain
        /// NoteOffs, bank, patch, pitchWheelDeviation, NoteOns 
        /// </summary>
        public void WriteSVG(XmlWriter w, int channel, CarryMsgs carryMsgs)
        {
            w.WriteStartElement("moment");
            w.WriteAttributeString("msDuration", _msDuration.ToString());

            if(carryMsgs.Count > 0)
            {
                carryMsgs.WriteSVG(w);
                carryMsgs.Clear();
            }

            if(BankIndex != null || PatchIndex != null || PitchWheelDeviation != null)
            {
                w.WriteStartElement("switches");
                if(BankIndex != null)
                {
                    MidiMsg msg = new MidiMsg(M.CMD_CONTROL_CHANGE_0xB0 + channel, M.CTL_BANK_CHANGE_0, BankIndex);
                    msg.WriteSVG(w);
                }
                if(PatchIndex != null)
                {
                    MidiMsg msg = new MidiMsg(M.CMD_PATCH_CHANGE_0xC0 + channel, (int)PatchIndex, null);
                    msg.WriteSVG(w);
                }
                if(PitchWheelDeviation != null)
                {
                    List<MidiMsg> pwdMessages = GetPitchWheelDeviationMessages(channel, (int) PitchWheelDeviation);
                    foreach(MidiMsg msg in pwdMessages)
                    {
                        msg.WriteSVG(w);
                    }
                }
                w.WriteEndElement(); // switches
            }

            if(Pitches != null)
            {
                Debug.Assert(Velocities != null && Pitches.Count == Velocities.Count);
                w.WriteStartElement("noteOns");
                int status = M.CMD_NOTE_ON_0x90 + channel; // NoteOn
                for(int i = 0; i < Pitches.Count; ++i)
                {
                    MidiMsg msg = new MidiMsg(status, Pitches[i], Velocities[i]);
                    msg.WriteSVG(w);
                }
                w.WriteEndElement(); // end of noteOns

                if(HasChordOff)
                {
                    status = M.CMD_NOTE_OFF_0x80 + channel;
                    int data2 = M.DEFAULT_NOTEOFF_VELOCITY_64;
                    foreach(byte pitch in Pitches)
                    {
                        carryMsgs.Add(new MidiMsg(status, pitch, data2));
                    }
                }
            }

            w.WriteEndElement(); // end of moment
        }

        /// <summary>
        /// Moritz currently just sends the two COARSE messages to set the semitones value
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="semitones"></param>
        /// <returns></returns>
        private List<MidiMsg> GetPitchWheelDeviationMessages(int channel, int semitones)
        {
            List<MidiMsg> rList = new List<MidiMsg>();
            int status = M.CMD_CONTROL_CHANGE_0xB0 + channel;
            MidiMsg msg1 = new MidiMsg(status, M.CTL_REGISTEREDPARAMETER_COARSE_101, M.SELECT_PITCHBEND_RANGE_0);
            rList.Add(msg1);
            MidiMsg msg2 = new MidiMsg(status, M.CTL_DATAENTRY_COARSE_6, semitones);
            rList.Add(msg2);
            //MidiMsg msg3 = new MidiMsg(status, M.CTL_REGISTEREDPARAMETER_FINE_100, M.SELECT_PITCHBEND_RANGE_0);
            //rList.Add(msg3);
            //MidiMsg msg4 = new MidiMsg(status, M.CTL_DATAENTRY_FINE_38, cents);
            //rList.Add(msg4);

            return rList;
        }

        /// <summary>
        /// The argument contains a list of 12 velocity values (range [1..127] in order of absolute pitch.
        /// For example: If the MidiChordDef contains one or more C#s, they will be given velocity velocityPerAbsolutePitch[1].
        /// Middle-C is midi pitch 60 (60 % 12 == absolute pitch 0), middle-C# is midi pitch 61 (61 % 12 == absolute pitch 1), etc.
        /// </summary>
        /// <param name="velocityPerAbsolutePitch">A list of 12 velocity values (range [1..127] in order of absolute pitch</param>
        public void SetVelocityPerAbsolutePitch(List<byte> velocityPerAbsolutePitch, double percent)
        {
            double factorForNewValue = percent / 100;
            double factorForOldValue = 1 - factorForNewValue;
            for(int pitchIndex = 0; pitchIndex < Pitches.Count; ++pitchIndex)
            {
                byte oldVelocity = Velocities[pitchIndex];

                int absPitch = Pitches[pitchIndex] % 12;
                byte newVelocity = velocityPerAbsolutePitch[absPitch];
                AssertIsVelocityValue(newVelocity);

                byte velocity = (byte)Math.Round((oldVelocity * factorForOldValue) + (newVelocity * factorForNewValue));
                Velocities[pitchIndex] = VelocityValue(velocity);
            }
        }

        /// <summary>
        /// Individual velocities will be set in the range 1..127
        /// </summary>
        /// <param name="factor"></param>
        internal void AdjustVelocities(double factor)
        {
            Debug.Assert(factor > 0.0);
            for(int i = 0; i < Velocities.Count; ++i)
            {
                byte velocity = (byte)Math.Round((Velocities[i] * factor));
                Velocities[i] = VelocityValue(velocity); 
            }
        }

        /// <summary>
        /// Returns the argument as a byte coerced to the range 1..127.
        /// </summary>
        private byte VelocityValue(int velocity)
        {
            velocity = (velocity >= 1) ? velocity : 1;
            velocity = (velocity <= 127) ? velocity : 127;
            return (byte)velocity;
        }

        /// <summary>
        /// A Debug.Assert that fails if the argument is outside the range 1..127.
        /// </summary>
        private static void AssertIsVelocityValue(int velocity)
        {
            Debug.Assert(velocity >= 1 && velocity <= 127);
        }

        /// <summary>
        /// The arguments must both be in range [1..127].
        /// If the basicMidiChordDef contains more than 1 note (=velocity), the velocities of the root and top notes in the
        /// chord are set to the argument values, and the other velocities are interpolated linearly. 
        /// </summary>
        public void SetVerticalVelocityGradient(byte rootVelocity, byte topVelocity)
        {
            #region conditions
            AssertIsVelocityValue(rootVelocity);
            AssertIsVelocityValue(topVelocity);
            #endregion conditions
            
            if(Velocities.Count > 1)
            {
                double increment = (((double)(topVelocity - rootVelocity)) / (Velocities.Count - 1));
                double newVelocity = rootVelocity;
                for(int velocityIndex = 0; velocityIndex < Velocities.Count; ++velocityIndex)
                {
                    Velocities[velocityIndex] = VelocityValue((int)Math.Round(newVelocity));
                    newVelocity += increment;
                }
            }
        }

        public override string ToString() => $"BasicMidiChordDef: MsDuration={MsDuration.ToString()} BasePitch={Pitches[0]} ";

        public int MsDuration { get { return _msDuration; } set { _msDuration = value; } }
        private int _msDuration = 0;

        public List<byte> Pitches = new List<byte>();
        public List<byte> Velocities = new List<byte>();
        public byte? BankIndex = null; // optional. If null, bank commands are not sent
        public byte? PatchIndex = null; // optional. If null, patch commands are not sent
        public byte? PitchWheelDeviation = null; // optional. If null, PitchWheelDeviation commands are not sent
        public bool HasChordOff = true;
    }
}
