﻿using AdminApplication.Data;
using AdminApplication.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace AngularMVCApplication
{
	public class Startup
	{
		private const string SecretKey = "iNivDmHLpUA223sqsfhqGbMRdRj1PVkH"; // todo: get this from somewhere secure
		private readonly SymmetricSecurityKey _signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey));

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddDbContext<AdminAppDbContext>(opts =>
				opts.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
				b => b.MigrationsAssembly("AdminApplication.Data")));

			services.AddIdentity<ApplicationUser, IdentityRole>()
				.AddEntityFrameworkStores<AdminAppDbContext>();

			services.AddMvc();

			var jwtSettingsSection = Configuration.GetSection("JwtIssuerOptions");
			services.Configure<JwtIssuerOptions>(jwtSettingsSection);
			var jwtSettings = jwtSettingsSection.Get<JwtIssuerOptions>();

			var tokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidIssuer = jwtSettings.Issuer,

				ValidateAudience = true,
				ValidAudience = jwtSettings.Audience,

				ValidateIssuerSigningKey = true,
				IssuerSigningKey = _signingKey,

				RequireExpirationTime = false,
				ValidateLifetime = true,
				ClockSkew = TimeSpan.Zero
			};

			services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

			}).AddJwtBearer(configureOptions =>
			{
				configureOptions.ClaimsIssuer = jwtSettings.Issuer;
				configureOptions.TokenValidationParameters = tokenValidationParameters;
				configureOptions.SaveToken = true;
			});

			services.AddSingleton<IJwtTokenFactory, JwtTokenFactory>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			// Maps to default route named 'default' and the following template: '{controller=Home}/{action=Index}/{id?}'.
			app.UseMvcWithDefaultRoute();
			app.UseMvc(routes =>
			{
				//routes.MapRoute(
				//	name: "default",
				//	template: "{controller=Home}/{action=Index}/{id?}");

				routes.MapSpaFallbackRoute(
					name: "spa-fallback",
					defaults: new { controller = "Home", action = "Index" });
			})
			.UseDefaultFiles()
			.UseStaticFiles();
		}
	}
}
