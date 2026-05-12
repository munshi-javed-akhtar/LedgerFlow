using System.Text;
using FluentValidation;
using LedgerFlow.Application.DTOs;
using LedgerFlow.Application.Transactions.Commands.Transfer;
using LedgerFlow.Application.Transactions.Validators;
using LedgerFlow.Infrastructure.DependencyInjection;
using LedgerFlow.Persistence.DependencyInjection;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(TransferCommand).Assembly));
builder.Services.AddScoped<IValidator<TransferRequestDto>, TransferRequestValidator>();
builder.Services.AddInfrastructure();
builder.Services.AddPersistence(builder.Configuration);

var jwtKey = builder.Configuration["Jwt:Key"] ?? "this-is-a-dev-key-change-me-in-production";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "LedgerFlow",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "LedgerFlowClients",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/transactions/transfer", async (TransferRequestDto request, IValidator<TransferRequestDto> validator, IMediator mediator, CancellationToken ct) =>
{
    var validation = await validator.ValidateAsync(request, ct);
    if (!validation.IsValid) return Results.ValidationProblem(validation.ToDictionary());

    var result = await mediator.Send(new TransferCommand(request), ct);
    return Results.Ok(result);
}).RequireAuthorization();

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Utc = DateTimeOffset.UtcNow }));

app.Run();
