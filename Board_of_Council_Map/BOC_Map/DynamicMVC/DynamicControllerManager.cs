using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DynamicMVC.Business.Attributes;
using DynamicMVC.Business.Models;
using DynamicMVC.Data;
using DynamicMVC.UI.Extensions;
using DynamicMVC.UI.ViewModels;

namespace DynamicMVC.UI
{
    public class DynamicControllerManager
    {
        public DynamicControllerManager(HttpRequestBase request, ControllerManager controllerManager, IDynamicRepository dynamicRepository, UrlHelper urlHelper)
        {
            RequestManager = new RequestManager(request);
            DynamicEntitySearcher = new DynamicEntitySearcher(RequestManager);
            RequestManager.QueryStringDictionary = RequestManager.QueryStringDictionary.RouteValueDictionaryTypeCorrection(DynamicEntitySearcher.DynamicEntityMetadata);
            ControllerManager = controllerManager;
            DynamicRepository = dynamicRepository;
            UrlManager = new UrlManager(urlHelper);
            ReturnUrlCalculator = new ReturnUrlCalculator(UrlManager);
        }

        private RequestManager RequestManager { get; set; }
        private DynamicEntitySearcher DynamicEntitySearcher { get; set; }
        private ControllerManager ControllerManager { get; set; }
        private IDynamicRepository DynamicRepository { get; set; }
        private ReturnUrlCalculator ReturnUrlCalculator { get; set; }
        private UrlManager UrlManager { get; set; }

        public ActionResult Index(string defaultOrderBy)
        {
            //Add paging attributes if not added
            if (!RequestManager.QueryStringDictionary.ContainsKey("OrderBy") || !RequestManager.QueryStringDictionary.ContainsKey("Page") ||
                !RequestManager.QueryStringDictionary.ContainsKey("PageSize"))
            {
                if (defaultOrderBy == null)
                    defaultOrderBy = DynamicEntitySearcher.DynamicEntityMetadata.KeyName() + " Desc";
                RequestManager.QueryStringDictionary["OrderBy"] = defaultOrderBy;
                RequestManager.QueryStringDictionary["Page"] = 1;
                RequestManager.QueryStringDictionary["PageSize"] = 10;
                return ControllerManager.RedirectToAction("Index", DynamicEntitySearcher.DynamicEntityMetadata.EntityMetadata.TypeName, RequestManager.QueryStringDictionary.ToRouteValues());
            }

            var viewModel = new DynamicIndexViewModel(DynamicEntitySearcher.DynamicEntityMetadata, RequestManager.QueryStringDictionary);
            return ControllerManager.View("DynamicIndex", viewModel);
        }

        public virtual ActionResult _Index(Func<IQueryable, IQueryable> filter)
        {
            var page = int.Parse(RequestManager.QueryStringDictionary["Page"].ToString());
            var pageSize = int.Parse(RequestManager.QueryStringDictionary["PageSize"].ToString());
            var orderBy = RequestManager.QueryStringDictionary["OrderBy"].ToString();

            var viewModel = new DynamicIndexViewModel(DynamicEntitySearcher.DynamicEntityMetadata, RequestManager.QueryStringDictionary);

            var entityType = DynamicEntitySearcher.DynamicEntityMetadata.EntityMetadata.EntityType;
            var filters = viewModel.GetDynamicFilters().Select(x => (Func<IQueryable, IQueryable>)x.Filter).ToList();
            if (filter != null)
                filters.Add(filter);
            var models = DynamicRepository.GetItems(entityType, filters, page, pageSize, orderBy, DynamicEntitySearcher.DynamicEntityMetadata.GetListIncludes().ToArray());

            viewModel = new DynamicIndexViewModel(DynamicEntitySearcher.DynamicEntityMetadata, RequestManager.QueryStringDictionary, models);
            viewModel.RecordCount = DynamicRepository.GetRecordCount(DynamicEntitySearcher.DynamicEntityMetadata.EntityMetadata.EntityType, filters);

            var routeValueDictionary = viewModel.RouteValueDictionary.Clone();
            routeValueDictionary.Remove("ReturnUrl");
            viewModel.RouteValueDictionary["ReturnUrl"] = ReturnUrlCalculator.GetReturnUrl("Index", viewModel.DynamicEntityMetadata.EntityMetadata.TypeName, routeValueDictionary);

            return ControllerManager.PartialView("_DynamicIndex", viewModel);
        }

