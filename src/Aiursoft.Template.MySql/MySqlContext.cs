using System.Diagnostics.CodeAnalysis;
using Aiursoft.Template.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Template.MySql;

[ExcludeFromCodeCoverage]

public class MySqlContext(DbContextOptions<MySqlContext> options) : TemplateDbContext(options);
