using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ElevenLabs
{
    /// <summary>
    /// Service for interacting with ElevenLabs API
    /// </summary>
    public class ElevenLabsService : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly Settings _settings;

        /// <summary>
        /// Service constructor
        /// </summary>
        public ElevenLabsService(Settings settings)
        {
            _settings = settings;
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        /// <summary>
        /// Speaks text via ElevenLabs API and returns an audio stream
        /// </summary>
        /// <param name="text">Text to speak</param>
        /// <returns>Stream with audio data</returns>
        /// <exception cref="HttpRequestException">Network or API error</exception>
        /// <exception cref="InvalidOperationException">Missing API key</exception>
        public async Task<Stream> SpeakTextAsync(string text)
        {
            // Verify API key presence
            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                throw new InvalidOperationException("API Key not configured. Please set the API key in plugin settings.");
            }

            // Verify Voice ID presence
            if (string.IsNullOrWhiteSpace(_settings.VoiceId))
            {
                throw new InvalidOperationException("Voice ID not configured.");
            }

            // Verify text presence
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Text to speak cannot be empty.", nameof(text));
            }

            try
            {
                // Build URL for streaming endpoint
                var url = $"https://api.elevenlabs.io/v1/text-to-speech/{_settings.VoiceId}/stream";

                // Prepare request body
                var requestBody = new
                {
                    text = text,
                    model_id = "eleven_turbo_v2_5",
                    voice_settings = new
                    {
                        stability = 0.5,
                        similarity_boost = 0.75
                    }
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Set API key header
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("xi-api-key", _settings.ApiKey);
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("audio/mpeg"));

                // Send request
                var response = await _httpClient.PostAsync(url, content);

                // Process response
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    
                    // Parse error if possible
                    string errorMessage = "Unknown API error";
                    try
                    {
                        var errorJson = JsonSerializer.Deserialize<JsonElement>(errorContent);
                        if (errorJson.TryGetProperty("detail", out var detail))
                        {
                            if (detail.ValueKind == JsonValueKind.Object && detail.TryGetProperty("message", out var message))
                            {
                                errorMessage = message.GetString() ?? errorMessage;
                            }
                            else if (detail.ValueKind == JsonValueKind.String)
                            {
                                errorMessage = detail.GetString() ?? errorMessage;
                            }
                        }
                    }
                    catch
                    {
                        // Use default message if parsing failed
                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            errorMessage = "Invalid API Key. Please check your settings.";
                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            errorMessage = "Voice ID not found. Please check your settings.";
                        }
                        else
                        {
                            errorMessage = $"API Error: {response.StatusCode} - {errorContent}";
                        }
                    }

                    throw new HttpRequestException(errorMessage, null, response.StatusCode);
                }

                // Return audio data stream
                return await response.Content.ReadAsStreamAsync();
            }
            catch (TaskCanceledException)
            {
                throw new HttpRequestException("Server response timed out. Check your internet connection.");
            }
            catch (HttpRequestException)
            {
                // Rethrow to preserve original error message
                throw;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"Error contacting ElevenLabs API: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Disposes resources
        /// </summary>
        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
