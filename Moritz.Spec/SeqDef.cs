using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Moritz.Spec
{
	/// <summary>
	/// Message types that can be sent to Trks
	/// </summary>
	public enum TrkMessageType
	{ 
		// sent from incoming NoteOns or NoteOffs
		trkOn, trkOff,
		// sent from incoming pressure (Aftertouch or ChannelPressure)
		aftertouch,
        channelPressure,
        pitchWheel,
        modulation,
        pan,
        expression,
        timbre				
	}


	public class SeqDef
	{
		public SeqDef(int containersMsPositionInScore)
		{
			_msPositionInScore = containersMsPositionInScore;
		}

		/// When the performer sends a NoteOn to the containing InputNoteDef, the TrkDef will be sent the TrkOnOrOffMessage. 
		/// <param name="trkDef">The trkDef to which a message is to be sent.</param>
		/// <param name="trkOnOrTrkOff">The message to be sent.</param>
		public void AddNoteOnTrkMsg(TrkDef trkDef, TrkMessageType trkOnOrTrkOff)
		{
			Debug.Assert(trkOnOrTrkOff == TrkMessageType.trkOn || trkOnOrTrkOff == TrkMessageType.trkOff);
			int msOffset = trkDef[0].MsPosition - _msPositionInScore;
			_noteOnTrkRefs.Add(new TrkRef(trkOnOrTrkOff, trkDef.MidiChannel, trkDef.Count, msOffset));
		}

		/// When the performer sends a NoteOff to the containing InputNoteDef, the TrkDef will be sent the TrkOnOrOffMessage. 
		/// <param name="trkDef">The trkDef to which a message is to be sent.</param>
		/// <param name="trkOnOrTrkOff">The message to be sent.</param>
		public void AddNoteOffTrkMsg(TrkDef trkDef, TrkMessageType trkOnOrTrkOff)
		{
			Debug.Assert(trkOnOrTrkOff == TrkMessageType.trkOn || trkOnOrTrkOff == TrkMessageType.trkOff);
			int msOffset = trkDef[0].MsPosition - _msPositionInScore;
			_noteOffTrkRefs.Add(new TrkRef(trkOnOrTrkOff, trkDef.MidiChannel, trkDef.Count, msOffset));
		}

		/// When the performer sends a pressure message (Aftertouch or ChannelPressure) to the containing InputNoteDef,
		/// the TrkDef will be sent the TrkMessage. 
		/// <param name="trkDef">The trkDef to which a message is to be sent.</param>
		/// <param name="TrkOnOrTrkOff">The message to be sent.</param>
		public void AddPressureTrkMsg(TrkDef trkDef, TrkMessageType controller)
		{
			Debug.Assert(!(controller == TrkMessageType.trkOn || controller == TrkMessageType.trkOff));
			int msOffset = trkDef[0].MsPosition - _msPositionInScore;
			_noteOffTrkRefs.Add(new TrkRef(controller, trkDef.MidiChannel, trkDef.Count, msOffset));
		}


		internal void WriteSvg(Xml.SvgWriter w)
		{
			if(_noteOnTrkRefs != null && _noteOnTrkRefs.Count > 0)
			{
				w.WriteStartElement("noteOnTrks");
				//w.WriteAttributeString("notatedKey", _notatedMidiPitch.ToString());
				foreach(TrkRef trkRef in _noteOnTrkRefs)
				{
					trkRef.WriteSvg(w);
				}
				w.WriteEndElement(); // noteOnTrks
			}
			if(_noteOffTrkRefs != null && _noteOffTrkRefs.Count > 0)
			{
				w.WriteStartElement("noteOffTrks");
				//w.WriteAttributeString("notatedKey", _notatedMidiPitch.ToString());
				foreach(TrkRef trkRef in _noteOffTrkRefs)
				{
					trkRef.WriteSvg(w);
				}
				w.WriteEndElement(); // noteOffTrks
			}
			if(_pressureTrkRefs != null && _pressureTrkRefs.Count > 0)
			{
				w.WriteStartElement("pressureTrks");
				//w.WriteAttributeString("notatedKey", _notatedMidiPitch.ToString());
				foreach(TrkRef trkRef in _pressureTrkRefs)
				{
					trkRef.WriteSvg(w);
				}
				w.WriteEndElement(); // pressureTrks
			}	
		}

		/// <summary>
		/// A list of synchronous TrkRefs.
		/// A performer's NoteOn sends messages to these Trks.
		/// The message _type_ (on or off) is defined in the TrkRef.
		/// </summary>
		private List<TrkRef> _noteOnTrkRefs = new List<TrkRef>();
		/// <summary>
		/// A list of synchronous TrkRefs.
		/// A performer's NoteOff sends messages to these Trks.
		/// The message _type_ (on, or off) is defined in the TrkRef.
		/// </summary>
		private List<TrkRef> _noteOffTrkRefs = new List<TrkRef>();
		/// <summary>
		/// A list of synchronous TrkRefs.
		/// A performer's Pressure (Aftertouch or ChannelPressure) sends messages to these Trks
		/// The message _type_ (MIDI controller) is defined in the TrkRef.
		/// </summary>
		private List<TrkRef> _pressureTrkRefs = new List<TrkRef>();

		private int _msPositionInScore; // the position of the containing chord
	}
}
