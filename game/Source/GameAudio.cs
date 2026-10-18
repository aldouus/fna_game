using SoLoudSharp;

public sealed class GameAudio : IDisposable
{
    private readonly Soloud _soloud;
    private readonly Wav _testWav;

    public GameAudio(string wavPath)
    {
        _soloud = new Soloud(SoloudInitFlags.ClipRoundoff, SoloudBackend.MiniAudio);
        _testWav = new Wav();
        var loaded = _testWav.Load(wavPath);
        if (loaded != SoloudResult.Ok)
        {
            var message = _soloud.GetErrorString(loaded);
            _testWav.Dispose();
            _soloud.Dispose();
            throw new InvalidOperationException($"SoLoud failed to load '{wavPath}': {message}");
        }
    }

    public string Backend => _soloud.BackendString;

    public SoloudBackend BackendId => _soloud.Backend;

    public uint ActiveVoiceCount => _soloud.ActiveVoiceCount;

    public double TestLengthSeconds => _testWav.Length;

    public static string FindTestWav()
    {
        string[] candidates =
        [
            Path.Combine(AppContext.BaseDirectory, "Content", "Audio", "tung.wav"),
            Path.Combine(Directory.GetCurrentDirectory(), "Content", "Audio", "tung.wav"),
        ];

        foreach (var path in candidates)
        {
            if (File.Exists(path))
                return path;
        }

        throw new FileNotFoundException("Could not find Content/Audio/tung.wav.", candidates[0]);
    }

    public void PlayTest()
    {
        var voice = _soloud.Play(_testWav);
        if (!voice.IsValid)
            throw new InvalidOperationException("SoLoud failed to start the test sound.");
    }

    public static int SelfTest()
    {
        using var audio = new GameAudio(FindTestWav());
        Console.WriteLine($"backend={audio.Backend} id={(int)audio.BackendId} length={audio.TestLengthSeconds:0.00}s");

        if (audio.BackendId is SoloudBackend.NoSound or SoloudBackend.NullDriver)
        {
            Console.Error.WriteLine("SoLoud initialized without an output device.");
            return 1;
        }

        audio.PlayTest();

        var sawVoice = false;
        var deadline = DateTime.UtcNow.AddSeconds(audio.TestLengthSeconds + 2);
        while (DateTime.UtcNow < deadline)
        {
            var voices = audio.ActiveVoiceCount;
            if (voices > 0)
                sawVoice = true;
            if (sawVoice && voices == 0)
            {
                Console.WriteLine("played");
                return 0;
            }

            Thread.Sleep(20);
        }

        Console.Error.WriteLine($"playback did not finish (voices={audio.ActiveVoiceCount}, heard={sawVoice})");
        return sawVoice ? 0 : 1;
    }

    public void Dispose()
    {
        _testWav.Dispose();
        _soloud.Dispose();
    }
}
