using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;

namespace ChatBackend.Composition
{
    internal static partial class ProgramConfiguration
    {
        public static WebApplicationBuilder ConfigureAWS(this WebApplicationBuilder builder)
        {
            builder.Services.AddDefaultAWSOptions(new AWSOptions
            {
                Region = Amazon.RegionEndpoint.USEast1 
            });
            builder.Services.AddAWSService<IAmazonS3>();
            return builder;
        }
    }
}
