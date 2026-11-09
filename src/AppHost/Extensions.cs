internal static class AspireExtensions
{
    public static IResourceBuilder<T> WithAspNetCoreEnvironment<T>(this IResourceBuilder<T> builder) 
        where T : IResourceWithEnvironment
    {
        builder.WithEnvironment(context =>
        {
            var environment = builder.ApplicationBuilder.ExecutionContext.IsPublishMode
                ? "Production"
                : Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            context.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = environment;
        });

        return builder;
    }
}
