using NAudio.Wave;
using NAudio.CoreAudioApi;
using NAudio.Wave.SampleProviders;

namespace VirtualMicrophoneApp
{
    public partial class Form1 : Form
    {
        private List<WasapiCapture> captureDevices = new List<WasapiCapture>();
        private WasapiLoopbackCapture loopbackCapture;
        private WasapiOut playbackDevice;
        private MMDevice virtualCableDevice;
        private MixingSampleProvider mixer;
        private WaveFormat mixFormat;

        public Form1()
        {
            InitializeComponent();
            InitializeAudioSources();
            buttonStop.Enabled = false;
        }

        private void InitializeAudioSources()
        {
            var enumerator = new MMDeviceEnumerator();

            // Get all active audio input devices (microphones)
            var inputDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);

            // Add input devices to the checkedListBoxSources
            foreach (var device in inputDevices)
            {
                checkedListBoxSources.Items.Add(new AudioSourceItem
                {
                    Name = device.FriendlyName,
                    Type = AudioSourceType.Device,
                    Device = device
                });
            }

            // Add "System Audio" option
            checkedListBoxSources.Items.Add(new AudioSourceItem
            {
                Name = "System Audio",
                Type = AudioSourceType.SystemAudio
            });

            // Find the virtual audio cable device (CABLE Input)
            var renderDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            virtualCableDevice = renderDevices.FirstOrDefault(d => d.FriendlyName.Contains("CABLE Input"));
            if (virtualCableDevice == null)
            {
                MessageBox.Show("Virtual Audio Cable not found. Please install VB-Audio Virtual Cable.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (checkedListBoxSources.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select at least one input source.", "No Input Source Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            StartVirtualMicrophone();
            buttonStart.Enabled = false;
            buttonStop.Enabled = true;
            labelStatus.Text = "Virtual microphone started.";
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            StopVirtualMicrophone();
            buttonStart.Enabled = true;
            buttonStop.Enabled = false;
            labelStatus.Text = "Virtual microphone stopped.";
        }

        private void StartVirtualMicrophone()
        {
            var mixerInputs = new List<ISampleProvider>();

            // Define a common WaveFormat for mixing (e.g., 44.1 kHz, 2 channels)
            mixFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

            // Initialize capture devices
            foreach (AudioSourceItem item in checkedListBoxSources.CheckedItems)
            {
                ISampleProvider resampledProvider = null;

                if (item.Type == AudioSourceType.Device)
                {
                    var device = item.Device;
                    var captureDevice = new WasapiCapture(device);
                    captureDevice.DataAvailable += CaptureDevice_DataAvailable;
                    captureDevice.RecordingStopped += CaptureDevice_RecordingStopped;
                    captureDevices.Add(captureDevice);

                    var waveProvider = new WaveInProvider(captureDevice);
                    var sampleProvider = waveProvider.ToSampleProvider();

                    // Resample to common sample rate
                    resampledProvider = new WdlResamplingSampleProvider(sampleProvider, mixFormat.SampleRate);
                }
                else if (item.Type == AudioSourceType.SystemAudio)
                {
                    // Initialize loopback capture
                    loopbackCapture = new WasapiLoopbackCapture();
                    loopbackCapture.DataAvailable += LoopbackCapture_DataAvailable;
                    loopbackCapture.RecordingStopped += LoopbackCapture_RecordingStopped;

                    var waveProvider = new WaveInProvider(loopbackCapture);
                    var sampleProvider = waveProvider.ToSampleProvider();

                    // Resample to common sample rate
                    resampledProvider = new WdlResamplingSampleProvider(sampleProvider, mixFormat.SampleRate);
                }

                // Ensure channel count matches
                if (resampledProvider.WaveFormat.Channels != mixFormat.Channels)
                {
                    // Use custom DownMixToStereoSampleProvider to convert to stereo
                    resampledProvider = new DownMixToStereoSampleProvider(resampledProvider);
                }

                mixerInputs.Add(resampledProvider);
            }

            if (mixerInputs.Count == 0)
            {
                MessageBox.Show("No input sources selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Create mixer
            mixer = new MixingSampleProvider(mixFormat);

            // Add inputs to mixer individually
            foreach (var input in mixerInputs)
            {
                mixer.AddMixerInput(input);
            }

            // Initialize playback device (virtual audio cable)
            playbackDevice = new WasapiOut(virtualCableDevice, AudioClientShareMode.Shared, false, 200);
            var mixerWaveProvider = mixer.ToWaveProvider();
            playbackDevice.Init(mixerWaveProvider);

            // Start capturing and playback
            playbackDevice.Play();

            foreach (var captureDevice in captureDevices)
            {
                captureDevice.StartRecording();
            }

            loopbackCapture?.StartRecording();
        }

        private void StopVirtualMicrophone()
        {
            foreach (var captureDevice in captureDevices)
            {
                captureDevice.StopRecording();
                captureDevice.Dispose();
            }
            captureDevices.Clear();

            if (loopbackCapture != null)
            {
                loopbackCapture.StopRecording();
                loopbackCapture.Dispose();
                loopbackCapture = null;
            }

            playbackDevice?.Stop();
            playbackDevice?.Dispose();
            playbackDevice = null;

            mixer = null;
        }

        private void CaptureDevice_DataAvailable(object sender, WaveInEventArgs e)
        {
            // Data is handled by the mixer automatically
        }

        private void LoopbackCapture_DataAvailable(object sender, WaveInEventArgs e)
        {
            // Data is handled by the mixer automatically
        }

        private void CaptureDevice_RecordingStopped(object sender, StoppedEventArgs e)
        {
            if (e.Exception != null)
            {
                MessageBox.Show($"An error occurred during recording: {e.Exception.Message}");
            }
        }

        private void LoopbackCapture_RecordingStopped(object sender, StoppedEventArgs e)
        {
            if (e.Exception != null)
            {
                MessageBox.Show($"An error occurred during loopback recording: {e.Exception.Message}");
            }
        }
    }

    public enum AudioSourceType
    {
        Device,
        SystemAudio
    }

    public class AudioSourceItem
    {
        public string Name { get; set; }
        public AudioSourceType Type { get; set; }
        public MMDevice Device { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
