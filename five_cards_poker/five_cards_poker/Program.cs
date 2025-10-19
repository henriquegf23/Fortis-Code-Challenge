using five_cards_poker.Models;
using five_cards_poker.Repository;
using five_cards_poker.Repository.Interfaces;
using five_cards_poker.Services;
using five_cards_poker.Services.Interfaces;
using Microsoft.OpenApi.Models;
using System.Diagnostics;
using System.Reflection;
using System.Runtime;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Fortis Code Challenge", Version = "v1" });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

});

builder.Services.AddScoped<IDeckRepository, DeckRepository>();


builder.Services.AddScoped<IDeckServices, DeckServices>();
builder.Services.AddScoped<IPokerService, PokerServices>();
builder.Services.AddScoped<IPokerActionsService, PokerActionsServices>();
builder.Services.AddScoped<IMappingDTOServices, MappingDTOServices>();



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

app.Run();