        public virtual ActionResult IndexSearch(FormCollection formCollection)
        {
            var routeValueDictionary = new RouteValueDictionary();
            foreach (var key in formCollection.AllKeys)
            {
                if (key == "__RequestVerificationToken") continue;
                routeValueDictionary[key] = formCollection[key];
            }
            return ControllerManager.RedirectToAction("Index", DynamicEntitySearcher.DynamicEntityMetadata.EntityMetadata.TypeName, routeValueDictionary);
        }

        public virtual ActionResult Create(string returnUrl)
        {
            dynamic createModel = DynamicEntitySearcher.DynamicEntityMetadata.EntityMetadata.CreateNewObject();
            ControllerManager.TryUpdateModel(createModel);
            ControllerManager.ModelState().Clear();
            var viewModel = new DynamicCreateViewModel(DynamicEntitySearcher.DynamicEntityMetadata, createModel, RequestManager.QueryStringDictionary, returnUrl);
            return ControllerManager.View("DynamicCreate", viewModel);
        }

        public virtual ActionResult Create(FormCollection formCollection, string returnUrl)
        {
            dynamic createModel = DynamicEntitySearcher.DynamicEntityMetadata.EntityMetadata.CreateNewObject();
            var viewModel = new DynamicCreateViewModel(DynamicEntitySearcher.DynamicEntityMetadata, createModel, RequestManager.QueryStringDictionary, returnUrl);

            if (ControllerManager.TryUpdateModel(viewModel.Item, "Item"))
            {
                DynamicEntitySearcher.DynamicEntityMetadata.LoadCreateIncludes(createModel, DynamicRepository, DynamicEntitySearcher.DynamicEntityMetadata.GetInstanceIncludes().ToArray());
                DynamicEntitySearcher.DynamicEntityMetadata.InvokeMethods<DynamicMethodPreSaveCreateAttribute>(createModel);
                DynamicRepository.CreateItem(DynamicEntitySearcher.DynamicEntityMetadata.EntityMetadata.EntityType, viewModel.Item);
                if (!string.IsNullOrWhiteSpace(returnUrl))
                {
                    if (returnUrl.Contains("ScopeIdentity"))
                    {
                        var keyValue = DynamicEntitySearcher.DynamicEntityMetadata.KeyValue(createModel);
                        returnUrl = returnUrl.Replace("ScopeIdentity", keyValue.ToString());
                    }
                    return ControllerManager.Redirect(returnUrl);
                }
                return ControllerManager.RedirectToAction("Index", DynamicEntitySearcher.DynamicEntityMetadata.EntityMetadata.TypeName, new { typeName = DynamicEntitySearcher.DynamicEntityMetadata.EntityMetadata.TypeName });
            }
            return ControllerManager.View("DynamicCreate", viewModel);
        }

        public virtual ActionResult Delete(dynamic id, string returnUrl)
        {
            id = DynamicEntitySearcher.DynamicEntityMetadata.ParseKeyType(id);
            var model = DynamicRepository.GetItem(DynamicEntitySearcher.DynamicEntityMetadata.EntityMetadata.EntityType, DynamicEntitySearcher.DynamicEntityMetadata.KeyName(), id);
            var viewModel = new DynamicDeleteViewModel(DynamicEntitySearcher.DynamicEntityMetadata, model, returnUrl);
            return ControllerManager.View("DynamicDelete", viewModel);
        }

