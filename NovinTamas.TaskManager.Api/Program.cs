using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using NovinTamas.Framework.Api;
using NovinTamas.Framework.Application.Exceptions;
using NovinTamas.TaskManager.Application.Contracts.Contracts;
using NovinTamas.TaskManager.Application.Contracts.Options;
using NovinTamas.TaskManager.Infrastructure.Config;
using NovinTamas.TaskManager.Infrastructure.Persistance.Events;
using NovinTamas.TaskManager.Infrastructure.Persistance.Services;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
});

builder.Services.Configure<MicroserviceOptions>(builder.Configuration.GetSection("Microservices"));

// ================= JWT =================
var signingKey = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SigningKey"]!));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.UseSecurityTokenValidators = true;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };
        o.Events = new JwtBearerEvents
        {
            OnTokenValidated = ctx =>
            {
                // چک revoke بر پایه‌ی cache این‌مموری محلی که از RabbitMQ پر می‌شود؛ بدون network call به IAM.
                // فقط توکن‌هایی با claim "sessioned" (لاگین شرکت/پرسنل) چک می‌شوند.
                var sessioned = ctx.Principal?.FindFirst("sessioned")?.Value;
                if (sessioned != "true") return Task.CompletedTask;

                var jti = ctx.Principal?.FindFirst("jti")?.Value;

                if (string.IsNullOrWhiteSpace(jti))
                {
                    ctx.Fail("invalid token");
                    return Task.CompletedTask;
                }

                var cache = ctx.HttpContext.RequestServices.GetRequiredService<ISessionRevocationCache>();

                if (cache.IsRevoked(jti))
                    ctx.Fail("session revoked");

                return Task.CompletedTask;
            }
        };
    });

// ================= Authorization =================
builder.Services.AddAuthorization(options =>
{
    // هر endpoint بدون [AllowAnonymous] حداقل به یک کاربر احراز-هویت‌شده نیاز دارد.
    // تفکیک مدیر شرکت / پرسنل در لایه‌ی Application انجام می‌شود چون به داده‌ی وظیفه وابسته است.
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// ================= Controllers =================
builder.Services.AddControllers(a =>
{
    a.Conventions.Add(new CqrsModelConvention());
});

// ================= Autofac =================
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
    containerBuilder.RegisterModule(new AutofacModule(builder.Configuration)));

// ================= Hosted Services =================
builder.Services.AddHostedService<OutboxDispatcher>();
builder.Services.AddHostedService<SessionRevocationConsumer>();
builder.Services.AddHostedService<DueDateReminderService>();

// ================= Mongo =================
BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

var mongoSection = builder.Configuration.GetSection("MongoClient");
var connectionString = mongoSection["ConnectionString"];
var dbName = mongoSection["DatabaseName"] ?? "NovinTamas_TaskManager";

builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));
builder.Services.AddScoped<IMongoDatabase>(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(dbName));

// ================= CORS =================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ================= Rate Limiting =================
var rateLimitPermitLimit = builder.Configuration.GetValue("RateLimiting:PermitLimit", 300);
var rateLimitWindowSeconds = builder.Configuration.GetValue("RateLimiting:WindowSeconds", 60);
var rateLimitQueueLimit = builder.Configuration.GetValue("RateLimiting:QueueLimit", 20);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/json";

        await context.HttpContext.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            ErrorCode = "ERR_429",
            ErrorMessage = "تعداد درخواست‌ها بیش از حد مجاز است. کمی بعد دوباره تلاش کنید."
        }), cancellationToken);
    };

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var partitionKey = !string.IsNullOrWhiteSpace(userId) ? $"user:{userId}" : $"ip:{remoteIp}";

        return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = rateLimitPermitLimit,
            Window = TimeSpan.FromSeconds(rateLimitWindowSeconds),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = rateLimitQueueLimit,
            AutoReplenishment = true
        });
    });
});

// ================= Swagger =================
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TaskManager API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your token}"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

// ================= Misc =================
builder.Services.AddHttpClient<IHttpClientService, HttpClientService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.Use(async (context, next) =>
{
    context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
    context.Response.Headers.TryAdd("X-Frame-Options", "SAMEORIGIN");
    context.Response.Headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");

    await next();
});

// ================= Swagger Middleware =================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "taskmanager API");
        c.RoutePrefix = "swagger";
    });
}
else
{
    app.UseSwagger(c =>
    {
        c.PreSerializeFilters.Add((swagger, httpReq) =>
        {
            swagger.Servers = new List<OpenApiServer>
            {
                new OpenApiServer { Url = "/taskmanager" }
            };
        });
    });

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/taskmanager/swagger/v1/swagger.json", "taskmanager API");
        c.RoutePrefix = "swagger";
    });
}

// ================= Exception Handling =================
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;

        string errorCode;
        string errorMessage;
        var statusCode = HttpStatusCode.InternalServerError;

        switch (exception)
        {
            case NotFoundException:
                errorCode = "ERR_404";
                errorMessage = exception.Message;
                statusCode = HttpStatusCode.NotFound;
                break;
            case UserAccessException:
            case UnauthorizedAccessException:
                errorCode = "ERR_403";
                errorMessage = exception.Message;
                statusCode = HttpStatusCode.Forbidden;
                break;
            case ArgumentException:
                errorCode = "ERR_400";
                errorMessage = exception.Message;
                statusCode = HttpStatusCode.BadRequest;
                break;
            default:
                errorCode = "ERR_500";
                errorMessage = exception?.Message ?? "Unknown error";
                break;
        }

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            ErrorCode = errorCode,
            ErrorMessage = errorMessage
        }));
    });
});

// ================= Static Files =================
var webRoot = string.IsNullOrWhiteSpace(app.Environment.WebRootPath)
    ? Path.Combine(app.Environment.ContentRootPath, "wwwroot")
    : app.Environment.WebRootPath;

var uploadsPath = Path.Combine(webRoot, "uploads");

if (!Directory.Exists(uploadsPath))
    Directory.CreateDirectory(uploadsPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

// ================= Middleware =================
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

// ================= Endpoints =================
app.MapHealthChecks("/health").AllowAnonymous();
app.MapControllers();

app.Run();
