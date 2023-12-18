using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyBGList.Constants;
/*using MyBGList;*/
using MyBGList.Models;
using MyBGList.Swagger;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MyBGList.GraphQL;
using MyBGList.gRPC;
using System.Reflection;
using Swashbuckle.AspNetCore.Annotations;
using MyBGList.Attributes;

var builder = WebApplication.CreateBuilder(args);

/*builder.Logging
    .ClearProviders()
    .AddSimpleConsole(options =>
    {
        options.SingleLine = true;
        options.TimestampFormat = "HH:mm:ss ";
        options.UseUtcTimestamp = true;
    })
    .AddDebug();*/

/*builder.Logging
    .ClearProviders()
    .AddSimpleConsole()
    .AddDebug()
    .AddApplicationInsights(
        telemetry =>
            telemetry.ConnectionString = builder.Configuration[
                "Azure:ApplicationInsights:ConnectionString"
            ],
        loggerOptions => { }
    );
;*/

builder.Logging
    .ClearProviders()
    .AddJsonConsole(options =>
    {
        options.TimestampFormat = "HH:mm";
        options.UseUtcTimestamp = true;
    })
    .AddDebug();

// builder.Logging.ClearProviders().AddSimpleConsole().AddDebug();

builder.Host.UseSerilog(
    (ctx, lc) =>
    {
        /*        lc.MinimumLevel.Is(Serilog.Events.LogEventLevel.Warning);
                lc.MinimumLevel.Override("MyBGList", Serilog.Events.LogEventLevel.Information);*/
        lc.ReadFrom.Configuration(ctx.Configuration);
        lc.Enrich.WithMachineName();
        lc.Enrich.WithThreadId();
        lc.Enrich.WithThreadName();
        lc.WriteTo.File(
            "Logs/log.txt",
            outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] "
                + "[{MachineName} #{ThreadId} {ThreadName}] "
                + "{Message:lj}{NewLine}{Exception}",
            rollingInterval: RollingInterval.Day
        );
        lc.WriteTo.File(
            "Logs/errors.txt",
            outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] "
                + "[{MachineName} #{ThreadId} {ThreadName}] "
                + "{Message:lj}{NewLine}{Exception}",
            restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error,
            rollingInterval: RollingInterval.Day
        );
        lc.WriteTo.MSSqlServer(
            // restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
            connectionString: ctx.Configuration.GetConnectionString("DefaultConnection"),
            sinkOptions: new MSSqlServerSinkOptions
            {
                TableName = "LogEvents",
                AutoCreateSqlTable = true
            },
            columnOptions: new ColumnOptions()
            {
                AdditionalColumns = new SqlColumn[]
                {
                    new SqlColumn()
                    {
                        ColumnName = "SourceContext",
                        PropertyName = "SourceContext",
                        DataType = System.Data.SqlDbType.NVarChar
                    }
                }
            }
        );
    },
    writeToProviders: true
);

builder.Services.AddControllers(options =>
{
    options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(
        (x) => $"The value '{x}' is invalid."
    );
    options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(
        (x) => $"The field {x} must be a number."
    );
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
        (x, y) => $"The value '{x}' is not valid for {y}."
    );
    options.ModelBindingMessageProvider.SetMissingKeyOrValueAccessor(() => $"A value is required.");

    options.CacheProfiles.Add("NoCache", new CacheProfile() { NoStore = true });
    options.CacheProfiles.Add(
        "Any-60",
        new CacheProfile() { Location = ResponseCacheLocation.Any, Duration = 60 }
    );
    options.CacheProfiles.Add(
        "Client-120",
        new CacheProfile() { Location = ResponseCacheLocation.Client, Duration = 120 }
    );
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(System.IO.Path.Combine(AppContext.BaseDirectory, xmlFilename));

    options.EnableAnnotations();

    options.ResolveConflictingActions(apiDesc => apiDesc.First());
    options.ParameterFilter<SortColumnFilter>();
    options.ParameterFilter<SortOrderFilter>();

    // JWT Auth
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "bearer"
        }
    );

    // JWT Auth
    /*    options.AddSecurityRequirement(
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            }
        );*/
    options.OperationFilter<AuthRequirementFilter>();
    // App title
    options.DocumentFilter<CustomDocumentFilter>();
    // Password
    options.RequestBodyFilter<PasswordRequestFilter>();
    options.SchemaFilter<CustomKeyValueFilter>();
    options.RequestBodyFilter<UsernameRequestFilter>();
});

