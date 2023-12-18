using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.OpenApi.Models;
using MyBGList;

var builder = WebApplication.CreateBuilder(args);

/*builder.Services.AddCors(
    options =>
        options.AddDefaultPolicy(cfg =>
        {
            cfg.AllowAnyOrigin();
            cfg.AllowAnyHeader();
            cfg.AllowAnyMethod();
        })
);*/

/*builder.Services.AddCors(options =>
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
});*/

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
    options.AddPolicy(
        name: "AnyOrigin_GetOnly",
        cfg =>
        {
            cfg.AllowAnyOrigin();
            cfg.AllowAnyHeader();
            cfg.WithMethods("GET");
        }
    );
});

builder.Services.AddApiVersioning(options =>
{
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opts => opts.ResolveConflictingActions(apiDesc => apiDesc.First()));
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "MyBGList", Version = "v1.0" });
    options.SwaggerDoc("v2", new OpenApiInfo { Title = "MyBGList", Version = "v2.0" });
    options.SwaggerDoc("v3", new OpenApiInfo { Title = "MyBGList", Version = "v3.0" });
});

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

if (app.Configuration.GetValue<bool>("UseSwagger"))
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint($"/swagger/v1/swagger.json", $"MyBGList v1");
        options.SwaggerEndpoint($"/swagger/v2/swagger.json", $"MyBGList v2");
        options.SwaggerEndpoint($"/swagger/v3/swagger.json", $"MyBGList v3");
    });
}

if (app.Configuration.GetValue<bool>("UseDeveloperExceptionPage"))
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
}

app.UseHttpsRedirection();

app.UseCors();

// app.UseCors("AnyOrigin");

app.UseAuthorization();

app.MapGet(
    "v{version:apiVersion}/HelloWorld",
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [ApiVersion("3.0")]
    [EnableCors("AnyOrigin")]
    [ResponseCache(NoStore = true)]
    () => "Hello, World !"
);

app.MapGet(
    "v{version:apiVersion}/BoardGamesMinimal",
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [ApiVersion("3.0")]
    [EnableCors("AnyOrigin")]
    [ResponseCache(NoStore = true)]
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
);

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

app.MapGet(
    "v{version:apiVersion}/error",
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [EnableCors("AnyOrigin")]
    [ResponseCache(NoStore = true)]
    () => Results.Problem()
);

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
    "v{version:apiVersion}/error/test",
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [EnableCors("AnyOrigin")]
    [ResponseCache(NoStore = true)]
    () =>
    {
        throw new Exception("test");
    }
);

/*app.MapGet(
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
);*/

/*app.MapGet(
    "/v{version:ApiVersion}/cod/test",
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
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
);*/

app.MapGet(
    "v{version:apiVersion}/cod/test",
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [EnableCors("AnyOrigin_GetOnly")]
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

// app.MapControllers();

app.MapControllers().RequireCors("AnyOrigin");

app.Run();
