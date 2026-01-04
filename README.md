# ElevenLabs TTS Plugin for PowerToys Run

A PowerToys Run plugin that provides text-to-speech functionality using the ElevenLabs API.

## Features
- Convert text to high-quality audio using ElevenLabs voices.
- Integrated playback within the PowerToys Run interface.
- Easy configuration of API Key and Voice ID through PowerToys settings.

## Installation

### Prerequisites
- [PowerToys](https://github.com/microsoft/PowerToys) installed.
- .NET 8.0 Desktop Runtime.
- An [ElevenLabs](https://elevenlabs.io) account and API key.

### Automatic Installation (Recommended)
1. Clone this repository.
2. Build the project using the .NET CLI:
   ```powershell
   dotnet build
   ```
   The plugin will be automatically copied to your PowerToys plugins directory:
   `%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\ElevenLabs`

3. Restart PowerToys.

### Manual Installation
1. Download the latest release from the [Releases](https://github.com/YOUR_USERNAME/PowerToys-Run-ElevenLabs/releases) page.
2. Extract the archive content to:
   `%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\ElevenLabs`
3. Restart PowerToys.

## Configuration
1. Open **PowerToys Settings**.
2. Go to the **PowerToys Run** section.
3. Scroll down to find the **ElevenLabs TTS** plugin.
4. Enter your configuration:
   - **API Key**: Your ElevenLabs API key.
   - **Voice ID**: The ID of the voice you want to use (defaults to a high-quality voice).

## Usage
1. Open PowerToys Run (Default: `Alt + Space`).
2. Type the action keyword followed by your text:
   ```text
   ### Hello, this is a test.
   ```
3. Press **Enter** to speak the text.

## Credits
- [ElevenLabs](https://elevenlabs.io) for the amazing TTS API.
- [NAudio](https://github.com/naudio/NAudio) for audio playback.
- [Community PowerToys Run Plugin Dependencies](https://github.com/microsoft/PowerToys) for the plugin framework.

## License
MIT License. See [LICENSE](LICENSE) for details.
