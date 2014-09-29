using Multimedia.Midi;
using Moritz.Midi;

namespace Moritz.Performer
{
	public interface IMidiSink
	{
		// this object's processing functions
		void ProcessMessage(ChordMessage chord); // calls SendChordMessage
		void ProcessMessage(ChannelMessage message); // calls SendChannelMessage
		void ProcessMessage(SysExMessage message); // calls SendSysExMessage
		void ProcessMessage(SysCommonMessage message); // calls SendSysCommonMessage
		void ProcessMessage(SysRealtimeMessage message); // calls SendSysRealtimeMessage
	}
}
