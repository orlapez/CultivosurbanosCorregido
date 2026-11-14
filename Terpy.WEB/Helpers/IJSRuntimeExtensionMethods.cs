using Microsoft.JSInterop;

namespace Terpy.WEB.Helpers;

public static class IJSRuntimeExtensionMethods
{
    public static ValueTask SetLocalStorage(this IJSRuntime js, string key, string content)
    {
        return js.InvokeVoidAsync("localStorage.setItem", key, content);
    }

    public static ValueTask<string?> GetLocalStorage(this IJSRuntime js, string key)
    {
        return js.InvokeAsync<string?>("localStorage.getItem", key);
    }

    public static ValueTask RemoveLocalStorage(this IJSRuntime js, string key)
    {
        return js.InvokeVoidAsync("localStorage.removeItem", key);
    }
}
