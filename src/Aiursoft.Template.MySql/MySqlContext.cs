using Aiursoft.Template.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Template.MySql;

public class MySqlContext(DbContextOptions<MySqlContext> options) : TemplateDbContext(options);
