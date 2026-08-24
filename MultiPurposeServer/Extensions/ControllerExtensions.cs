namespace MultiPurposeServer.Extensions
{
    public static class ControllerExtensions
    {
        public static IMvcBuilder AddMultiPurposeControllers(this IServiceCollection services)
        {
            return services
                .AddControllers()
                .AddApplicationPart(typeof(Finance.Api.AssemblyReference).Assembly)
                .AddApplicationPart(typeof(Portfolio.Api.AssemblyReference).Assembly)
                .AddControllersAsServices();
        }
    }
}
