using Microsoft.AspNetCore.Components;

namespace CoinFlipGame.App.Components;

public partial class UpdateModal
{
    [Parameter]
    public bool IsVisible { get; set; }
    
    [Parameter]
    public EventCallback OnUpdate { get; set; }
    
    [Parameter]
    public bool IsUpdating { get; set; }
    
    [Parameter]
    public string? CurrentVersion { get; set; }
    
    [Parameter]
    public string? ApiVersion { get; set; }
}
