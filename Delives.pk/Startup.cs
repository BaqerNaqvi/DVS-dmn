using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Delives.pk.Startup))]
namespace Delives.pk
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
