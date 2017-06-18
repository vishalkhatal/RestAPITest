using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RealEstate.API.Startup))]
namespace RealEstate.API
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
