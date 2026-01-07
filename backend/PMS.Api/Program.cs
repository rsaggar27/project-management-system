using Microsoft.EntityFrameworkCore;
using PMS.Api.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PMS.Api.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using PMS.Api.Authorization;
using PMS.Api.Models;


var builder = WebApplication.CreateBuilder(args);

// ==============================
// SERVICES
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtConfig = builder.Configuration.GetSection("Jwt");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = ClaimTypes.NameIdentifier,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtConfig["Issuer"],
        ValidAudience = jwtConfig["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtConfig["Key"]!)
        )
    };
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthorizationHandler, WorkspaceRoleHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("WorkspaceAdmin",
        policy => policy.Requirements.Add(
            new WorkspaceRoleRequirement(WorkspaceRole.Admin)));

    options.AddPolicy("WorkspaceManager",
        policy => policy.Requirements.Add(
            new WorkspaceRoleRequirement(WorkspaceRole.Manager)));

    options.AddPolicy("WorkspaceMember",
        policy => policy.Requirements.Add(
            new WorkspaceRoleRequirement(WorkspaceRole.Member)));
});

builder.Services.AddScoped<IAuthorizationHandler, ProjectRoleHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ProjectLead",
        p => p.Requirements.Add(new ProjectRoleRequirement(ProjectRole.Lead)));

    options.AddPolicy("ProjectContributor",
        p => p.Requirements.Add(new ProjectRoleRequirement(ProjectRole.Contributor)));

    options.AddPolicy("ProjectViewer",
        p => p.Requirements.Add(new ProjectRoleRequirement(ProjectRole.Viewer)));
});


builder.Services.AddScoped<JwtService>();


// ==============================

// Controllers (important, unlike minimal APIs)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()
        );
    });


// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PMS.Api", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


// Database (PostgreSQL)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// ==============================

var app = builder.Build();

// ==============================
// PIPELINE
// ==============================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();



// Map controllers
app.MapControllers();

// ==============================
// 🔍 DB CONNECTION TEST (STEP 1)
// ==============================

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    Console.WriteLine("DB CONNECTED: " + db.Database.CanConnect());
}

// ==============================

app.Run();
