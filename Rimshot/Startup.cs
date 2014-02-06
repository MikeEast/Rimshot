using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Rimshot.Startup))]
namespace Rimshot
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
