using Ambev.DeveloperEvaluation.Application;
using Ambev.DeveloperEvaluation.WebApi;
using AutoMapper;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class AutoMapperConfigurationTests
{
    [Fact(DisplayName = "All AutoMapper profiles should have valid configuration")]
    public void AllProfiles_ShouldBeValid()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(ApplicationLayer).Assembly);
            cfg.AddMaps(typeof(Program).Assembly);
        });

        configuration.AssertConfigurationIsValid();
    }
}
