//	wakeup.exe
//	Copyright (c) 2013 Velko Nikolov, velko.nikolov@gmail.com
//
//	MIT license (http://en.wikipedia.org/wiki/MIT_License)
//
//	Permission is hereby granted, free of charge, to any person obtaining a copy
//	of this software and associated documentation files (the "Software"), to deal
//	in the Software without restriction, including without limitation the rights 
//	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//	of the Software, and to permit persons to whom the Software is furnished to do so, 
//	subject to the following conditions:
//
//	The above copyright notice and this permission notice shall be included in all 
//	copies or substantial portions of the Software.
//
//	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
//	INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
//	PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
//	FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
//	ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Media;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NAudio;
using NAudio.Wave;

[assembly: AssemblyTitle("WakeUp!")]
[assembly: AssemblyProduct("WakeUp!")]
[assembly: AssemblyCopyright("Copyright © 2013 Velko Nikolov")]
[assembly: AssemblyVersion("1.0.*")]
// [assembly: AssemblyFileVersion("1.0.0.0")]

class Program
{
    static void Main(string[] args) {
        
        var form = new WakeupForm();
        Application.Run(form);
        
    }
    
    class WakeupForm : Form
    {
    
        int state;
        const int state_start = 0;
        const int state_load = 1;
        const int state_active = 2;
        const int state_over = 3;
        
        const int seconds = 1000;
        const int minutes = 60*seconds;
        
        const int COUNTDOWN_TIME = 30*seconds;
        const int USER_ACTIVITY = 5*seconds;
        const int MISSED_ALARM = 5*minutes;
        
        bool close_allowed = false;
        
        // http://msdn.microsoft.com/en-us/magazine/cc164015.aspx 2013-03-05
        readonly System.Windows.Forms.Timer timer_user_activity = new Timer();
        readonly System.Windows.Forms.Timer timer_countdown_seconds = new Timer();
        readonly System.Windows.Forms.Timer timer_missed_alarm = new Timer();
        readonly System.Windows.Forms.Timer timer_unmute = new Timer();
        readonly System.Windows.Forms.Timer timer_play = new Timer();
        readonly Button button;
        int remaining;
        
        
        public WakeupForm() 
        {
            this.Width = 400;
            this.Height = 400;
            this.ControlBox = false;
            this.ShowInTaskbar = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.TopMost = true;
            this.Text = "Wake Up!";
            
            button = new Button();
            button.Dock = DockStyle.Fill;
            this.Controls.Add(button);
            
            var clicks = 0;
            
#warning refactor spaghetti
            
            button.Font = new Font("Arial", 20);
            button.Click += (_1, _2) => { 
                if(state == state_over) {
                    return;
                }
                state = state_active;
                
                RefreshText();
                timer_play.Stop();
                DisposeAll();
                button.Text = (clicks+=1) + ""; 
                timer_user_activity.Stop(); 
                timer_user_activity.Start(); 
                timer_countdown_seconds.Start(); 
                timer_missed_alarm.Enabled = true;
            };
            
            timer_user_activity.Tick += (_1, _2) => { 
                if(state == state_over) {
                    return;
                }
                clicks = 0;
                state = StartAlarm();
            };

            timer_user_activity.Interval = USER_ACTIVITY;
            timer_countdown_seconds.Interval = 1*seconds;
            timer_missed_alarm.Interval = MISSED_ALARM;
            timer_unmute.Interval = 1*seconds;
            timer_play.Interval = 20*seconds;
            
            timer_play.Tick += (_1, _2) => {
                Play();
            };
            
            timer_countdown_seconds.Tick += (_1, _2) => {
                if(state == state_over) {
                    return;
                }
                remaining -= 1*seconds;
                RefreshText();
                if(remaining <= 0) {
                    Over("Good morning!\nNote that if you go back to bed, you will fall asleep again :-)");
                }
            };
            
            timer_missed_alarm.Tick += (_1, _2) => { 
                if(state == state_over) {
                    return;
                }
                if(state == state_load) {   

                    Over("You missed your alarm!");
                    this.Close();
                }
            };
            
            timer_unmute.Tick += (_1, _2) => {
                Interop.Mute(false);
                Interop.MaxVolume();
            };
            
            state = state_start;
            
        }
        