/*builder.Services.AddCors(
    options =>
        options.AddDefaultPolicy(cfg =>
        {
            cfg.AllowAnyOrigin();
            cfg.AllowAnyHeader();
            cfg.AllowAnyMethod();
        })
);*/

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(cfg =>
    {
        cfg.WithOrigins(builder.Configuration["AllowedOrigins"]!);
        cfg.AllowAnyHeader();
        cfg.AllowAnyMethod();
    });
    options.AddPolicy(
        name: "AnyOrigin",
        cfg =>
        {
            cfg.AllowAnyOrigin();
            cfg.AllowAnyHeader();
            cfg.AllowAnyMethod();
        }
    );
});

builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// GraphQL
builder.Services
    .AddGraphQLServer()
    .AddAuthorization()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddProjections()
    .AddFiltering()
    .AddSorting();

// gRPC
builder.Services.AddGrpc();

// JWT Auth
builder.Services
    .AddIdentity<ApiUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 12;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();

// JWT Auth
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            options.DefaultChallengeScheme =
            options.DefaultForbidScheme =
            options.DefaultScheme =
            options.DefaultSignInScheme =
            options.DefaultSignOutScheme =
                JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            RequireExpirationTime = true,
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidAudience = builder.Configuration["JWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"]!)
            )
        };
    });

// JWT Auth
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "ModeratorWithMobilePhone",
        policy =>
            policy
                .RequireClaim(ClaimTypes.Role, RoleNames.Moderator)
                .RequireClaim(ClaimTypes.MobilePhone)
    );

    options.AddPolicy(
        "MinAge18",
        policy =>
            policy.RequireAssertion(
                ctx =>
                    ctx.User.HasClaim(c => c.Type == ClaimTypes.DateOfBirth)
                    && DateTime.ParseExact(
                        "yyyyMMdd",
                        ctx.User.Claims.First(c => c.Type == ClaimTypes.DateOfBirth).Value,
                        System.Globalization.CultureInfo.InvariantCulture
                    ) >= DateTime.Now.AddYears(-18)
            )
    );
});

/*builder.Services.Configure<ApiBehaviorOptions>(
    options => options.SuppressModelStateInvalidFilter = true
);*/

/*builder.Services.AddResponseCaching(options =>
{
    options.MaximumBodySize = 32 * 1024 * 1024;
    options.SizeLimit = 50 * 1024 * 1024;
});*/

builder.Services.AddResponseCaching(options =>
{
    options.MaximumBodySize = 128 * 1024 * 1024;
    options.SizeLimit = 200 * 1024 * 1024;
    options.UseCaseSensitivePaths = true;
});

// builder.Services.AddResponseCaching();

builder.Services.AddMemoryCache();

builder.Services.AddDistributedSqlServerCache(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.SchemaName = "dbo";
    // options.TableName = "AppCache";
    options.TableName = "SQLCache";
});

/*builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:Configuration"];
});*/

var app = builder.Build();

/*if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Swagger services and middleware
    app.UseSwaggerUI(); // Swagger services and middleware
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
}*/

/*if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}*/
// For production
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // For production 756
    // HTTP Security Headers
    app.UseHsts();
    app.Use(
        async (context, next) =>
        {
            context.Response.Headers.Add("X-Frame-Options", "sameorigin");
            context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
            context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Add(
                "Content-Security-Policy",
                "default-src 'self'; script-src 'self' 'nonce-23a98b38c'"
            );
            context.Response.Headers.Add("Referrer-Policy", "strict-origin");
            await next();
        }
    );
}

