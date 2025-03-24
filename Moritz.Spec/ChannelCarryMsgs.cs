using Moritz.Globals;

using Sanford.Multimedia.Midi;

using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

namespace Moritz.Spec
{  
    /// <summary>
	/// Carries a channels noteOffs from a MidiChordDef to the next MidiChordDef or RestDef.
	/// This class's WriteSVG(w, channelIndex, trkIndex) function is called by both MidiChordDef and MidiRestDef.
	/// There is one ChannelCarryMsgs object per channel.
	/// </summary>
	public class ChannelCarryMsgs
	{
    	public ChannelCarryMsgs(int channel)
		{
			_channel = channel; // Add missing semicolon
		}

		/// <summary>
		/// Writes NoteOff messages carried from a previous moment
		/// </summary>
		/// <param name="w"></param>
		internal void WriteSVG(XmlWriter w, int trkIndex)
		{
			var NoteOffMsgs = _channelCarryMessages[trkIndex];
			if(NoteOffMsgs.Count > 0)
			{
				w.WriteStartElement("noteOffs");
				foreach(MidiMsg msg in NoteOffMsgs)
				{
					Debug.Assert(IsNoteOffMsg(msg));
					msg.WriteSVG(w);
				}
				w.WriteEndElement(); // end of noteOffs
			}
		}

		#region List functions
		/// <summary>
		/// Checks that all messages are noteOffs and have the correct channel.
		/// </summary>
		public void Add(int trkIndex, MidiMsg msg)
		{
			CheckNoteOffMsg(msg);
			_channelCarryMessages[trkIndex].Add(msg);
		}

		/// <summary>
		/// Checks that all messages are noteOffs and have the same channel.
		/// </summary>
		public void AddRange(int trkIndex, List<MidiMsg> msgs)
		{
			foreach(MidiMsg msg in msgs)
			{
				_channelCarryMessages[trkIndex].Add(msg);
			}
		}

		public void Clear()
		{
			foreach(var msgList in _channelCarryMessages)
			{
				msgList.Clear();
			}
		}

		public List<int> TrkCounts
		{
			get
			{
				List<int> trkCounts = new List<int>();
				foreach(var msgList in _channelCarryMessages)
				{
					trkCounts.Add(msgList.Count);
				}
				return trkCounts;
			}
		}

		/// <summary>
		/// Checks that a msg is a noteOff and has the correct channel.
		/// </summary>
		private void CheckNoteOffMsg(MidiMsg msg)
		{
			Debug.Assert(IsNoteOffMsg(msg));
			Debug.Assert(msg.Channel == _channel);
		}

		private bool IsNoteOffMsg(MidiMsg msg)
		{
			int statusHighNibbble = msg.Status & 0xF0;
			if(statusHighNibbble == (int)M.CMD.NOTE_OFF_120 || (statusHighNibbble == (int)M.CMD.NOTE_ON_144 && msg.Data2 == 0))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		#endregion

		private List<List<MidiMsg>> _channelCarryMessages = new List<List<MidiMsg>>();
        private int _channel;

        /// <summary>
        /// These two are true for the first moment in the score, otherwise false.
        /// </summary>
        internal bool IsStartOfEnvs = true;
		internal bool IsStartOfSwitches = true;

		// If an algorithm does not set the bank and patch for each channel at
		// the start of the score, they are both set to 0.
		internal byte BankState = 255;
		internal byte PatchState = 255;
		// If an algorithm does not set the states of the following controllers
		// at the start of the score, they are explicitly set to the followings values:
		// (These are the states that should be set by AllControllersOff)
		//     ModWheelState = 0;
		//     ExpressionState = 127;
		//     PanState = 64;
		//     PitchWheelState = 64;
		//     PitchWheelDeviationState = 2;
		internal byte ModWheelState = 255;
		internal byte ExpressionState = 255;
		internal byte PanState = 255;
		internal byte PitchWheelState = 255;
		internal byte PitchWheelDeviationState = 255;
	}
}