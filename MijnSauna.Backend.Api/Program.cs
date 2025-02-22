﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MijnSauna.Backend.Api.DependencyInjection;
using MijnSauna.Backend.Api.Middleware;
using MijnSauna.Backend.Api.Swagger;
using Prometheus;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.WebHost.UseKestrel();
builder.WebHost.ConfigureKestrel((context, options) =>
{
    var certificateFileName = context.Configuration.GetValue<string>("CERTIFICATE_FILENAME");
    var certificatePassword = context.Configuration.GetValue<string>("CERTIFICATE_PASSWORD");

    options.Listen(IPAddress.Any, 5000);
    //if (string.IsNullOrEmpty(certificateFileName) || string.IsNullOrEmpty(certificatePassword))
    //{
    //    options.Listen(IPAddress.Any, 5000);
    //}
    //else
    //{
    //    options.Listen(IPAddress.Any, 5000,
    //        listenOptions => { listenOptions.UseHttps(certificateFileName, certificatePassword); });
    //}
});

builder.Services.ConfigureApi(c =>
{
    c.ConnectionString = builder.Configuration.GetValue<string>("CONNECTIONSTRING");
    c.ClientId = builder.Configuration.GetValue<string>("CLIENT_ID");
});
builder.Services.AddControllers();
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MijnSauna API", Version = "v1" });
    c.OperationFilter<ClientIdHeaderParameterOperationFilter>();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseMiddleware<AuthenticationMiddleware>();
app.UseAuthorization();
app.UseRouting();
app.UseHttpMetrics();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MijnSauna API v1");
});

app.MapGet($"status", () => Results.Ok());

app.Run();