        public virtual ActionResult Delete(dynamic id, FormCollection formCollection, string returnUrl)
        {
            id = DynamicEntitySearcher.DynamicEntityMetadata.ParseKeyType(id);
            DynamicRepository.DeleteItem(DynamicEntitySearcher.DynamicEntityMetadata.EntityMetadata.EntityType, DynamicEntitySearcher.DynamicEntityMetadata.KeyName(), id);
            if (!string.IsNullOrWhiteSpace(returnUrl))
                return ControllerManager.Redirect(returnUrl);
            return ControllerManager.RedirectToAction("Index", DynamicEntitySearcher.DynamicEntityMetadata.EntityMetadata.TypeName, new { typeName = DynamicEntitySearcher.DynamicEntityMetadata.EntityMetadata.TypeName });
        }

        public virtual ActionResult Details(dynamic id)
        {
            id = DynamicEntitySearcher.DynamicEntityMetadata.ParseKeyType(id);
            var model = DynamicRepository.GetItem(DynamicEntitySearcher.DynamicEntityMetadata.EntityMetadata.EntityType, DynamicEntitySearcher.DynamicEntityMetadata.KeyName(), id, DynamicEntitySearcher.DynamicEntityMetadata.GetInstanceIncludes().ToArray());
            var viewModel = new DynamicDetailsViewModel(DynamicEntitySearcher.DynamicEntityMetadata, model, RequestManager.QueryStringDictionary);
            return ControllerManager.View("DynamicDetails", viewModel);
        }

        public virtual ActionResult Edit(dynamic id, string returnUrl)
        {
            id = DynamicEntitySearcher.DynamicEntityMetadata.ParseKeyType(id);
            var model = DynamicRepository.GetItem(DynamicEntitySearcher.DynamicEntityMetadata.EntityMetadata.EntityType, DynamicEntitySearcher.DynamicEntityMetadata.KeyName(), id, DynamicEntitySearcher.DynamicEntityMetadata.GetInstanceIncludes().ToArray());
            var viewModel = new DynamicEditViewModel(DynamicEntitySearcher.DynamicEntityMetadata, model, RequestManager.QueryStringDictionary, returnUrl);
            return ControllerManager.View("DynamicEdit", viewModel);
        }
        
        public virtual ActionResult Edit(dynamic id, FormCollection formCollection, string returnUrl)
        {
            id = DynamicEntitySearcher.DynamicEntityMetadata.ParseKeyType(id);
            var model = DynamicRepository.GetItem(DynamicEntitySearcher.DynamicEntityMetadata.EntityMetadata.EntityType, DynamicEntitySearcher.DynamicEntityMetadata.KeyName(), id, DynamicEntitySearcher.DynamicEntityMetadata.GetInstanceIncludes().ToArray());
            var viewModel = new DynamicEditViewModel(DynamicEntitySearcher.DynamicEntityMetadata, model, RequestManager.QueryStringDictionary, returnUrl);
            if (ControllerManager.TryUpdateModel(viewModel.Item, "Item"))
            {
                DynamicEntitySearcher.DynamicEntityMetadata.InvokeMethods<DynamicMethodPreSaveEditAttribute>(model);
                DynamicRepository.SaveChanges();
                if (!string.IsNullOrWhiteSpace(returnUrl))
                    return ControllerManager.Redirect(returnUrl);
                return ControllerManager.RedirectToAction("Index", DynamicEntitySearcher.DynamicEntityMetadata.EntityMetadata.TypeName, new { typeName = DynamicEntitySearcher.DynamicEntityMetadata.EntityMetadata.TypeName });
            }
            return ControllerManager.View("DynamicEdit", viewModel);
        }
        
        public virtual ActionResult AutoComplete(string searchString)
        {
            return AutoCompleteCustom(DynamicEntitySearcher.DynamicEntityMetadata.KeyName(), DynamicEntitySearcher.DynamicEntityMetadata.GetDefaultPropertyName(), searchString);
        }
        
        public virtual ActionResult AutoCompleteCustom(string valueMember, string displayMember, string searchString)
        {
            IEnumerable items = DynamicRepository.GetAutoCompleteItems(DynamicEntitySearcher.DynamicEntityMetadata.EntityMetadata.EntityType, valueMember, displayMember, searchString, 10);
            return ControllerManager.Json(items);
        }
    }
}
