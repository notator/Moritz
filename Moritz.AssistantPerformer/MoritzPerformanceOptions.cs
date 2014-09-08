using System.Diagnostics;
using System;
using System.Xml.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

using Multimedia.Midi;

using Moritz.Globals;
using Moritz.Krystals;
using Moritz.Score;
using Moritz.Score.Midi;
using Moritz.Score.Notation;

namespace Moritz.AssistantPerformer
{
    public class MoritzPerformanceOptions
    {
        /// <summary>
        /// If the performance options file named in the argument exists, this constructor loads the options
        /// stored there. 
        /// If the moritzOptionsPathname is null or does not exist, default options are created using the second
        /// argument to determine the (number of) voices for the (Assistant) MoritzPerformers.
        /// MoritzPerformanceOptions files are saved next to the score, with the same name as the score (without
        /// its extension) followed by ".[number].mpox", where [number] is an integer which increases each time a new
        /// .mpox file is saved for the score. For example, the score Bruckner.capx has performance option files
        /// Bruckner.1.mpox, Bruckner.2.mpox, Bruckner.3.mpox etc.
        /// </summary>
        /// <param name="moritzOptionsPathname">The path to an .mpox file. If the file does not yet exist, default values will be used</param>
        /// <param name="channels">The channels to be used.</param>
        /// <param name="midiFilePerformanceOptions">This argument simply disambiguates this constructor from another one.</param>
        public MoritzPerformanceOptions(Multimedia.Midi.Sequence sequence, string moritzOptionsPathname)
        {
            _moritzOptionsPathname = moritzOptionsPathname;

            if(File.Exists(_moritzOptionsPathname))
                Read(_moritzOptionsPathname);

            List<int> channels = new List<int>();
            foreach(Track track in sequence)
            {
                IEnumerator<MidiEvent> midiEvents = track.Iterator().GetEnumerator();
                // Here, midiEvents.Current is before the first midiEvent. (According to Leslie Sanford.)
                MidiEvent midiEvent = null;
                while(midiEvents.MoveNext())
                {
                    midiEvent = midiEvents.Current;
                    ChannelMessage channelMessage = midiEvent.MidiMessage as ChannelMessage;
                    if(channelMessage != null && channelMessage.Command == ChannelCommand.NoteOn)
                    {
                        channels.Add(channelMessage.MidiChannel);
                        break;
                    }
                }
            }

            // If _moritzOptionsPathname does not exist, or it does not contain a "voices" element,
            // then these are the defaults:
            if(MoritzPlayers.Count == 0)
            {
                for(int channelIndex = 0; channelIndex < channels.Count; ++channelIndex)
                {
                    string voiceName = "voice " + channels[channelIndex].ToString() + ":";
                    VoiceNames.Add(voiceName);
                    MoritzPlayers.Add(MoritzPlayer.Assistant);
                }
            }

            // If the options file does not exist, or it does not contain a "keyboards" element,
            // then these are the defaults:
            if(KeyboardSettings.Count == 0)
            {
                List<KeyType> defaultKeyboard = new List<KeyType>();
                for(int i = 0; i < 128; i++)
                    defaultKeyboard.Add(KeyType.Assisted);
                KeyboardSettings.Add(defaultKeyboard);
            }
        }
        /// <summary>
        /// If the performance options file named in the argument exists, this constructor loads the options
        /// stored there. Otherwise default options are created, whereby the instrument names and number of
        /// channels are taken from the original score file. The original score file can be in .html, .htm,
        /// .svg or .capx format and the scores are looked for in that order.
        /// MoritzPerformanceOptions files are saved next to the score, with the same name as the score (without
        /// its extension) followed by ".[number].mpox", where [number] is an integer which increases each time a new
        /// .mpox file is saved for the score. For example, the score Bruckner.capx has performance option files
        /// Bruckner.1.mpox, Bruckner.2.mpox, Bruckner.3.mpox etc.
        /// </summary>
        /// <param name="moritzOptionsPathname">The path to an .mpox file. If the file does not yet exist, default values will be used</param>
        /// <param name="svgScore">The score for which to create performance options</param>
        public MoritzPerformanceOptions(Preferences preferences, string moritzOptionsPathname, SvgScore svgScore)
        {
            _preferences = preferences;
            _moritzOptionsPathname = moritzOptionsPathname;

            if(File.Exists(_moritzOptionsPathname))
                Read(_moritzOptionsPathname);

            // If _moritzOptionsPathname does not exist, or it does not contain a "channels" element,
            // then these are the defaults:
            if(MoritzPlayers.Count == 0)
            {
                if(String.IsNullOrEmpty(moritzOptionsPathname))
                {
                    MoritzPlayers.Add(MoritzPlayer.Assistant);
                    VoiceNames.Add("1.voice1");
                }
                else
                    if(svgScore != null)
                    {
                        foreach(Staff staff in svgScore.Systems[0].Staves)
                        {
                            string staffName = staff.Staffname;
                            int voiceNumber = 1;
                            foreach(Voice voice in staff.Voices)
                            {
                                MoritzPlayers.Add(MoritzPlayer.Assistant);
                                VoiceNames.Add(staffName + ".voice" + voiceNumber.ToString());
                                ++voiceNumber;
                            }
                        }
                        Debug.Assert(VoiceNames.Count == MoritzPlayers.Count);
                    }
            }

            // If the options file does not exist, or it does not contain a "keyboards" element,
            // then these are the defaults:
            if(KeyboardSettings.Count == 0)
            {
                List<KeyType> defaultKeyboard = new List<KeyType>();
                for(int i = 0; i < 128; i++)
                    defaultKeyboard.Add(KeyType.Assisted);
                KeyboardSettings.Add(defaultKeyboard);
            }
        }

