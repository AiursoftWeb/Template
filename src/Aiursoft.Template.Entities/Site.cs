using System.ComponentModel.DataAnnotations;

namespace Aiursoft.Template.Entities;

public class Site
{
    public int Id { get; init; }
    [Display(Name = "校区名称")]
    [MaxLength(100)]
    public required string SiteName { get; init; }
}
