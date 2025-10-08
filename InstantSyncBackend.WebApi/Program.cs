using InstantSyncBackend.Persistence.ServiceCollections;
using InstantSyncBackend.WebApi.Configurations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfiguration();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();


app.UseSwaggerConfiguration();

app.UseHttpsRedirection();
app.UseCors("DefaultPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