        #region Read
        private void Read(string moritzOptionsPathname)
        {
            int attributesCount = 0;
            Stream optionsStream = File.OpenRead(moritzOptionsPathname);
            using(XmlReader r = XmlReader.Create(optionsStream))
            {
                M.ReadToXmlElementTag(r, "voices", "global", "local", "keyboardSettings");
                while(r.Name == "voices" || r.Name == "global" || r.Name == "local" || r.Name == "keyboardSettings")
                {
                    if(r.NodeType != XmlNodeType.EndElement)
                    {
                        switch(r.Name)
                        {
                            case "voices":
                                GetVoices(r);
                                break;
                            case "global":
                                attributesCount = r.AttributeCount;
                                for(int i = 0; i < attributesCount; i++)
                                {
                                    r.MoveToAttribute(i);
                                    switch(r.Name)
                                    {
                                        case "startAtMoment":
                                            this.StartAtMoment = int.Parse(r.Value);
                                            break;
                                        case "repeatPerformance":
                                            this.RepeatPerformance = (int.Parse(r.Value) != 0);
                                            break;
                                        case "saveMidiFile":
                                            this.SaveMidiFile = (int.Parse(r.Value) != 0);
                                            break;
                                        case "minimumOrnamentChordMsDuration":
                                            this.MinimumOrnamentChordMsDuration = int.Parse(r.Value, M.En_USNumberFormat);
                                            break;
                                    }
                                }
                                break;
                            case "local":
                                attributesCount = r.AttributeCount;
                                for(int i = 0; i < attributesCount; i++)
                                {
                                    r.MoveToAttribute(i);
                                    switch(r.Name)
                                    {
                                        case "performersDynamics":
                                            PerformersDynamicsType = GetPerformersDynamicsType(r.Value);
                                            break;
                                        case "performersPitches":
                                            this.PerformersPitchesType = GetPerformersPitchesType(r.Value);
                                            break;
                                        case "assistantsDurations":
                                            AssistantsDurationsType = GetAssistantsDurationsType(r.Value);
                                            break;
                                        case "assistantsSpeedFactor":
                                            this.SpeedFactor = float.Parse(r.Value, M.En_USNumberFormat);
                                            break;
                                    }
                                }
                                break;
                            case "keyboardSettings":
                                GetKeyboardSettings(r);
                                break;
                        }
                    }
                    M.ReadToXmlElementTag(r, "voices", "global", "local", "keyboardSettings", "MoritzPerformanceOptions");
                }
                // r.Name is "MoritzPerformanceOptions" here
            }
            optionsStream.Close();
        }