        void Over(string message) {
            state = state_over;
            timer_play.Stop();
            DisposeAll();
            timer_user_activity.Stop(); 
            timer_countdown_seconds.Stop();
            timer_missed_alarm.Stop();
            timer_unmute.Stop();

            close_allowed = true;
            button.Enabled = false; 
            MessageBox.Show(message); 
            this.Close();
        }

        void Play()
        {
            var asm = Assembly.GetExecutingAssembly();
            var stream = asm.GetManifestResourceStream(@"wakeup.alarm-clock-1.wav") ??
                asm.GetManifestResourceStream(@"alarm-clock-1.wav");

            PlayInAllAvailableDevices(stream);
        }
        
        void RefreshText() {
            if(state <= state_load) {
                this.Text = "Wake Up!";
            }
            else if(state == state_active) {
                var rem = TimeSpan.FromMilliseconds(remaining);
                this.Text = rem.Minutes.ToString("00") + ":" + rem.Seconds.ToString("00");
            }
        }
        
        // http://stackoverflow.com/questions/1201634/why-is-onclosing-obsolete-and-should-i-migrate-to-onformclosing 2013-03-05
        protected override void OnFormClosing(FormClosingEventArgs fcea) {
# warning handle various cases
            base.OnFormClosing(fcea);
            if(!close_allowed) {
                fcea.Cancel = true;
            }
            
        }
            
        // http://stackoverflow.com/questions/156046/show-a-form-without-stealing-focus 2013-03-05
        protected override bool ShowWithoutActivation
        {
          get { return true; }
        }
        
        int StartCountdown() {

            state = state_active;
            timer_play.Stop();
            DisposeAll();
            
            timer_user_activity.Start();
            timer_countdown_seconds.Start();
            timer_missed_alarm.Stop();
            
            RefreshText();
            return state_active;
                
        }
        
        int StartAlarm() {
            state = state_load;

            remaining = COUNTDOWN_TIME;

            Play();
            timer_play.Start();
            
            
            timer_countdown_seconds.Stop();
            timer_missed_alarm.Start();
            timer_unmute.Start();
            RefreshText();
            
            return state_load;
        }
        
        // http://stackoverflow.com/questions/9232291/is-it-possible-to-hide-winform-in-taskmanager-application-tab
        protected override CreateParams CreateParams {
            get {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x80;  // Turn on WS_EX_TOOLWINDOW
                return cp;
            }
        }
        
        protected override void OnActivated(EventArgs ea) {
            base.OnActivated(ea);
            if(state <= state_load) {
            
                state = StartCountdown();
                
            }
        }
        
        protected override void OnLoad(EventArgs ea) {
            base.OnLoad(ea);
            
            state = StartAlarm();

        }
        
        // http://stackoverflow.com/questions/6011515/how-to-play-sound-in-many-devices-at-the-same-time
        
        private void PlaySoundInDevice(int deviceNumber, Stream stream)
        {
            if (outputDevices.ContainsKey(deviceNumber))
            {
                outputDevices[deviceNumber].WaveOut.Dispose();
                outputDevices[deviceNumber].WaveStream.Dispose();
            }
            var waveOut = new WaveOut();
            waveOut.DeviceNumber = deviceNumber;
            WaveStream waveReader = new RawSourceWaveStream(stream, new WaveFormat());
            // hold onto the WaveOut and  WaveStream so we can dispose them later
            outputDevices[deviceNumber] = new PlaybackSession { WaveOut=waveOut, WaveStream=waveReader };

            waveOut.Init(waveReader);
            waveOut.Play();
        }

        private Dictionary<int, PlaybackSession> outputDevices = new Dictionary<int, PlaybackSession>();

        class PlaybackSession
        {
            public IWavePlayer WaveOut { get; set; }
            public WaveStream WaveStream { get; set; }
        }
        
        private void DisposeAll()
        {
            foreach (var playbackSession in outputDevices.Values)
            {
                playbackSession.WaveOut.Dispose();
                playbackSession.WaveStream.Dispose();
            }
        }
        
        public void PlayInAllAvailableDevices(Stream stream)
        {
            int waveOutDevices = WaveOut.DeviceCount;
            for (int n = 0; n < waveOutDevices; n++)
            {
                PlaySoundInDevice(n, stream);
            }
        }
    }
}























