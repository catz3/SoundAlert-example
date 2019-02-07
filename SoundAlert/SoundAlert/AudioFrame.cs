/* Copyright (C) 2008 Jeff Morton (jeffrey.raymond.morton@gmail.com)

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
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Diagnostics;

namespace SoundAlert
{
    class AudioFrame
    {
        private double[] _waveLeft;
        private double[] _fftLeft;
        private ArrayList _fftLeftSpect = new ArrayList();
        private int _maxHeightLeftSpect = 0;
        private double[] _waveRight;
        private double[] _fftRight;
        private ArrayList _fftRightSpect = new ArrayList();
        private int _maxHeightRightSpect = 0;
        private SignalGenerator _signalGenerator;
        private bool _isTest = false;
        public bool IsDetectingEvents = false;
        public bool IsEventActive = false;
        public int AmplitudeThreshold = 1000;
        Stopwatch stopwatch = new Stopwatch();

      
        /// <summary>
        /// Render waterfall spectrogram to PictureBox
        /// </summary>
        /// <param name="pictureBox"></param>
        public void RenderSpectrogramLeft(ref PictureBox pictureBox)
        {
            Bitmap canvas = new Bitmap(pictureBox.Width, pictureBox.Height);
            Graphics offScreenDC = Graphics.FromImage(canvas);

            // Determine channnel boundries
            int width = canvas.Width;
            int height = canvas.Height;

            double min = double.MaxValue;
            double max = double.MinValue;
            double range = 0;

            if (height > _maxHeightLeftSpect)
                _maxHeightLeftSpect = height;

            // get min/max
            for (int w = 0; w < _fftLeftSpect.Count; w++)
                for (int x = 0; x < ((double[])_fftLeftSpect[w]).Length; x++)
                {
                    double amplitude = ((double[])_fftLeftSpect[w])[x];
                    if (min > amplitude)
                    {
                        min = amplitude;
                    }
                    if (max < amplitude)
                    {
                        max = amplitude;
                    }
                }

            // get range
            if (min < 0 || max < 0)
                if (min < 0 && max < 0)
                    range = max - min;
                else
                    range = Math.Abs(min) + max;
            else
                range = max - min;

            // lock image
            PixelFormat format = canvas.PixelFormat;
            BitmapData data = canvas.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, format);
            int stride = data.Stride;
            int offset = stride - width * 4;

            try
            {
                unsafe
                {
                    byte* pixel = (byte*)data.Scan0.ToPointer();

                    // for each cloumn
                    for (int y = 0; y <= height; y++)
                    {
                        if (y < _fftLeftSpect.Count)
                        {
                            // for each row
                            for (int x = 0; x < width; x++, pixel += 4)
                            {
                                double amplitude = ((double[])_fftLeftSpect[_fftLeftSpect.Count - y - 1])[(int)(((double)(_fftLeft.Length) / (double)(width)) * x)];
                                double color = GetColor(min, max, range, amplitude);
                                pixel[0] = (byte)0;
                                pixel[1] = (byte)color;
                                pixel[2] = (byte)0;
                                pixel[3] = (byte)255;
                            }
                            pixel += offset;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            // unlock image
            canvas.UnlockBits(data);

            // Clean up
            pictureBox.Image = canvas;
            offScreenDC.Dispose();
        }
        /// <summary>
        /// Render waterfall spectrogram to PictureBox
        /// </summary>
        /// <param name="pictureBox"></param>
        public void RenderSpectrogramRight(ref PictureBox pictureBox)
        {
            Bitmap canvas = new Bitmap(pictureBox.Width, pictureBox.Height);
            Graphics offScreenDC = Graphics.FromImage(canvas);

            // Determine channnel boundries
            int width = canvas.Width;
            int height = canvas.Height;

            double min = double.MaxValue;
            double max = double.MinValue;
            double range = 0;

            if (height > _maxHeightRightSpect)
                _maxHeightRightSpect = height;

            // get min/max
            for (int w = 0; w < _fftRightSpect.Count; w++)
                for (int x = 0; x < ((double[])_fftRightSpect[w]).Length; x++)
                {
                    double amplitude = ((double[])_fftRightSpect[w])[x];
                    if (min > amplitude)
                    {
                        min = amplitude;
                    }
                    if (max < amplitude)
                    {
                        max = amplitude;
                    }
                }

            // get range
            if (min < 0 || max < 0)
                if (min < 0 && max < 0)
                    range = max - min;
                else
                    range = Math.Abs(min) + max;
            else
                range = max - min;

            // lock image
            PixelFormat format = canvas.PixelFormat;
            BitmapData data = canvas.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, format);
            int stride = data.Stride;
            int offset = stride - width * 4;

            try
            {
                unsafe
                {
                    byte* pixel = (byte*)data.Scan0.ToPointer();

                    // for each cloumn
                    for (int y = 0; y <= height; y++)
                    {
                        if (y < _fftRightSpect.Count)
                        {
                            // for each row
                            for (int x = 0; x < width; x++, pixel += 4)
                            {
                                double amplitude = ((double[])_fftRightSpect[_fftRightSpect.Count - y - 1])[(int)(((double)(_fftRight.Length) / (double)(width)) * x)];
                                double color = GetColor(min, max, range, amplitude);
                                pixel[0] = (byte)0;
                                pixel[1] = (byte)color;
                                pixel[2] = (byte)0;
                                pixel[3] = (byte)255;
                            }
                            pixel += offset;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            // unlock image
            canvas.UnlockBits(data);

            // Clean up
            pictureBox.Image = canvas;
            offScreenDC.Dispose();
        }



        /// <summary>
        /// Render time domain to PictureBox
        /// </summary>
        /// <param name="pictureBox"></param>
        public void RenderTimeDomainLeft(ref PictureBox pictureBox)
        {
            // Set up for drawing
            Bitmap canvas = new Bitmap(pictureBox.Width, pictureBox.Height);
            Graphics offScreenDC = Graphics.FromImage(canvas);
            Pen pen = new System.Drawing.Pen(Color.WhiteSmoke);

            // Determine channnel boundries
            int width = canvas.Width;
            int height = canvas.Height;
            double center = height / 2;

            // Draw left channel
            double scale = 0.5 * height / 32768;  // a 16 bit sample has values from -32768 to 32767
            int xPrev = 0, yPrev = 0;
            for (int x = 0; x < width; x++)
            {
                int y = (int)(center + (_waveLeft[_waveLeft.Length / width * x] * scale));
                if (x == 0)
                {
                    xPrev = 0;
                    yPrev = y;
                }
                else
                {
                    pen.Color = Color.Green;
                    offScreenDC.DrawLine(pen, xPrev, yPrev, x, y);
                    xPrev = x;
                    yPrev = y;
                }
            }

            // Clean up
            pictureBox.Image = canvas;
            offScreenDC.Dispose();
        }
        /// <summary>
        /// Render time domain to PictureBox
        /// </summary>
        /// <param name="pictureBox"></param>
        public void RenderTimeDomainRight(ref PictureBox pictureBox)
        {
            // Set up for drawing
            Bitmap canvas = new Bitmap(pictureBox.Width, pictureBox.Height);
            Graphics offScreenDC = Graphics.FromImage(canvas);
            Pen pen = new System.Drawing.Pen(Color.WhiteSmoke);

            // Determine channnel boundries
            int width = canvas.Width;
            int height = canvas.Height;
            double center = height / 2;

            // Draw left channel
            double scale = 0.5 * height / 32768;  // a 16 bit sample has values from -32768 to 32767
            int xPrev = 0, yPrev = 0;
            for (int x = 0; x < width; x++)
            {
                int y = (int)(center + (_waveRight[_waveRight.Length / width * x] * scale));
                if (x == 0)
                {
                    xPrev = 0;
                    yPrev = y;
                }
                else
                {
                    pen.Color = Color.Green;
                    offScreenDC.DrawLine(pen, xPrev, yPrev, x, y);
                    xPrev = x;
                    yPrev = y;
                }
            }

            // Clean up
            pictureBox.Image = canvas;
            offScreenDC.Dispose();
        }


        public AudioFrame()
        {
        }
        public AudioFrame(bool isTest)
        {
            _isTest = isTest;
        }

        /// <summary>
        /// Process 16 bit sample
        /// </summary>
        /// <param name="wave"></param>
        public void Process(ref byte[] wave)
        {
            IsEventActive = false;
            _waveLeft = new double[wave.Length / 4];
            _waveRight = new double[wave.Length / 4];

            if (_isTest == false)
            {
                // Split out channels from sample
                int h = 0;
                for (int i = 0; i < wave.Length; i += 4)
                {
                    _waveLeft[h] = (double)BitConverter.ToInt16(wave, i);
                    if (IsDetectingEvents == true)
                        if (_waveLeft[h] > AmplitudeThreshold || _waveLeft[h] < -AmplitudeThreshold)
                            IsEventActive = true;
                    _waveRight[h] = (double)BitConverter.ToInt16(wave, i + 2);
                    if (IsDetectingEvents == true)
                        if (_waveLeft[h] > AmplitudeThreshold || _waveLeft[h] < -AmplitudeThreshold)
                            IsEventActive = true;
                    h++;
                }
            }
            else
            {
                // Generate artificial sample for testing
                _signalGenerator = new SignalGenerator();
                _signalGenerator.SetWaveform("Sine");
                _signalGenerator.SetSamplingRate(44100);
                _signalGenerator.SetSamples(8192);
                _signalGenerator.SetFrequency(4096);
                _signalGenerator.SetAmplitude(32768);
                _waveLeft = _signalGenerator.GenerateSignal();
                _waveRight = _signalGenerator.GenerateSignal();
            }

            // Generate frequency domain data in decibels
            _fftLeft = FourierTransform.FFT(ref _waveLeft);
            _fftLeftSpect.Add(_fftLeft);
            if (_fftLeftSpect.Count > _maxHeightLeftSpect)
                _fftLeftSpect.RemoveAt(0);
            _fftRight = FourierTransform.FFT(ref _waveRight);
            _fftRightSpect.Add(_fftRight);
            if (_fftRightSpect.Count > _maxHeightRightSpect)
                _fftRightSpect.RemoveAt(0);
        }

       

        /// <summary>
        /// Render frequency domain to PictureBox
        /// </summary>
        /// <param name="pictureBox"></param>
        /// <param name="samples"></param>
        public void RenderFrequencyDomainLeft(ref PictureBox pictureBox, int samples)
        {

            stopwatch.Start();
            // Set up for drawing
            Bitmap canvas = new Bitmap(pictureBox.Width, pictureBox.Height);
            Graphics offScreenDC = Graphics.FromImage(canvas);
            SolidBrush brush = new System.Drawing.SolidBrush(Color.FromArgb(128, 255, 255, 255));
            Pen pen = new System.Drawing.Pen(Color.LimeGreen);
            Font font = new Font("Arial", 10);

            // Determine channnel boundries
            int width = canvas.Width;//538;
            int height = canvas.Height;//91;

            double min = double.MaxValue;
            double minHz = 0;
            double max = double.MinValue;
            double maxHz = 0;
            double range = 0;
            double scale = 0;
            double scaleHz = (double)(samples / 2) / (double)_fftLeft.Length;

            // get left min/max
            for (int x = 0; x < _fftLeft.Length; x++)
            {
                double amplitude = _fftLeft[x];
                if (min > amplitude)
                {
                    min = amplitude;
                    minHz = (double)x * scaleHz;
                }
                if (max < amplitude)
                {
                    max = amplitude;
                    maxHz = (double)x * scaleHz;
                }
            }

            // get left range
            if (min < 0 || max < 0)
                if (min < 0 && max < 0)
                    range = max - min;
                else
                    range = Math.Abs(min) + max;
            else
                range = max - min;
            scale = range / height;

            // draw left channel
            
            for (int xAxis = 0; xAxis < width; xAxis++)
            {
                double amplitude = (double)_fftLeft[(int)(((double)(_fftLeft.Length) / (double)(width)) * xAxis)];
                if (amplitude == double.NegativeInfinity || amplitude == double.PositiveInfinity || amplitude == double.MinValue || amplitude == double.MaxValue)
                    amplitude = 0;
                int yAxis;
                if (amplitude < 0)
                    yAxis = (int)(height - ((amplitude - min) / scale));
                else
                    yAxis = (int)(0 + ((max - amplitude) / scale));
                if (yAxis < 0)
                    yAxis = 0;
                if (yAxis > height)
                    yAxis = height;
                //pen.Color = pen.Color = Color.FromArgb(0, GetColor(min, max, range, amplitude), 0);
                offScreenDC.DrawLine(pen, xAxis, height, xAxis, yAxis);
            }


            /* ALERTS */
            if ((maxHz > 17000) && (maxHz <= 18000) && (FormMain.GlobalVars.tickCount == 0) && (stopwatch.ElapsedMilliseconds > 400)) 
            {
                FormMain.GlobalVars.tickCount += 1;
                MessageBox.Show("Tone between 17kHz and 18kHz detected"); //just a simple alert - could write something to console or a logfile
                stopwatch.Reset();
            }

            if ((maxHz > 18000) && (maxHz <= 19000) && (FormMain.GlobalVars.tickCount == 0) && (stopwatch.ElapsedMilliseconds > 400))
            {
                FormMain.GlobalVars.tickCount += 1;
                MessageBox.Show("Tone between 18kHz and 19kHz detected");
                stopwatch.Reset();
            }

            if ((maxHz > 19000) && (maxHz <= 20000) && (FormMain.GlobalVars.tickCount == 0) && (stopwatch.ElapsedMilliseconds > 400))
                {
                FormMain.GlobalVars.tickCount += 1;
                MessageBox.Show("Tone between 19kHz and 20kHz detected");
                stopwatch.Reset();
                }

            if ((maxHz > 20000) && (FormMain.GlobalVars.tickCount == 0) && (stopwatch.ElapsedMilliseconds > 400))
            {
                FormMain.GlobalVars.tickCount += 1;
                MessageBox.Show("Tone greater than 20kHz detected");
                stopwatch.Reset();
            }


             offScreenDC.DrawString("Min: " + minHz.ToString(".#") + " Hz (±" + scaleHz.ToString(".#") + ") = " + min.ToString(".###") + " dB", font, brush, 0 + 1, 0 + 1);
             offScreenDC.DrawString("Max: " + maxHz.ToString(".#") + " Hz (±" + scaleHz.ToString(".#") + ") = " + max.ToString(".###") + " dB", font, brush, 0 + 1, 0 + 18);

            // Clean up
           pictureBox.Image = canvas;
           offScreenDC.Dispose();
           
        }
        /// <summary>
        /// Render frequency domain to PictureBox
        /// </summary>
        /// <param name="pictureBox"></param>
        /// <param name="samples"></param>
        public void RenderFrequencyDomainRight(ref PictureBox pictureBox, int samples)
        {
            // Set up for drawing
            Bitmap canvas = new Bitmap(pictureBox.Width, pictureBox.Height);
            Graphics offScreenDC = Graphics.FromImage(canvas);
            SolidBrush brush = new System.Drawing.SolidBrush(Color.FromArgb(128, 255, 255, 255));
            Pen pen = new System.Drawing.Pen(Color.LimeGreen);
            Font font = new Font("Arial", 10);

            // Determine channnel boundries
            int width = canvas.Width;
            int height = canvas.Height;

            double min = double.MaxValue;
            double minHz = 0;
            double max = double.MinValue;
            double maxHz = 0;
            double range = 0;
            double scale = 0;
            double scaleHz = (double)(samples / 2) / (double)_fftRight.Length;

            // get left min/max
            for (int x = 0; x < _fftRight.Length; x++)
            {
                double amplitude = _fftRight[x];
                if (min > amplitude && amplitude != double.NegativeInfinity)
                {
                    min = amplitude;
                    minHz = (double)x * scaleHz;
                }
                if (max < amplitude && amplitude != double.PositiveInfinity)
                {
                    max = amplitude;
                    maxHz = (double)x * scaleHz;
                }
            }

            // get right range
            if (min < 0 || max < 0)
                if (min < 0 && max < 0)
                    range = max - min;
                else
                    range = Math.Abs(min) + max;
            else
                range = max - min;
            scale = range / height;

            // draw right channel
            for (int xAxis = 0; xAxis < width; xAxis++)
            {
                double amplitude = (double)_fftRight[(int)(((double)(_fftRight.Length) / (double)(width)) * xAxis)];
                if (amplitude == double.NegativeInfinity || amplitude == double.PositiveInfinity || amplitude == double.MinValue || amplitude == double.MaxValue)
                    amplitude = 0;
                int yAxis;
                if (amplitude < 0)
                    yAxis = (int)(height - ((amplitude - min) / scale));
                else
                    yAxis = (int)(0 + ((max - amplitude) / scale));
                if (yAxis < 0)
                    yAxis = 0;
                if (yAxis > height)
                    yAxis = height;
                //pen.Color = pen.Color = Color.FromArgb(0, GetColor(min, max, range, amplitude), 0);
                offScreenDC.DrawLine(pen, xAxis, height, xAxis, yAxis);
            }


           
            offScreenDC.DrawString("Min: " + minHz.ToString(".#") + " Hz (±" + scaleHz.ToString(".#") + ") = " + min.ToString(".###") + " dB", font, brush, 0 + 1, 0 + 1);
            offScreenDC.DrawString("Max: " + maxHz.ToString(".#") + " Hz (±" + scaleHz.ToString(".#") + ") = " + max.ToString(".###") + " dB", font, brush, 0 + 1, 0 + 18);

            // Clean up
            pictureBox.Image = canvas;
            offScreenDC.Dispose();
        }

        

        /// <summary>
        /// Get color in the range of 0-255 for amplitude sample
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="range"></param>
        /// <param name="amplitude"></param>
        /// <returns></returns>
        private static int GetColor(double min, double max, double range, double amplitude)
        {
            double color;
            if (min != double.NegativeInfinity && min != double.MaxValue & max != double.PositiveInfinity && max != double.MinValue && range != 0)
            {
                if (min < 0 || max < 0)
                    if (min < 0 && max < 0)
                        color = (255 / range) * (Math.Abs(min) - Math.Abs(amplitude));
                    else
                        if (amplitude < 0)
                            color = (255 / range) * (Math.Abs(min) - Math.Abs(amplitude));
                        else
                            color = (255 / range) * (amplitude + Math.Abs(min));
                else
                    color = (255 / range) * (amplitude - min);
            }
            else
                color = 0;
            return (int)color;
        }
    }
}
