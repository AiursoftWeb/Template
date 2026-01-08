using System.ComponentModel.DataAnnotations;

namespace Aiursoft.Template.Entities;

public class GlobalSetting
{
    [Key]
    public required string Key { get; set; }
    public string? Value { get; set; }
}
