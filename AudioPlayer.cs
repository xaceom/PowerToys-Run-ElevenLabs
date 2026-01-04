using NAudio.Wave;
using System.Diagnostics;
using System.IO;

namespace ElevenLabs
{
    /// <summary>
    /// Class for audio stream playback
    /// </summary>
    public class AudioPlayer : IDisposable
    {
        private WaveOutEvent? _waveOut;
        private bool _disposed = false;

        /// <summary>
        /// Plays an audio stream asynchronously
        /// </summary>
        /// <param name="audioStream">Stream containing audio data (MP3)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task PlayStreamAsync(Stream audioStream, CancellationToken cancellationToken = default)
        {
            if (audioStream == null)
            {
                throw new ArgumentNullException(nameof(audioStream));
            }

            try
            {
                // Use Task.Run for background playback
                await Task.Run(async () =>
                {
                    // Create WaveStream from MP3 stream
                    using var mp3Reader = new Mp3FileReader(audioStream);
                    
                    // Create WaveOut for playback
                    _waveOut = new WaveOutEvent();
                    _waveOut.Init(mp3Reader);
                    
                    // Subscribe to playback completion event
                    var completionSource = new TaskCompletionSource<bool>();
                    _waveOut.PlaybackStopped += (sender, e) =>
                    {
                        completionSource.TrySetResult(true);
                    };

                    // Start playback
                    _waveOut.Play();

                    // Wait for playback completion or cancellation
                    using (cancellationToken.Register(() =>
                    {
                        _waveOut?.Stop();
                        completionSource.TrySetCanceled();
                    }))
                    {
                        try
                        {
                            await completionSource.Task;
                        }
                        catch (OperationCanceledException)
                        {
                            // Playback was cancelled
                        }
                    }
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Audio playback error: {ex.Message}");
                throw new InvalidOperationException($"Failed to play audio: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Stops playback
        /// </summary>
        public void Stop()
        {
            _waveOut?.Stop();
        }

        /// <summary>
        /// Disposes resources
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _waveOut?.Stop();
                _waveOut?.Dispose();
                _disposed = true;
            }
        }
    }
}
