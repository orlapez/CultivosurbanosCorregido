using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using Terpy.API.Data;
using Terpy.API.Helpers;
using Terpy.Shared.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddCors(options => options.AddPolicy("TerpyWeb", policy =>
    policy.WithOrigins("https://localhost:7080", "http://localhost:5117")
        .AllowAnyMethod()
        .AllowAnyHeader()));
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Terpy API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Escribe Bearer, un espacio y el token obtenido en api/accounts/Login.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});
builder.Services.AddDbContext<DataContext>(options => options.UseSqlServer("name=DefaultConnection"));
builder.Services.AddTransient<SeedDb>();
builder.Services.AddIdentity<User, IdentityRole>(options => options.User.RequireUniqueEmail = true)
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();
builder.Services.AddScoped<IUserHelper, UserHelper>();

var jwtKey = builder.Configuration["jwtKey"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
{
    throw new InvalidOperationException(
        "Configura jwtKey con al menos 32 caracteres en appsettings.Development.json.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    });

var app = builder.Build();

SeedData(app);

void SeedData(WebApplication application)
{
    using (var scope = application.Services.CreateScope())
    {
        var seed = scope.ServiceProvider.GetRequiredService<SeedDb>();
        seed.SeedAsync().Wait();
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Terpy API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("TerpyWeb");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