if (app.Configuration.GetValue<bool>("UseSwagger"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Configuration.GetValue<bool>("UseDeveloperExceptionPage"))
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");

    /*    app.UseExceptionHandler(action =>
        {
            action.Run(async context =>
            {
                var exceptionHandler = context.Features.Get<IExceptionHandlerPathFeature>();
                var details = new ProblemDetails();
                details.Detail = exceptionHandler?.Error.Message;
                details.Extensions["traceId"] =
                    System.Diagnostics.Activity.Current?.Id ?? context.TraceIdentifier;
                details.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
                details.Status = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(details));
            });
        });*/
}

app.UseHttpsRedirection();

app.UseCors();

app.UseResponseCaching();

// app.UseCors("AnyOrigin");

// JWT Auth
app.UseAuthentication();

app.UseAuthorization();

// GraphQL
// app.MapGraphQL();
app.MapGraphQL("/graphql");

// gRPC
app.MapGrpcService<GrpcService>();

/*app.Use(
    (context, next) =>
    {
        context.Response.Headers["cache-control"] = "no-cache, no-store";
        return next.Invoke();
    }
);*/
app.Use(
    (context, next) =>
    {
        context.Response.GetTypedHeaders().CacheControl =
            new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
            {
                NoCache = true,
                NoStore = true
            };
        return next.Invoke();
    }
);

/*app.MapGet("/HelloWorld", () => "Hello, World !");*/
/*app.MapGet(
    "/BoardGamesMinimal",
    () =>
        new[]
        {
            new BoardGame()
            {
                Id = 1,
                Name = "Axis & Allies",
                Year = 1981,
                MinPlayers = 2,
                MaxPlayers = 5,
            },
            new BoardGame()
            {
                Id = 2,
                Name = "Citadels",
                Year = 2000,
                MinPlayers = 2,
                MaxPlayers = 8,
            },
            new BoardGame()
            {
                Id = 3,
                Name = "Terraforming Mars",
                Year = 2016,
                MinPlayers = 1,
                MaxPlayers = 5,
            }
        }
);*/

/*app.MapGet("/error", () => Results.Problem()).RequireCors("AnyOrigin");

app.MapGet(
        "/error/test",
        () =>
        {
            throw new Exception("test");
        }
    )
    .RequireCors("AnyOrigin");*/

/*app.MapGet(
    "/error",
    [EnableCors("AnyOrigin")]
    [ResponseCache(NoStore = true)]
    () => Results.Problem()
);*/

/*app.MapGet(
    "/error",
    [EnableCors("AnyOrigin")]
    [ResponseCache(NoStore = true)]
    (HttpContext context) =>
    {
        var exceptionHandler = context.Features.Get<IExceptionHandlerPathFeature>();

        var details = new ProblemDetails();
        details.Detail = exceptionHandler?.Error.Message;
        details.Extensions["traceId"] =
            System.Diagnostics.Activity.Current?.Id ?? context.TraceIdentifier;
        details.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
        details.Status = StatusCodes.Status500InternalServerError;
        return Results.Problem(details);
    }
);*/

/*app.MapGet(
    "/error/test",
    [EnableCors("AnyOrigin")]
    [ResponseCache(NoStore = true)]
    () =>
    {
        throw new Exception("test");
    }
);*/

app.MapGet(
    "/error",
    [EnableCors("AnyOrigin")]
    [ResponseCache(NoStore = true)]
    // (HttpContext context, ILogger logger) =>
    (HttpContext context) =>
    {
        var exceptionHandler = context.Features.Get<IExceptionHandlerPathFeature>();

        var details = new ProblemDetails();
        details.Detail = exceptionHandler?.Error.Message;
        details.Extensions["traceId"] =
            System.Diagnostics.Activity.Current?.Id ?? context.TraceIdentifier;

        if (exceptionHandler?.Error is NotImplementedException)
        {
            details.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.2";
            details.Status = StatusCodes.Status501NotImplemented;
        }
        else if (exceptionHandler?.Error is TimeoutException)
        {
            details.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.5";
            details.Status = StatusCodes.Status504GatewayTimeout;
        }
        else
        {
            details.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
            details.Status = StatusCodes.Status500InternalServerError;
        }

        /*        app.Logger.LogError(
                    CustomLogEvents.Error_Get,
                    exceptionHandler?.Error,
                    "An unhandled exception occurred."
                );*/

        app.Logger.LogError(
            CustomLogEvents.Error_Get,
            exceptionHandler?.Error,
            "An unhandled exception occurred: " + "{errorMessage}.",
            exceptionHandler?.Error.Message
        );

        return Results.Problem(details);
    }
);

