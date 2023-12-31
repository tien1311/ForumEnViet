﻿namespace ForumNet.Web.Infrastructure.Extensions
{
    using CloudinaryDotNet;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using Data;
    using Data.Models;
    using Services.Categories;
    using Services.Messages;
    using Services.Posts;
    using Services.Providers.Cloudinary;
    using Services.Providers.DateTime;
    using Services.Providers.Email;
    using Services.Reactions;
    using Services.Replies;
    using Services.Reports;
    using Services.Tags;
    using Services.Users;

    public static class ServiceCollectionExtensions
    {
        private const string AntiforgeryHeaderName = "X-CSRF-TOKEN";

        public static IServiceCollection AddDatabase(
            this IServiceCollection services,
            IConfiguration configuration)
            => services
                .AddDbContext<ForumDbContext>(options => options
                    .UseSqlServer(configuration.GetDefaultConnectionString()));

        public static IServiceCollection AddIdentity(this IServiceCollection services)
        {
            services
                .AddDefaultIdentity<ForumUser>(options => options
                    .SetIdentityOptions())
                .AddRoles<ForumRole>()
                .AddEntityFrameworkStores<ForumDbContext>();

            return services;
        }

        public static IServiceCollection ConfigureCookiePolicyOptions(this IServiceCollection services)
            => services
                .Configure<CookiePolicyOptions>(options => options
                    .SetCookiePolicyOptions());

        public static IServiceCollection AddResponseCompressionForHttps(this IServiceCollection services)
            => services
                .AddResponseCompression(options => options
                    .EnableForHttps = true);

        public static IServiceCollection AddAntiforgeryHeader(this IServiceCollection services)
            => services
                .AddAntiforgery(options => options
                    .HeaderName = AntiforgeryHeaderName);

        public static IServiceCollection AddFacebookAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services
                .AddAuthentication()
                .AddFacebook(facebookOptions =>
                {
                    facebookOptions.AppId = configuration["Facebook:AppId"];
                    facebookOptions.AppSecret = configuration["Facebook:AppSecret"];
                });

            return services;
        }

        public static IServiceCollection AddGoogleAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services
                .AddAuthentication()
                .AddGoogle(googleOptions =>
                {
                    googleOptions.ClientId = configuration["Google:ClientId"];
                    googleOptions.ClientSecret = configuration["Google:ClientSecret"];
                });

            return services;
        }

        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration)
            => services
                .AddSingleton(configuration)
                .AddSingleton(CloudinaryConfiguration(configuration))
                .AddTransient<ICategoriesService, CategoriesService>()
                .AddTransient<ICloudinaryService, CloudinaryService>()
                .AddTransient<IDateTimeProvider, DateTimeProvider>()
                .AddTransient<IMessagesService, MessagesService>()
                .AddTransient<IPostReactionsService, PostReactionsService>()
                .AddTransient<IPostReportsService, PostReportsService>()
                .AddTransient<IPostsService, PostsService>()
                .AddTransient<IRepliesService, RepliesService>()
                .AddTransient<IReplyReactionsService, ReplyReactionsService>()
                .AddTransient<IReplyReportsService, ReplyReportsService>()
                .AddTransient<ITagsService, TagsService>()
                .AddTransient<IUsersService, UsersService>()
                .AddTransient<IEmailSender>(serviceProvider =>
                    new SendGridEmailSender(configuration["SendGrid:ApiKey"]));

        public static IServiceCollection AddControllersWithAutoAntiforgeryTokenAttribute(this IServiceCollection services)
        {
            services
                .AddControllersWithViews(options => options
                    .Filters
                    .Add<AutoValidateAntiforgeryTokenAttribute>());

            return services;
        }

        private static Cloudinary CloudinaryConfiguration(IConfiguration configuration)
        {
            var cloudinaryCredentials = new Account(
                configuration["Cloudinary:CloudName"],
                configuration["Cloudinary:ApiKey"],
                configuration["Cloudinary:ApiSecret"]);

            return new Cloudinary(cloudinaryCredentials);
        }
    }
}
