using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace BOC_Map
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            if (typeof(MvcApplication).Assembly.ManifestModule.Name.ToUpper() == "DYNAMICMVC.DLL")
            {
                throw new Exception("Your UI assembly cannot be named DynamicMVC.  This conflicts with dynamicmvc.dll");
            }
            var applicationMetadata = new DynamicMVC.Business.Models.ApplicationMetadata(typeof(MvcApplication).Assembly,
                typeof(MvcApplication).Assembly, typeof(MvcApplication).Assembly,
                () => new DynamicMVC.Data.DynamicRepository(new BOC_Map.Models.ApplicationDbContext()));
            DynamicMVC.Managers.DynamicMVCManager.ParseApplicationMetadata(applicationMetadata);

            DynamicMVC.Managers.DynamicMVCManager.SetDynamicRoutes(RouteTable.Routes);
        }
    }
}
