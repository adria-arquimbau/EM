using EventsManager.Server;

var builder = WebApplication.CreateBuilder(args);

var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services, builder.Configuration); // calling ConfigureServices method

var app = builder.Build();

startup.Configure(app, builder.Environment, app.Services); // calling Configure method