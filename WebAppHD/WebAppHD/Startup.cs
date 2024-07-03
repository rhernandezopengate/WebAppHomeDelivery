using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebAppHD.Startup))]
namespace WebAppHD
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
