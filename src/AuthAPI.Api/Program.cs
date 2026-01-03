using AuthAPI.Api;
using AuthAPI.Application;
using AuthAPI.Domain;
using AuthAPI.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddPresentation(builder.Configuration);
    builder.Services.AddApplication();
    builder.Services.AddDomain();
    builder.Services.AddInfrastructure(builder.Configuration);
}

var app = builder.Build();
{
    app.AddPresentation();
    app.AddInfrastructure();
    app.Run();
}
