
using System.Windows.Forms;

using WMPLib; // needs a reference to WMPLib in Solution Explorer. WMPLib can be found in the COM libs. 

namespace Moritz.Globals
{
    /// <summary>
    /// This player is simply a wrapper for the WindowsMediaPlayer.
    /// It could obviously be extended, but I dont need any more functions for now...
    /// 
    /// See http://msdn.microsoft.com/en-us/library/dd562692.aspx
    /// For all functionality reference, see
    /// http://msdn.microsoft.com/en-us/library/dd563069%28v=VS.85%29.aspx
    /// For just the controls (stop, fast forward etc.) see
    /// http://msdn.microsoft.com/en-us/library/dd563212%28v=VS.85%29.aspx
    /// 
    /// j.i. Sept.2011
    /// </summary>
    public class MoritzMediaPlayer
    {
        public MoritzMediaPlayer(HasStoppedEventHandler hasStoppedEventHandler)
        {
            InitializeWindowsMediaPlayer();
            HasStopped = hasStoppedEventHandler;
        }

        private void InitializeWindowsMediaPlayer()
        {
            WindowsMediaPlayer = new WindowsMediaPlayer();
            WindowsMediaPlayer.PlayStateChange +=
                new _WMPOCXEvents_PlayStateChangeEventHandler(Player_PlayStateChange);
            WindowsMediaPlayer.MediaError +=
                new _WMPOCXEvents_MediaErrorEventHandler(Player_MediaError);
            WindowsMediaPlayer.settings.autoStart = false;
        }

        private void Player_MediaError(object pMediaObject)
        {
            MessageBox.Show("File error: Cannot play media file.");
        }

        /// <summary>
        /// Plays the full file path currently set in the players URL field.
        /// If the player is already playing, this function does nothing.
        /// </summary>
        /// <param name="fullPath"></param>
        public void Play()
        {
            if(!(WindowsMediaPlayer.playState == WMPPlayState.wmppsPlaying))
            {
                WindowsMediaPlayer.controls.play();
            }
        }
        /// <summary>
        /// If the player is already stopped, this function does nothing.
        /// </summary>
        public void StopPlaying()
        {
            if(WindowsMediaPlayer.playState == WMPPlayState.wmppsPlaying)
            {
                WindowsMediaPlayer.controls.stop();
            }
        }

        private void Player_PlayStateChange(int NewState)
        {
            if((WMPLib.WMPPlayState)NewState == WMPLib.WMPPlayState.wmppsStopped)
            {
                this.HasStopped();
            }
        }

        public string URL { get { return WindowsMediaPlayer.URL; } set { WindowsMediaPlayer.URL = value; } }

        private WindowsMediaPlayer WindowsMediaPlayer;
        private readonly HasStoppedEventHandler HasStopped;
    }

    public delegate void HasStoppedEventHandler();
}
