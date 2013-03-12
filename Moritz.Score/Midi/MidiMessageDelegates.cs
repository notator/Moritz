
using Multimedia.Midi;

namespace Moritz.Score.Midi
{
    public delegate void ChordMessageDelegate(ChordMessage chord);
    public delegate void ChannelMessageDelegate(ChannelMessage midiMessage);
    public delegate void SysExMessageDelegate(SysExMessage midiMessage);
    public delegate void SysCommonMessageDelegate(SysCommonMessage midiMessage);
    public delegate void SysRealtimeMessageDelegate(SysRealtimeMessage midiMessage);
}
