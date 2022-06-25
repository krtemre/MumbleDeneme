using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace MumbleDeneme
{
    class BasicVoiceDetector : IVoiceDetector
    {
        public short VoiceDetectionSampleVolume { get; set; }
        public short NoiseDetectionSampleVolume { get; set; }

        public BasicVoiceDetector()
        {
            VoiceDetectionSampleVolume = Convert.ToInt16(short.MaxValue * 0.5f);
            NoiseDetectionSampleVolume = Convert.ToInt16(short.MaxValue * 0.25f);
        }

        private enum SoundType
        {
            NOTHING,
            NOISE,
            VOICE
        }

        private DateTime _lastDetectedSoundTime = DateTime.MinValue;
        private TimeSpan _minVoiceHoldTime = TimeSpan.FromMilliseconds(1000);
        private TimeSpan _minNoiseHoldTime = TimeSpan.FromMilliseconds(250);
        private SoundType _lastDetectedSound = SoundType.NOTHING;
    
        public bool VoiceDetected(WaveBuffer waveBuffer, int bytesRecorded)
        {
            var now = DateTime.UtcNow;

            SoundType detectedSound = DetectSound(waveBuffer, bytesRecorded, VoiceDetectionSampleVolume, NoiseDetectionSampleVolume);
            if (detectedSound != SoundType.NOTHING)
                _lastDetectedSoundTime = now;
            //Tutma sürelerini hesaba katmak için algılanan Sesi ayarlayın.
            if (_lastDetectedSound == SoundType.NOISE && (_lastDetectedSoundTime + _minNoiseHoldTime > now))
            {
                switch (detectedSound)
                {
                    case SoundType.NOTHING:
                    case SoundType.NOISE: detectedSound = SoundType.NOISE;
                        break;
                    case SoundType.VOICE: detectedSound = SoundType.VOICE;
                        break;
                }
            }
            else if(_lastDetectedSound == SoundType.VOICE && (_lastDetectedSoundTime + _minVoiceHoldTime > now))
            {
                detectedSound = SoundType.VOICE;
            }

            _lastDetectedSound = detectedSound;

            if (detectedSound == SoundType.NOTHING)
                return false;
            else
                return true;
        }

        private static SoundType DetectSound(WaveBuffer waveBuffer, int bytesRecorded, short minVoiceRecordSampleVolume, short minNoiseRecordSampleVolume)
        {
            if (minVoiceRecordSampleVolume == 0)
                return SoundType.VOICE;
            if (minNoiseRecordSampleVolume == 0)
                return SoundType.NOISE;

            SoundType result = SoundType.NOTHING;

            //check if the volume peaks above the MinRecordVolume
            // interpret as 32 bit floating point audio
            for(int i = 0; i < bytesRecorded / 4; i++)
            {
                var sample = waveBuffer.ShortBuffer[i];

                //Check voice volume threshold
                if(sample > minVoiceRecordSampleVolume || sample < -minVoiceRecordSampleVolume)
                {
                    result = SoundType.VOICE; //Sesi almak önemli olan
                    break;
                }
                else if(sample > minNoiseRecordSampleVolume || sample < -minNoiseRecordSampleVolume)
                {
                    result = SoundType.NOISE;
                }
            }
            return result;
        }
    }
}
