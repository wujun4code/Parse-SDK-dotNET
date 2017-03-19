using Microsoft.Owin;
using Owin;
using LeanCloud;

[assembly: OwinStartupAttribute(typeof(ASPMVC.Debug.Startup))]
namespace ASPMVC.Debug
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            AVClient.Initialize("uay57kigwe0b6f5n0e1d4z4xhydsml3dor24bzwvzr57wdap", "kfgz7jjfsk55r5a8a3y4ttd3je1ko11bkibcikonk32oozww");
            ConfigureAuth(app);
        }
    }
}
