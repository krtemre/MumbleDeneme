using NAudio.Wave;

namespace MumbleDeneme
{
    public interface IVoiceDetector
    {
        bool VoiceDetected(WaveBuffer waveBuffer, int bytesRecorded);
    }
}
