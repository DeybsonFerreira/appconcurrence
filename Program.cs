using Concorrencia.Api.Repositories;
using Concorrencia.Api.Repositories.Estrategias;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<PessoaStore>();
builder.Services.AddSingleton<IEstrategiaCadastro, EstrategiaSemProtecao>();
builder.Services.AddSingleton<IEstrategiaCadastro, EstrategiaSemaphoreSlim>();
builder.Services.AddSingleton<IEstrategiaCadastro, EstrategiaLock>();
builder.Services.AddSingleton<IEstrategiaCadastro, EstrategiaConcurrentDictionary>();
builder.Services.AddSingleton<IEstrategiaCadastro, EstrategiaMemoryCache>();
builder.Services.AddSingleton<PessoaRepository>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
