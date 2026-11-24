using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using System.Text.Json.Serialization;
using BloodDonor.API.Middleware;
using BloodDonor.API.RateLimiting;
using BloodDonor.Application.Auth;
using BloodDonor.Application.BloodRequests;
using BloodDonor.Application.BloodVerification;
using BloodDonor.Application.Compatibility;
using BloodDonor.Application.DonorRequests;
using BloodDonor.Application.Donors;
using BloodDonor.Application.Hospitals;
using BloodDonor.Application.Matching;
using BloodDonor.Application.Notifications;
using BloodDonor.Infrastructure.BloodRequests;
using BloodDonor.Infrastructure.BloodVerification;
using BloodDonor.Infrastructure.DonorRequests;
using BloodDonor.Infrastructure.Donors;
using BloodDonor.Infrastructure.Hospitals;
using BloodDonor.Infrastructure.Identity;
using BloodDonor.Infrastructure.Matching;
using BloodDonor.Infrastructure.Notifications;
using BloodDonor.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Request body limit: every endpoint takes small JSON, so 1 MB is generous.
// A future upload endpoint will raise this for itself only.
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 1_048_576;
});

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Enums travel as names ("APlus", "Critical"), so reordering an enum can never silently break clients.
        // Numbers are still accepted in requests, so earlier test bodies keep working.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<IUserRegistrationService, UserRegistrationService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IDonorProfileService, DonorProfileService>();
builder.Services.AddScoped<IDonorBloodGroupVerificationService, DonorBloodGroupVerificationService>();
builder.Services.AddScoped<IReviewBloodGroupVerificationService, ReviewBloodGroupVerificationService>();
builder.Services.AddScoped<IBloodGroupVerificationQueryService, BloodGroupVerificationQueryService>();
builder.Services.AddScoped<IResubmitBloodGroupService, ResubmitBloodGroupService>();
builder.Services.AddScoped<ICreateHospitalService, CreateHospitalService>();
builder.Services.AddScoped<IHospitalQueryService, HospitalQueryService>();
builder.Services.AddScoped<IHospitalAdminQueryService, HospitalAdminQueryService>();
builder.Services.AddScoped<IUpdateHospitalStatusService, UpdateHospitalStatusService>();
builder.Services.AddScoped<IUpdateHospitalService, UpdateHospitalService>();
builder.Services.AddScoped<ICreateHospitalAdminService, CreateHospitalAdminService>();
builder.Services.AddScoped<ICreateHospitalStaffService, CreateHospitalStaffService>();
builder.Services.AddScoped<IHospitalStaffQueryService, HospitalStaffQueryService>();
//It’s a singleton because the service is stateless:
builder.Services.AddSingleton<IBloodCompatibilityService, BloodCompatibilityService>();
builder.Services.AddScoped<IBloodRequestService, BloodRequestService>();
builder.Services.AddScoped<IBloodRequestQueryService, BloodRequestQueryService>();
builder.Services.AddScoped<IDonorAvailabilityService, DonorAvailabilityService>();
builder.Services.AddScoped<IDonorMatchingService, DonorMatchingService>();
builder.Services.AddScoped<IHospitalDonorRequestService, HospitalDonorRequestService>();
builder.Services.AddScoped<IDonorOwnRequestService, DonorOwnRequestService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience is not configured.");
var jwtSigningKey = builder.Configuration["Jwt:SigningKey"] ?? throw new InvalidOperationException("Jwt:SigningKey is not configured.");

// Fail fast on a weak signing key instead of running with weak token security.
if (jwtSigningKey.Length < 32)
{
    throw new InvalidOperationException("Jwt:SigningKey must be at least 32 characters long.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
        ValidateLifetime = true,
        RequireExpirationTime = true,
        ClockSkew = TimeSpan.FromSeconds(30),
        ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 }
    };
});

// CORS: an allow-list from configuration. An empty list (the default outside Development)
// means no browser origin is allowed. No credentials: the JWT travels in a header, not a cookie.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                .WithMethods("GET", "POST", "PUT", "PATCH")
                .WithHeaders("Content-Type", "Authorization");
        }
    });
});

// Rate limiting (framework-native, in memory, per instance).
// NOTE: the client IP is the connection IP. Behind a reverse proxy every client would share one
// bucket until forwarded headers are configured. That is a required item at deployment.
static string GetClientIp(HttpContext httpContext)
{
    return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}

static string GetUserOrIpKey(HttpContext httpContext)
{
    var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

    return userId != null ? $"user:{userId}" : $"ip:{GetClientIp(httpContext)}";
}

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, cancellationToken) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers["Retry-After"] =
                ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
        }

        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        await context.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Title = "Too many requests.",
            Detail = "The rate limit was exceeded. Try again later."
        }, cancellationToken);
    };

    // Default for every endpoint: 120 requests per minute per user (per IP when anonymous).
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(GetUserOrIpKey(httpContext), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 120,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));

    // Login and register: 10 requests per minute per IP.
    options.AddPolicy(RateLimitPolicies.Auth, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter($"auth:{GetClientIp(httpContext)}", _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));

    // Donor contact disclosure: 20 requests per minute per user.
    options.AddPolicy(RateLimitPolicies.Contact, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter($"contact:{GetUserOrIpKey(httpContext)}", _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 20,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestMethod
        | HttpLoggingFields.RequestPath
        | HttpLoggingFields.ResponseStatusCode
        | HttpLoggingFields.Duration;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseExceptionHandler();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpLogging();

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await RoleSeeder.SeedRolesAsync(roleManager);
}

app.Run();