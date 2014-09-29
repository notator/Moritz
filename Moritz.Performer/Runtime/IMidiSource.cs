
namespace Moritz.Performer
{
	public interface IMidiSource
	{
		// Connect to a subordinate object, so that that object's ProcessMessage
		// functions get linked to this node's Send... delegates.
		void Connect(IMidiSink connectedMidiNode);
		// Disconnect the subordinate node.
		void Disconnect(IMidiSink connectedMidiNode);

		bool IsRunning { get; } // true if this object has been connected to an IMidiSinkNode or an output device.

        /// <summary>
        /// Stop the streaming of Midi messages to the subsidiary object,
        /// possibly by calling this function on a superior object.
        /// Called by a subsidiary object which is receiving a stream of Midi messages.
        /// </summary>
        void StopMidiStreaming();
        /// <summary>
        /// Start the streaming of Midi messages to the subsidiary object,
        /// possibly by calling this function on a superior object.
        /// Called by a subsidiary object which is not receiving a stream of Midi messages.
        /// </summary>
        void StartMidiStreaming();
    }
}