app.MapGet(
    "/error/test",
    [EnableCors("AnyOrigin")]
    [ResponseCache(NoStore = true)]
    () =>
    {
        throw new Exception("test");
    }
);

app.MapGet(
    "/error/test/501",
    [EnableCors("AnyOrigin")]
    [ResponseCache(NoStore = true)]
    () =>
    {
        throw new NotImplementedException("test 501");
    }
);

app.MapGet(
    "/error/test/504",
    [EnableCors("AnyOrigin")]
    [ResponseCache(NoStore = true)]
    () =>
    {
        throw new TimeoutException("test 504");
    }
);

app.MapGet(
    "/cod/test",
    [EnableCors("AnyOrigin")]
    [ResponseCache(NoStore = true)]
    () =>
        Results.Text(
            "<script>"
                + "window.alert('Your client supports JavaScript!"
                + "\\r\\n\\r\\n"
                + $"Server time (UTC): {DateTime.UtcNow.ToString("o")}"
                + "\\r\\n"
                + "Client time (UTC): ' + new Date().toISOString());"
                + "</script>"
                + "<noscript>Your client does not support JavaScript</noscript>",
            "text/html"
        )
);

app.MapGet(
    "/cache/test/1",
    [EnableCors("AnyOrigin")]
    (HttpContext context) =>
    {
        context.Response.Headers["cache-control"] = "no-cache, no-store";
        return Results.Ok();
    }
);

app.MapGet(
    "/cache/test/2",
    [EnableCors("AnyOrigin")]
    (HttpContext context) =>
    {
        return Results.Ok();
    }
);

app.MapGet(
    "/auth/test/1",
    [Authorize]
    [EnableCors("AnyOrigin")]
    [SwaggerOperation(
        Tags = new[] { "Auth" },
        Summary = "Auth test #1 (authenticated users).",
        Description = "Returns 200 - OK if called by "
            + "an authenticated user regardless of its role(s)."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Authorized")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Not authorized")]
    [ResponseCache(NoStore = true)]
    () =>
    {
        return Results.Ok("You are authorized!");
    }
);

app.MapGet(
    "/auth/test/2",
    [Authorize(Roles = RoleNames.Moderator)]
    [EnableCors("AnyOrigin")]
    [SwaggerOperation(
        Tags = new[] { "Auth" },
        Summary = "Auth test #2 (Moderator role).",
        Description = "Returns 200 - OK status code if called by "
            + "an authenticated user assigned to the Moderator role."
    )]
    [ResponseCache(NoStore = true)]
    () =>
    {
        return Results.Ok("You are authorized!");
    }
);

app.MapGet(
    "/auth/test/3",
    [Authorize(Roles = RoleNames.Administrator)]
    [EnableCors("AnyOrigin")]
    [SwaggerOperation(
        Tags = new[] { "Auth" },
        Summary = "Auth test #3 (Administrator role).",
        Description = "Returns 200 - OK if called by "
            + "an authenticated user assigned to the Administrator role."
    )]
    [ResponseCache(NoStore = true)]
    () =>
    {
        return Results.Ok("You are authorized!");
    }
);

app.MapGet(
    "/auth/test/4",
    [Authorize(Roles = RoleNames.SuperAdmin)]
    [EnableCors("AnyOrigin")]
    [ResponseCache(NoStore = true)]
    () =>
    {
        return Results.Ok("You are authorized!");
    }
);

// app.MapControllers();

app.MapControllers().RequireCors("AnyOrigin");

app.Run();
