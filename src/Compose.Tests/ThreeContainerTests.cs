using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Squadron
{
    public class SampleAppOptions : GenericContainerOptions
    {
        public override void Configure(ContainerResourceBuilder builder)
        {
            base.Configure(builder);
            builder
                .Name("nginx-sample")
                .InternalPort(80)
                .Image("nginx")
                .AddNetwork("demo-network");
        }
    }


    public class ThreeContainerOptions : ComposeResourceOptions
    {
        public override void Configure(ComposeResourceBuilder builder)
        {
            builder.AddGlobalEnvironmentVariable("FOO", "BAR");
            builder.AddContainer<MongoDefaultOptions>("mongo");
            builder.AddContainer<SampleAppOptions>("api")
                    .AddLink("mongo", new EnvironmentVariableMapping(
                                "Sample:Database:ConnectionString",
                                "#CONNECTIONSTRING#"
                                ));

            builder.AddContainer<SampleAppOptions>("ui")
                    .AddLink("api", new EnvironmentVariableMapping(
                                "Sample:Api:Url", "#HTTPURL#"
                                ));
        }
    }

    public class TwoContainerComposedResource : ComposeResource<ThreeContainerOptions>
    {


    }


    public class ThreeContainerTests : ISquadronResourceFixture<TwoContainerComposedResource>
    {
        private readonly TwoContainerComposedResource _resource;

        public ThreeContainerTests(SquadronResource<TwoContainerComposedResource> resource)
        {
            _resource = resource.Resource;
        }

        [Fact]
        public void ThreeContainer_NoError()
        {
            //Act
            Action action = () =>
            {
                MongoResource mongo = _resource.GetResource<MongoResource>("mongo");
            };

            //Assert
            action.Should().NotThrow();
        }
    }
}