        private void GetVoices(XmlReader r)
        {
            // r.Name is "channels" here
            M.ReadToXmlElementTag(r, "voice");
            while(r.Name == "voice")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    int attributesCount = r.AttributeCount;
                    for(int i = 0; i < attributesCount; i++)
                    {
                        r.MoveToAttribute(i);
                        switch(r.Name)
                        {
                            case "name":
                            VoiceNames.Add(r.Value);
                            break;
                            case "player":
                            MoritzPlayer player = MoritzPlayer.Assistant;
                            switch(r.Value)
                            {
                                case "notPlayed":
                                player = MoritzPlayer.None;
                                break;
                                case "assistant":
                                player = MoritzPlayer.Assistant;
                                break;
                                case "livePerformer":
                                player = MoritzPlayer.LivePerformer;
                                break;
                            }
                            MoritzPlayers.Add(player);
                            break;
                        }
                    }
                }
                M.ReadToXmlElementTag(r, "voice", "voices");
            }
            // r.Name is "channels" here
        }
        private void GetKeyboardSettings(XmlReader r)
        {
            // r.Name is "keyboardSettings" here
            M.ReadToXmlElementTag(r, "keyboardSetting");
            while(r.Name == "keyboardSetting")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    List<KeyType> keyTypes = new List<KeyType>();
                    KeyboardSettings.Add(keyTypes);
                    r.MoveToContent();

                    string ks = r.ReadString();
                    for(int i = 0; i < ks.Length; i++)
                    {
                        switch(ks[i])
                        {
                            case '0':
                                keyTypes.Add(KeyType.Silent);
                                break;
                            case '1':
                                keyTypes.Add(KeyType.Assisted);
                                break;
                            case '2':
                                keyTypes.Add(KeyType.Solo_AssistantHearsNothing);
                                break;
                            case '3':
                                keyTypes.Add(KeyType.Solo_AssistantHearsFirst);
                                break;
                        }
                    }
                }
                M.ReadToXmlElementTag(r, "keyboardSetting", "keyboardSettings");
            }
            // r.Name is "keyboardSettings" here
        }
        private PerformersPitchesType GetPerformersPitchesType(string strVal)
        {
            PerformersPitchesType performersPitchesType = PerformersPitchesType.AsNotated;
            switch(strVal)
            {
                case "asNotated":
                    performersPitchesType = PerformersPitchesType.AsNotated;
                    break;
                case "asPerformed":
                    performersPitchesType = PerformersPitchesType.AsPerformed;
                    break;
            }
            return performersPitchesType;
        }
        private PerformersDynamicsType GetPerformersDynamicsType(string strVal)
        {
            PerformersDynamicsType performersDynamicsType = PerformersDynamicsType.AsNotated;
            switch(strVal)
            {
                case "asNotated":
                    performersDynamicsType = PerformersDynamicsType.AsNotated;
                    break;
                case "asPerformed":
                    performersDynamicsType = PerformersDynamicsType.AsPerformed;
                    break;
                case "silent":
                    performersDynamicsType = PerformersDynamicsType.Silent;
                    break;
            }
            return performersDynamicsType;
        }
        private AssistantsDurationsType GetAssistantsDurationsType(string strVal)
        {
            AssistantsDurationsType assistantsDurationsType = AssistantsDurationsType.SymbolsAbsolute;
            switch(strVal)
            {
                case "symbolsAbsolute":
                    assistantsDurationsType = AssistantsDurationsType.SymbolsAbsolute;
                    break;
                case "symbolsRelative":
                    assistantsDurationsType = AssistantsDurationsType.SymbolsRelative;
                    break;
            }
            return assistantsDurationsType;
        }
        #endregion // Read

        #region Save
        private bool AllMoritzPlayersAreDefault()
        {
            bool returnValue = true;
            foreach(MoritzPlayer player in MoritzPlayers)
            {
                if(player != MoritzPlayer.Assistant)
                {
                    returnValue = false;
                    break;
                }
            }
            return returnValue;
        }
        private bool KeyboardSettingsAreDefault()
        {
            bool result = true;
            if(KeyboardSettings.Count > 1)
                result = false;
            else if(KeyboardSettings.Count > 0)
            {
                foreach(KeyType keyType in KeyboardSettings[0])
                {
                    if(keyType != KeyType.Assisted)
                    {
                        result = false;
                        break;
                    }
                }
            }
            return result;
        }
        public void Save()
        {
            bool allChannelsDataAreDefault = AllMoritzPlayersAreDefault();
            bool keyboardSettingsAreDefault = KeyboardSettingsAreDefault();

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("\t");
            settings.CloseOutput = false;
            settings.NewLineOnAttributes = true;
            using(XmlWriter w = XmlWriter.Create(_moritzOptionsPathname, settings))
            {
                w.WriteStartDocument();
                w.WriteComment("file created: " + M.NowString);

                w.WriteStartElement("MoritzPerformanceOptions");
                w.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                w.WriteAttributeString("xsi", "noNamespaceSchemaLocation", null, M.OnlineXMLSchemasFolder + "/moritzPerformanceOptions.xsd");

                #region voices
                w.WriteStartElement("voices");
                w.WriteComment(" Voices appear here in top to bottom order of the score's system layout.        ");
                w.WriteComment(" Voices which are \"notPlayed\" simply do not appear in either the performer's    ");
                w.WriteComment(" or the assistant's lists of moments.                                             ");

                for(int i = 0; i < MoritzPlayers.Count; i++)
                {
                    w.WriteStartElement("voice");
                    w.WriteAttributeString("name", VoiceNames[i].ToString());
                    switch(MoritzPlayers[i])
                    {
                        case MoritzPlayer.None:
                        w.WriteAttributeString("player", "notPlayed");
                        break;
                        case MoritzPlayer.Assistant:
                        w.WriteAttributeString("player", "assistant");
                        break;
                        case MoritzPlayer.LivePerformer:
                        w.WriteAttributeString("player", "livePerformer");
                        break;
                    }
                    w.WriteEndElement(); // closes the channel element
                }
                w.WriteEndElement(); // closes the channels element
                #endregion // voices
                #region global
                w.WriteStartElement("global");
                w.WriteAttributeString("startAtMoment", this.StartAtMoment.ToString());
                w.WriteAttributeString("repeatPerformance", (this.RepeatPerformance ? "1" : "0"));
                w.WriteAttributeString("saveMidiFile", (this.SaveMidiFile ? "1" : "0"));
                w.WriteAttributeString("minimumOrnamentChordMsDuration", MinimumOrnamentChordMsDuration.ToString());
                w.WriteEndElement(); // closes the global element
                #endregion // global
                #region local
                w.WriteStartElement("local");
                w.WriteAttributeString("performersDynamics", M.GetEnumDescription(PerformersDynamicsType));
                w.WriteAttributeString("performersPitches", M.GetEnumDescription(PerformersPitchesType));
                w.WriteAttributeString("assistantsDurations", M.GetEnumDescription(AssistantsDurationsType));
                w.WriteAttributeString("assistantsSpeedFactor", SpeedFactor.ToString(M.En_USNumberFormat));
                w.WriteEndElement(); // closes the local element
                #endregion // local
                #region keyboardSettings
                if(!keyboardSettingsAreDefault)
                {
                    w.WriteStartElement("keyboardSettings");

                    foreach(List<KeyType> keyboardSetting in KeyboardSettings)
                    {
                        w.WriteStartElement("keyboardSetting");
                        XElement keyboardSettingElement = new XElement("keyboardSetting");
                        StringBuilder sb = new StringBuilder();
                        foreach(KeyType keyType in keyboardSetting)
                        {
                            switch(keyType)
                            {
                                case KeyType.Silent:
                                    sb.Append('0');
                                    break;
                                case KeyType.Assisted:
                                    sb.Append('1');
                                    break;
                                case KeyType.Solo_AssistantHearsNothing:
                                    sb.Append('2');
                                    break;
                                case KeyType.Solo_AssistantHearsFirst:
                                    sb.Append('3');
                                    break;
                            }
                        }
                        w.WriteString(sb.ToString());
                        w.WriteEndElement(); // closes the keyboardSetting element
                    }
                    w.WriteEndElement(); // closes the keyboardSettings element
                }
                #endregion // keyboardSettings
                w.WriteEndElement(); // closes the MoritzPerformanceOptions element
                w.Flush();
                //w.Close(); // close unnecessary because of the using statement?
            }
        }
        #endregion // Save

        public void Delete()
        {
            File.Delete(_moritzOptionsPathname);
        }

        public string FileName
        {
            get
            {
                return (Path.GetFileName(_moritzOptionsPathname));
            }
            // set -- see set FilePath
        }
        public string FilePath
        {
            get
            {
                return _moritzOptionsPathname;
            }
            set
            {
                _moritzOptionsPathname = value;
            }
        }

        public int StartAtMoment = 1; 
        public bool RepeatPerformance = false;
        public bool SaveMidiFile = false;
        public int MinimumOrnamentChordMsDuration = M.DefaultMinimumBasicMidiChordMsDuration;
        public PerformersDynamicsType PerformersDynamicsType = PerformersDynamicsType.AsNotated;
        public PerformersPitchesType PerformersPitchesType = PerformersPitchesType.AsNotated;
        public AssistantsDurationsType AssistantsDurationsType = AssistantsDurationsType.SymbolsAbsolute;
        public float SpeedFactor = 1.0f;
        public List<string> VoiceNames = new List<string>();
        public List<MoritzPlayer> MoritzPlayers = new List<MoritzPlayer>();
        public List<List<KeyType>> KeyboardSettings = new List<List<KeyType>>();

        private Preferences _preferences = null;
        private string _moritzOptionsPathname = null;
     }
}
