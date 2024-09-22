using System.ComponentModel.DataAnnotations;

namespace Frituquim.Models;

public enum ConversionHardware
{
    [Display(Name = "Nvidia")]
    Nvidia,
    
    [Display(Name = "Intel Quick Sync")]
    IntelQuickSync,
    
    [Display(Name = "CPU")]
    Cpu
}