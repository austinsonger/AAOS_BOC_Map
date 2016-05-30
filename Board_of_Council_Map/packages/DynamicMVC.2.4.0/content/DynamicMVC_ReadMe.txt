**if using the DynamicMVC project template, skip to step 6

**make sure to update DynamicMVC nuget file to the latest version

1.  Add the following to the end of the global file:
var applicationMetadata = new DynamicMVC.Business.Models.ApplicationMetadata(typeof(MvcApplication).Assembly,
                typeof(MvcApplication).Assembly, typeof(MvcApplication).Assembly,
                () => new DynamicMVC.Data.DynamicRepository(new YourDbContext()));
            DynamicMVC.Managers.DynamicMVCManager.ParseApplicationMetadata(applicationMetadata);

            DynamicMVC.Managers.DynamicMVCManager.SetDynamicRoutes(RouteTable.Routes);

2.  Add the following to the _layout file to add dynamic menu items (this should be added just prior to </ul>):
@foreach (var menuItemViewModel in DynamicMVC.Managers.DynamicMVCManager.GetDynamicMenuItems())
                    {
                        <li class="dropdown">
                            <a href="#" class="dropdown-toggle" data-toggle="dropdown">@menuItemViewModel.DisplayName <b class="caret"></b></a>
                            <ul class="dropdown-menu">
                                @foreach (var childMenuViewModel in menuItemViewModel.DynamicMenuItemViewModels)
                                {
                                    <li><a href="@Url.Action("Index", childMenuViewModel.DynamicEntityMetadata.EntityMetadata.TypeName)">@(childMenuViewModel.DisplayName)</a></li>
                                }
                            </ul>
                        </li>
                    }
                    
3.  Modify jquery, jqueryval, and css bundles to look like the following (This can be found in App_Start\BundleConfig):
bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/select2.js",
                        "~/Scripts/DynamicScript.js",
                        "~/Scripts/jquery-ui-1.11.1.js"));
                        
bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                                 "~/Scripts/jquery.validate*",
                                 "~/Scripts/jquery.unobtrusive-ajax.js"));
                                 
bundles.Add(new StyleBundle("~/Content/bundle/css").Include(
                    "~/Content/bootstrap.css",
                    "~/Content/DynamicStyle.css",
                    "~/Content/site.css",
                    "~/Content/Validation.css",
                    "~/Content/css/select2.css",
                    "~/Content/select2custom.css"));

4.  Modfy the _Layout file to reference the new css bundle name as well as the jquery ui bundle.  It is important to change the bundle name because the select2 package adds a folder on the file system that conflicts with the default bundle name.
@Styles.Render("~/Content/bundle/css")
<link rel="stylesheet" type="text/css" href="http://ajax.googleapis.com/ajax/libs/jqueryui/1.11.1/themes/smoothness/jquery-ui.css" />

5.  In _Layout view move the "@Scripts.Render("~/bundles/jquery")" line from the bottom of the page to be directly under "@Scripts.Render("~/bundles/modernizr")".

6.  Decorate each model class you want to be dynamic with DynamicEntityAttribute.

7.  Visit us as DynamicMVC.com and https://dynamicmvc.codeplex.com/.
                        