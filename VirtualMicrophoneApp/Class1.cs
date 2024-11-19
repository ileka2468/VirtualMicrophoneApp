using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace VirtualMicrophoneApp
{
    public class DownMixToStereoSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider sourceProvider;
        private readonly int sourceChannels;
        private readonly WaveFormat waveFormat;

        public DownMixToStereoSampleProvider(ISampleProvider sourceProvider)
        {
            this.sourceProvider = sourceProvider;
            this.sourceChannels = sourceProvider.WaveFormat.Channels;
            this.waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sourceProvider.WaveFormat.SampleRate, 2);
        }

        public WaveFormat WaveFormat => waveFormat;

        public int Read(float[] buffer, int offset, int count)
        {
            int outputSamplesRequested = count;
            int outputFramesRequested = outputSamplesRequested / waveFormat.Channels;

            int sourceSamplesRequired = outputFramesRequested * sourceChannels;
            float[] sourceBuffer = new float[sourceSamplesRequired];
            int sourceSamplesRead = sourceProvider.Read(sourceBuffer, 0, sourceSamplesRequired);

            int sourceIndex = 0;
            int outIndex = offset;

            int framesProvided = sourceSamplesRead / sourceChannels;
            int framesToCopy = framesProvided;

            for (int frame = 0; frame < framesToCopy; frame++)
            {
                float left = 0;
                float right = 0;

                if (sourceChannels == 1)
                {
                    // Mono input: duplicate the sample to both left and right channels
                    float sample = sourceBuffer[sourceIndex++];
                    left = sample;
                    right = sample;
                }
                else
                {
                    // Multichannel input: sum or average samples to left and right
                    for (int ch = 0; ch < sourceChannels; ch++)
                    {
                        float sample = sourceBuffer[sourceIndex++];
                        if (ch % 2 == 0)
                            left += sample;
                        else
                            right += sample;
                    }
                    left /= (sourceChannels / 2);
                    right /= (sourceChannels / 2);
                }

                buffer[outIndex++] = left;
                buffer[outIndex++] = right;
            }

            // Return the number of samples written to the buffer
            return (outIndex - offset);
        }
    }
}
