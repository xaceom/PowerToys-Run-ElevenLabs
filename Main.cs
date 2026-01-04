using Wox.Plugin;
using Microsoft.PowerToys.Settings.UI.Library;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace ElevenLabs
{
    /// <summary>
    /// Main class for the ElevenLabs PowerToys Run plugin
    /// </summary>
    [Export(typeof(IPlugin))]
    public class Main : IPlugin, ISettingProvider
    {
        public static string PluginID => "A1B2C3D4E5F64A7B8C9D0E1F2A3B4C5D";

        private Settings _settings = new Settings();
        private ElevenLabsService? _elevenLabsService;
        private AudioPlayer? _audioPlayer;

        public string Name => "ElevenLabs";
        public string Description => "Text-to-Speech using ElevenLabs API";

        public IEnumerable<PluginAdditionalOption> AdditionalOptions => new List<PluginAdditionalOption>
        {
            new PluginAdditionalOption
            {
                Key = "ApiKey",
                DisplayLabel = "API Key",
                DisplayDescription = "Get your API key at https://elevenlabs.io",
                PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Textbox,
                TextValue = _settings.ApiKey
            },
            new PluginAdditionalOption
            {
                Key = "VoiceId",
                DisplayLabel = "Voice ID",
                DisplayDescription = "ID of the voice to use (default: 21m00Tcm4TlvDq8ikWAM)",
                PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Textbox,
                TextValue = _settings.VoiceId
            }
        };

        public void Init(PluginInitContext context)
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            _settings = Settings.Load();
            UpdateService();
        }

        private Assembly? OnAssemblyResolve(object? sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name).Name;
            if (string.IsNullOrEmpty(assemblyName)) return null;

            var pluginDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (string.IsNullOrEmpty(pluginDirectory)) return null;

            var assemblyPath = Path.Combine(pluginDirectory, assemblyName + ".dll");

            if (File.Exists(assemblyPath))
            {
                return Assembly.LoadFrom(assemblyPath);
            }

            return null;
        }

        private void UpdateService()
        {
            _elevenLabsService?.Dispose();
            _elevenLabsService = new ElevenLabsService(_settings);
        }

        public List<Result> Query(Query query)
        {
            var results = new List<Result>();
            var searchText = query.Search.Trim();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                return results;
            }

            results.Add(new Result
            {
                Title = "Speak text",
                SubTitle = searchText,
                IcoPath = "Images\\elevenlabs.light.png",
                Action = (e) =>
                {
                    Task.Run(async () =>
                    {
                        try
                        {
                            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
                            {
                                MessageBox.Show("API Key not configured.", "ElevenLabs Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }
                            await SpeakTextAsync(searchText);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error: {ex.Message}", "ElevenLabs Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    });
                    return true;
                }
            });

            return results;
        }

        private async Task SpeakTextAsync(string text)
        {
            if (_elevenLabsService == null) return;
            
            _audioPlayer?.Stop();
            _audioPlayer?.Dispose();
            _audioPlayer = new AudioPlayer();

            try
            {
                using var audioStream = await _elevenLabsService.SpeakTextAsync(text);
                await _audioPlayer.PlayStreamAsync(audioStream);
            }
            finally
            {
                _audioPlayer?.Dispose();
                _audioPlayer = null;
            }
        }

        public List<Result> Query(Query query, bool isGlobalQuery) => Query(query);

        public void UpdateSettings(PowerLauncherPluginSettings settings)
        {
            if (settings?.AdditionalOptions != null)
            {
                foreach (var option in settings.AdditionalOptions)
                {
                    if (option.Key == "ApiKey") _settings.ApiKey = option.TextValue ?? string.Empty;
                    else if (option.Key == "VoiceId") _settings.VoiceId = option.TextValue ?? "21m00Tcm4TlvDq8ikWAM";
                }
                _settings.Save();
                UpdateService();
            }
        }

        public Control CreateSettingPanel() => new ContentControl();
    }
}
