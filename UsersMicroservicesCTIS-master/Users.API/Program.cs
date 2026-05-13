using Microsoft.EntityFrameworkCore;
using Users.APP.Domain;
using CORE.APP.Services.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container. IoC (Inversion of Control) Container
// For DbContext Injection
var connectionString = builder.Configuration.GetConnectionString(nameof(UsersDb)); // "UsersDb"
builder.Services.AddDbContext<UsersDb>(options => options.UseSqlite(connectionString));
builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<UsersDb>());

// For Mediator Injection
foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
{
    builder.Services.AddMediatR(config => config.RegisterServicesFromAssemblies(assembly));
}

builder.Services.AddSingleton<ITokenAuthService, TokenAuthService>();

// Authentication:
builder.Configuration["SecurityKey"] = "users_microservices_security_key_2025=";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(config =>
    {
        config.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["SecurityKey"] ?? string.Empty)),
            ValidIssuer = builder.Configuration["Issuer"], 
            ValidAudience = builder.Configuration["Audience"], 
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API",
        Version = "v1"
    });
    c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = """
        JWT Authorization header using the Bearer scheme.
        Enter your JWT as: "Bearer jwt"
        Example: "Bearer a1b2c3"
        """
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UsersDb>();
    var dbPath = Path.GetFullPath(db.Database.GetDbConnection().DataSource);
    Console.WriteLine($"[DB SEED] Database file path: {dbPath}");
    db.Database.EnsureCreated();

    // Seed Admin Role and User so we have an account to generate a token with
    if (!db.Roles.Any())
    {
        Console.WriteLine("[DB SEED] Seeding Roles...");
        db.Roles.Add(new Role { Name = "Admin" });
        db.Roles.Add(new Role { Name = "User" });
        db.SaveChanges();
    }
    if (!db.Users.Any())
    {
        Console.WriteLine("[DB SEED] Seeding Admin User...");
        var adminRole = db.Roles.FirstOrDefault(r => r.Name == "Admin");
        var adminUser = new User
        {
            UserName = "admin",
            Password = "admin",
            FirstName = "System",
            LastName = "Admin",
            IsActive = true,
            RegistrationDate = DateTime.Now
        };
        if (adminRole != null)
            adminUser.UserRoles.Add(new UserRole { RoleId = adminRole.Id });
        
        db.Users.Add(adminUser);
        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
