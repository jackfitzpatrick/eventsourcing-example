using Marten;
using Marten.Events.Daemon.Resiliency;
using System.Text.Json.Serialization;
using UsersApi.Commands;
using UsersApi.Core.Marten;
using UsersApi.Domain;
using UsersApi.Projections;
using Weasel.Core;
using static UsersApi.Services.UserCommandService;
using Marten.Events.Projections;
using Microsoft.AspNetCore.Mvc;
using UsersApi.Core.Http;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;
using static Microsoft.AspNetCore.Http.Results;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMarten(options =>
    {
        var schemaName = Environment.GetEnvironmentVariable("SchemaName") ?? "Users";
        options.Events.DatabaseSchemaName = schemaName;
        options.DatabaseSchemaName = schemaName;
        options.Connection(builder.Configuration.GetConnectionString("Users") ?? throw new InvalidOperationException());
        options.UseDefaultSerialization(EnumStorage.AsString, nonPublicMembersStorage: NonPublicMembersStorage.All);

        options.Projections.Add<UserDetailsProjection>(ProjectionLifecycle.Inline);
    })
    .UseLightweightSessions()
    .AddAsyncDaemon(DaemonMode.Solo);

builder.Services.Configure<JsonOptions>(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("api/users/register", async (IDocumentSession documentSession, RegisterUserRequest body, CancellationToken ct) =>
{
    var (id, name, email) = body;
    await documentSession.Add<User>(id, Handle(new RegisterUser(id, name, email)), ct);

    return Created($"api/users/{id}", id);
}
).WithTags("User");

app.MapGet("api/users/{userId:guid}",
    (HttpContext context, IQuerySession querySession, Guid userId) =>
        querySession.Json.FindById<UserDetails>(userId)
).WithTags("User");;

app.MapPost("api/users/change-name", async (
    IDocumentSession documentSession,
    ChangeUserNameRequest body,
    [FromHeader(Name = "If-Match")] string eTag,
    CancellationToken ct
) =>
{
    await documentSession.GetAndUpdate<User>(
        body.Id, ETagExtensions.ToExpectedVersion(eTag),
        (User current) => Handle(current, new ChangeUserName(body.Id, body.NewName)),
        ct
        );
}
).WithTags("User");;

app.MapPost("api/users/update-address", async (
    IDocumentSession documentSession,
    UpdateAddressRequest body,
    [FromHeader(Name = "If-Match")] string eTag,
    CancellationToken ct
) =>
{
    await documentSession.GetAndUpdate<User>(
        body.Id, ETagExtensions.ToExpectedVersion(eTag),
        (User current) => Handle(current, new UpdateAddress(body.Id, body.Address)),
        ct
        );
}
).WithTags("User");;

app.MapPost("api/users/rebuild-projections", async (IDocumentStore documentStore, CancellationToken ct) => {
    using var daemon = await documentStore.BuildProjectionDaemonAsync();

    await daemon.RebuildProjection<UserDetailsProjection>(new TimeSpan(0, 5, 0), ct);

    return "Rebuilt projections";
}).WithTags("User");;

app.Run();

public record RegisterUserRequest(Guid Id, string Name, string Email);

public record ChangeUserNameRequest(Guid Id, string NewName);

public record UpdateAddressRequest(Guid Id, string Address);
