/* This program is based on SoundCatcher (2008, by Jeff Morton) and was downloaded and modified
 * under the terms of the GNU GPL, from https://www.codeproject.com/Articles/22951/Sound-Activated-Recorder-with-Spectrogram-in-C
 * This program is a PoC *ONLY* and should not under any circumstances be used to evaluate if there
 * is a risk of safety to hearing, or to perform any sort of safety/compliance-related testing.
 
   This program is free software; you can redistribute it and/or modify
   it under the terms of the GNU General Public License as published by
   the Free Software Foundation; either version 2 of the License, or
   (at your option) any later version.

   This program is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU General Public License for more details.
 
   You should have received a copy of the GNU General Public License
   along with this program; if not, write to the Free Software
   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Media;
using SoundAlert;

namespace SoundAlert
{
    public partial class FormMain : Form
    {
        private WaveInRecorder _recorder;
        private byte[] _recorderBuffer;
        private byte[] _playerBuffer;
        private WaveFormat _waveFormat;
        private AudioFrame _audioFrame;
        private FifoStream _streamOut;
        private MemoryStream _streamMemory;
        private Stream _streamWave;
        private FileStream _streamFile;
        private bool _isPlayer = false;  // audio output for testing
        private bool _isTest = false;  // signal generation for testing
        private bool _isSaving = false;
        private bool _isShown = true;
        private string _sampleFilename;
        public DateTime _timeLastDetection;
        

        public static class GlobalVars
        {
            public static int tickCount = 0;
            public static bool listenFlag = true; 
        }


        public FormMain()
        {
            InitializeComponent();
        }
        private void FormMain_Load(object sender, EventArgs e)
        {
            timer1.Start();
            if (WaveNative.waveInGetNumDevs() == 0)
            {
                textBoxConsole.AppendText(DateTime.Now.ToString() + " : No audio input devices detected\r\n");
            }
            else
            {
                textBoxConsole.AppendText(DateTime.Now.ToString() + " : Audio input device detected\r\n");
                if (_isPlayer == true)
                    _streamOut = new FifoStream();
                _audioFrame = new AudioFrame(_isTest);
                _audioFrame.IsDetectingEvents = SoundAlert.Properties.Settings.Default.SettingIsDetectingEvents;
                _audioFrame.AmplitudeThreshold = SoundAlert.Properties.Settings.Default.SettingAmplitudeThreshold;
                _streamMemory = new MemoryStream();
                Start();
            }
        }
        private void FormMain_Resize(object sender, EventArgs e)
        {
            if (_audioFrame != null)
            {
                  _audioFrame.RenderTimeDomainLeft(ref pictureBoxTimeDomainLeft);
                  _audioFrame.RenderTimeDomainRight(ref pictureBoxTimeDomainRight);
                _audioFrame.RenderFrequencyDomainLeft(ref pictureBoxFrequencyDomainLeft, SoundAlert.Properties.Settings.Default.SettingSamplesPerSecond);
                _audioFrame.RenderFrequencyDomainRight(ref pictureBoxFrequencyDomainRight, SoundAlert.Properties.Settings.Default.SettingSamplesPerSecond);
                 _audioFrame.RenderSpectrogramLeft(ref pictureBoxSpectrogramLeft);
                  _audioFrame.RenderSpectrogramRight(ref pictureBoxSpectrogramRight);
            }
        }
        private void FormMain_SizeChanged(object sender, EventArgs e)
        {
            if (_isShown & this.WindowState == FormWindowState.Minimized)
            {
                foreach (Form f in this.MdiChildren)
                {
                    f.WindowState = FormWindowState.Normal;
                }
                this.ShowInTaskbar = false;
                this.Visible = false;
                notifyIcon1.Visible = true;
                _isShown = false;
            }
        }
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Stop();
            if (_isSaving == true)
            {
                byte[] waveBuffer = new byte[SoundAlert.Properties.Settings.Default.SettingBitsPerSample];
                _streamWave = WaveStream.CreateStream(_streamMemory, _waveFormat);
                waveBuffer = new byte[_streamWave.Length - _streamWave.Position];
                _streamWave.Read(waveBuffer, 0, waveBuffer.Length);
                if (SoundAlert.Properties.Settings.Default.SettingOutputPath != "")
                    _streamFile = new FileStream(SoundAlert.Properties.Settings.Default.SettingOutputPath + "\\" + _sampleFilename, FileMode.Create);
                else
                    _streamFile = new FileStream(_sampleFilename, FileMode.Create);
                _streamFile.Write(waveBuffer, 0, waveBuffer.Length);
                _isSaving = false;
            }
            if (_streamOut != null)
                try
                {
                    _streamOut.Close();
                }
                finally
                {
                    _streamOut = null;
                }
            if (_streamWave != null)
                try
                {
                    _streamWave.Close();
                }
                finally
                {
                    _streamWave = null;
                }
            if (_streamFile != null)
                try
                {
                    _streamFile.Close();
                }
                finally
                {
                    _streamFile = null;
                }
            if (_streamMemory != null)
                try
                {
                    _streamMemory.Close();
                }
                finally
                {
                    _streamMemory = null;
                }
        }

      
        private void Start()
        {
            Stop();
            try
            {
                _waveFormat = new WaveFormat(SoundAlert.Properties.Settings.Default.SettingSamplesPerSecond, SoundAlert.Properties.Settings.Default.SettingBitsPerSample, SoundAlert.Properties.Settings.Default.SettingChannels);
                _recorder = new WaveInRecorder(SoundAlert.Properties.Settings.Default.SettingAudioInputDevice, _waveFormat, SoundAlert.Properties.Settings.Default.SettingBytesPerFrame * SoundAlert.Properties.Settings.Default.SettingChannels, 3, new BufferDoneEventHandler(DataArrived));

                textBoxConsole.AppendText(DateTime.Now.ToString() + " : Audio input device polling started\r\n");
                textBoxConsole.AppendText(DateTime.Now + " : Device = " + SoundAlert.Properties.Settings.Default.SettingAudioInputDevice.ToString() + "\r\n");
                textBoxConsole.AppendText(DateTime.Now + " : Channels = " + SoundAlert.Properties.Settings.Default.SettingChannels.ToString() + "\r\n");
                textBoxConsole.AppendText(DateTime.Now + " : Bits per sample = " + SoundAlert.Properties.Settings.Default.SettingBitsPerSample.ToString() + "\r\n");
                textBoxConsole.AppendText(DateTime.Now + " : Samples per second = " + SoundAlert.Properties.Settings.Default.SettingSamplesPerSecond.ToString() + "\r\n");
                textBoxConsole.AppendText(DateTime.Now + " : Frame size = " + SoundAlert.Properties.Settings.Default.SettingBytesPerFrame.ToString() + "\r\n");
            }
            catch (Exception ex)
            {
                //textBoxConsole.AppendText(DateTime.Now + " : " + ex.InnerException.ToString() + "\r\n");
            }
        }
        private void Stop()
        {
            if (_recorder != null)
                try
                {
                    _recorder = null;
                    _recorder.Dispose();
                }
                catch
                {
                    _recorder = null;
                }
                finally
                {
                    _recorder = null;
                }

            textBoxConsole.AppendText(DateTime.Now.ToString() + " : Audio input device polling stopped\r\n");
        }

 
        private void Filler(IntPtr data, int size)
        {
            if (_isPlayer == true)
            {
                if (_playerBuffer == null || _playerBuffer.Length < size)
                    _playerBuffer = new byte[size];
                if (_streamOut.Length >= size)
                    _streamOut.Read(_playerBuffer, 0, size);
                else
                    for (int i = 0; i < _playerBuffer.Length; i++)
                        _playerBuffer[i] = 0;
                System.Runtime.InteropServices.Marshal.Copy(_playerBuffer, 0, data, size);
            }
        }
        private void DataArrived(IntPtr data, int size)
        {
            if (_isSaving == true)
            {
                byte[] recBuffer = new byte[size];
                System.Runtime.InteropServices.Marshal.Copy(data, recBuffer, 0, size);
                _streamMemory.Write(recBuffer, 0, recBuffer.Length);
            }
            if (_recorderBuffer == null || _recorderBuffer.Length != size)
                _recorderBuffer = new byte[size];
            if (_recorderBuffer != null)
            {
                System.Runtime.InteropServices.Marshal.Copy(data, _recorderBuffer, 0, size);
                if (_isPlayer == true)
                    _streamOut.Write(_recorderBuffer, 0, _recorderBuffer.Length);
                _audioFrame.Process(ref _recorderBuffer);
                if (_audioFrame.IsEventActive == true)
                {
                    if (_isSaving == false && SoundAlert.Properties.Settings.Default.SettingIsSaving == true)
                    {
                        _sampleFilename = DateTime.Now.ToString("yyyyMMddHHmmss") + ".wav";
                        _timeLastDetection = DateTime.Now;
                        _isSaving = true;
                    }
                    else
                    {
                        _timeLastDetection = DateTime.Now;
                    }
                    Invoke(new MethodInvoker(AmplitudeEvent));
                }

                _audioFrame.RenderTimeDomainLeft(ref pictureBoxTimeDomainLeft);
                _audioFrame.RenderTimeDomainRight(ref pictureBoxTimeDomainRight);
                _audioFrame.RenderFrequencyDomainLeft(ref pictureBoxFrequencyDomainLeft, SoundAlert.Properties.Settings.Default.SettingSamplesPerSecond);
                _audioFrame.RenderFrequencyDomainRight(ref pictureBoxFrequencyDomainRight, SoundAlert.Properties.Settings.Default.SettingSamplesPerSecond);
                _audioFrame.RenderSpectrogramLeft(ref pictureBoxSpectrogramLeft);
               _audioFrame.RenderSpectrogramRight(ref pictureBoxSpectrogramRight);
                GlobalVars.tickCount = 0;

            }
        }
        private void AmplitudeEvent()
        {
           // fire something here if you want
        }
        private void FileSavedEvent()
        {
            textBoxConsole.AppendText(_timeLastDetection.ToString() + " : File " + _sampleFilename + " saved\r\n"); //not implemented in our modification
        }


          
    }
}
