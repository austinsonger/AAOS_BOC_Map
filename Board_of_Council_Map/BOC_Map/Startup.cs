using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BOC_Map.Startup))]
namespace BOC_Map
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
