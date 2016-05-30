using System.Web.Mvc;
using DynamicMVC.Data;
using DynamicMVC.Managers;
using DynamicMVC.UI.Filters;

namespace DynamicMVC.UI.Controllers
{
    [DynamicEditorViewaBagActionFilter]
    public class DynamicController : Controller
    { 
        public DynamicController()
        {
            DynamicRepository = DynamicMVCManager.GetNewDynamicRepository();
        }

        public IDynamicRepository DynamicRepository { get; set; }
        
        protected override void Dispose(bool disposing)
        {
            if (DynamicRepository != null)
                DynamicRepository.Dispose();
            base.Dispose(disposing);
        }

        private DynamicControllerManager _dynamicControllerManager;
        private DynamicControllerManager DynamicControllerManager()
        {
            if (_dynamicControllerManager == null)
            {
                var controllerManager = new ControllerManager(View, RedirectToAction, RedirectToAction, PartialView, () => ModelState, (dynamic) => TryUpdateModel(dynamic), (dynamic, prefix) => TryUpdateModel(dynamic, prefix), Redirect, Json);
                _dynamicControllerManager = new DynamicControllerManager(Request, controllerManager, DynamicRepository, Url);
            }
            return _dynamicControllerManager;
        }
       
        public virtual ActionResult Index(string defaultOrderBy)
        {
            return DynamicControllerManager().Index(defaultOrderBy);
        }

        public virtual ActionResult _Index()
        {
            return DynamicControllerManager()._Index(null);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult IndexSearch(FormCollection formCollection)
        {
            return DynamicControllerManager().IndexSearch(formCollection);
        }

        public virtual ActionResult Create(string returnUrl)
        {
            return DynamicControllerManager().Create(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Create(FormCollection formCollection, string returnUrl)
        {
            return DynamicControllerManager().Create(formCollection, returnUrl);
        }
        public virtual ActionResult Delete(dynamic id, string returnUrl)
        {
            return DynamicControllerManager().Delete(id, returnUrl);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Delete(dynamic id, FormCollection formCollection, string returnUrl)
        {
            return DynamicControllerManager().Delete(id, formCollection, returnUrl);
        }
        
        public virtual ActionResult Details(dynamic id)
        {
            return DynamicControllerManager().Details(id);
        }
        public virtual ActionResult Edit(dynamic id, string returnUrl)
        {
            return DynamicControllerManager().Edit(id, returnUrl);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Edit(dynamic id, FormCollection formCollection, string returnUrl)
        {
            return DynamicControllerManager().Edit(id, formCollection, returnUrl);
        }
        [HttpPost]
        public virtual ActionResult AutoComplete(string searchString)
        {
            return DynamicControllerManager().AutoComplete(searchString);
        }
        [HttpPost]
        public virtual ActionResult AutoCompleteCustom(string valueMember, string displayMember, string searchString)
        {
            return DynamicControllerManager().AutoCompleteCustom(valueMember, displayMember, searchString);
        }
    }
}