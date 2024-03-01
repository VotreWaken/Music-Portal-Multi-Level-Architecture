using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using MusicPortal.DAL.Context;
using MusicPortal.BLL.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using MusicPortal.BLL.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TokenApp;
var builder = WebApplication.CreateBuilder(args);


var configuration = builder.Configuration;


builder.Services.AddControllersWithViews();


//builder.Services.AddDistributedMemoryCache();
//builder.Services.AddSession();


string? connection = configuration.GetConnectionString("DefaultConnection");
// Auth 

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//					.AddJwtBearer(options =>
//					{
//						options.RequireHttpsMetadata = false;
//						options.TokenValidationParameters = new TokenValidationParameters
//						{
//							// ��������, ����� �� �������������� �������� ��� ��������� ������
//							ValidateIssuer = true,
//							// ������, �������������� ��������
//							ValidIssuer = AuthOptions.ISSUER,

//							// ����� �� �������������� ����������� ������
//							ValidateAudience = true,
//							// ��������� ����������� ������
//							ValidAudience = AuthOptions.AUDIENCE,
//							// ����� �� �������������� ����� �������������
//							ValidateLifetime = true,

//							// ��������� ����� ������������
//							IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
//							// ��������� ����� ������������
//							ValidateIssuerSigningKey = true,
//						};
//					});

// ��������� �����������
// builder.Services.AddAuthorization();
// builder.Services.AddHttpContextAccessor();

builder.Services.AddMusicPortalContext(connection);


builder.Services.AddUnitOfWorkService();
builder.Services.AddBLLServices();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10); // ������������ ������ (����-��� ���������� ������)
    options.Cookie.Name = "Session"; // ������ ������ ����� ���� �������������, ������� ����������� � �����.

});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
}

app.UseSession();
app.UseHttpsRedirection();


app.UseStaticFiles();


app.UseRouting();


// app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();

public partial class Program { }