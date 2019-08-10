# SoundAlert

This is a proof-of-concept .NET project for detecting noise at particular frequencies above an amplitude threshold.

![Image1](https://github.com/catz3/SoundAlert-example/blob/master/SoundAlert.PNG)

The performance of the application will depend greatly on the quality of the microphone and soundcard being used. 

This application is presented as-is, for personal use, and as a PoC *ONLY*. It should not under any circumstances be used to evaluate if there is any risk of damage to hearing or any other adverse effects arising from exposure to noise, nor should it be used to perform any sort of safety/compliance-related testing or assessment. The results shown by this tool may not be accurate. If you are in any doubt or have concerns about noise levels to which you may be exposed, we recommend you employ the services of a trained professional to use suitably calibrated equipment.

This program is a personal project and is not affiliated or connected with my employer or any other organisation or agency.

This application is adapted from Jeff Morton's "Sound Activated Recorder with Spectrogram", at https://www.codeproject.com/Articles/22951/Sound-Activated-Recorder-with-Spectrogram-in-C, and licensed under the GNU GPLv3.

USING THE TOOL:
- Set AmplitudeThreshold in AudioFrame.cs (default 1000)
- For the PoC, monitoring is done in the RenderFrequencyDomainLeft() method in AudioFrame.cs. Change frequencies and durations here
- Build the solution for x86

When noise is detected between the specified frequency ranges and above the amplitude threshold, a messagebox alert will pop up (although you could change this action, e.g. log to a file). An example is shown here:

![Image2](https://github.com/catz3/SoundAlert-example/blob/master/SoundAlert2.PNG)

TROUBLESHOOTING:
- If you don't see any results in the spectrogram, frequency domain, or time domain displays, check that your microphone is enabled, and check the levels
- If still no results, try an external microphone if you have one available

