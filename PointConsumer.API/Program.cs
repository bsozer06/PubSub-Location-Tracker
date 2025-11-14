using PointConsumer.API.Configurations;
using PointConsumer.API.Hubs;
using PointConsumer.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
//builder.Services.AddSingleton<PointConsumer.API.Services.WebSocketManager>();
builder.Services.AddHostedService<PointConsumerService>();
builder.Services.AddHttpClient();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithOrigins("http://localhost:4200"));
    //.SetIsOriginAllowed(_ => true));
});

builder.Services.Configure<RabbitMQOptions>(
    builder.Configuration.GetSection(RabbitMQOptions.RabbitMQ)
);
builder.Services.Configure<ProducerApiOptions>(
    builder.Configuration.GetSection(ProducerApiOptions.ProducerApi)
);

builder.Services.AddHttpClient<IProducerApiClient, ProducerApiClient>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseCors();
app.MapHub<PointHub>("/pointHub");

app.Run();
