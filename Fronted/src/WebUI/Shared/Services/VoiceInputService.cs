using Microsoft.JSInterop;

namespace KPG.Timesheet.WebUI.Shared.Services;

public class VoiceInputService(IJSRuntime js)
{
    public async Task<bool> IsSupportedAsync()
        => await js.InvokeAsync<bool>("voiceInput.isSupported");

    public async Task InitAsync()
        => await js.InvokeVoidAsync("voiceInput.init");

    public async Task StartAsync<T>(DotNetObjectReference<T> dotNetRef, string lang = "es-CO")
        where T : class
        => await js.InvokeVoidAsync("voiceInput.start", dotNetRef, lang);

    public async Task StopAsync()
        => await js.InvokeVoidAsync("voiceInput.stop");
}
