using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ElevenLabs
{
    /// <summary>
    /// Plugin settings model
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// API Key for ElevenLabs
        /// </summary>
        [JsonPropertyName("apiKey")]
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Voice ID for speech (defaults to a popular voice)
        /// </summary>
        [JsonPropertyName("voiceId")]
        public string VoiceId { get; set; } = "21m00Tcm4TlvDq8ikWAM";

        /// <summary>
        /// Path to settings file
        /// </summary>
        private static string SettingsPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Microsoft",
            "PowerToys",
            "PowerToys Run",
            "Plugins",
            "ElevenLabs",
            "settings.json"
        );

        /// <summary>
        /// Loads settings from file
        /// </summary>
        public static Settings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    var settings = JsonSerializer.Deserialize<Settings>(json);
                    return settings ?? new Settings();
                }
            }
            catch (Exception ex)
            {
                // Log error but return default settings
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
            }

            return new Settings();
        }

        /// <summary>
        /// Saves settings to file
        /// </summary>
        public void Save()
        {
            try
            {
                var directory = Path.GetDirectoryName(SettingsPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                var json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
                throw;
            }
        }
    }
}
