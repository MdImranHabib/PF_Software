using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using CustomJsonResponse;
using DLL;
using DLL.DataPrepare;
using DLL.Repository;
using DLL.ViewModel;
using PFMVC.common;
using Telerik.Web.Mvc;

namespace PFMVC.Controllers
{
    public class CountryController : Controller
    {
        int PageID = 2;

        private UnitOfWork unitOfWork = new UnitOfWork();
        DP_Country dp_Country = new DP_Country();
        [Authorize]
        public ActionResult Index()
        {
            ViewBag.PageName = "Country Information";
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 0);
            if (b)
            {
                return View();
            }
            else
            {
                ViewBag.PageName = "Settings";
                return View("Unauthorized");
            }
        }



        [GridAction]
        public ActionResult _SelectCountries()
        {
            return View(new GridModel(GetCountries()));
        }


        private IEnumerable<VM_Country> GetCountries()
        {
            var result = unitOfWork.CountryRepository.Get().OrderBy(c => c.CountryName).ToList();
            List<VM_Country> countryList = new List<VM_Country>();
            VM_Country r;
            foreach (var item in result)
            {
                r = new VM_Country();
                r.CountryID = item.CountryID;
                r.Country = item.CountryName;
                countryList.Add(r);
            }
            return countryList;
        }

        public ActionResult CountryForm(string id = null)
        {
            VM_Country v;

            if (string.IsNullOrEmpty(id))
            {
                v = new VM_Country();
                return PartialView("_CountryForm", v);
            }
            else
            {
                LU_tbl_Country t = unitOfWork.CountryRepository.GetByID(id);
                v = dp_Country.vm_Country(t);
                return PartialView("_CountryForm", v);
            }
        }

        [HttpPost]
        public ActionResult CountryForm(VM_Country v)
        {
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            bool IsAllowedToEdit = PagePermission.HasPermission(User.Identity.Name, PageID, 1);
            if (!IsAllowedToEdit) return Json(new { Success = false, ErrorMessage = "You are not allowed to edit information! contact system admin!" }, JsonRequestBehavior.AllowGet);

            if (ModelState.IsValid)
            {
                string _message = "";
                bool b = true;
                if (!string.IsNullOrEmpty(v.CountryID))
                {
                    b = unitOfWork.CountryRepository.IsExist(filter: c => c.CountryName == v.Country && v.CountryID != v.CountryID);
                }
                else
                {
                    b = unitOfWork.CountryRepository.IsExist(filter: c => c.CountryName == v.Country);
                }
                if (!b)
                {
                    LU_tbl_Country s;
                    if (string.IsNullOrEmpty(v.CountryID))
                    {
                        s = dp_Country.tbl_Country(v);
                        s.CountryID = GetMaxID();
                        s.EditDate = System.DateTime.Now;
                        s.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                        unitOfWork.CountryRepository.Insert(s);
                        _message = "New country inserted!";
                    }
                    else
                    {
                        s = dp_Country.tbl_Country(v);
                        s.EditDate = System.DateTime.Now;
                        s.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                        unitOfWork.CountryRepository.Update(s);
                        _message = "Country information updated";
                    }
                    try
                    {
                        unitOfWork.Save();
                        return Json(new { Success = true, Message = _message }, JsonRequestBehavior.AllowGet);

                    }
                    catch (DataException x)
                    {
                        return Json(new { Success = false, ErrorMessage = x.Message + " \n=> " + x.InnerException }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "This Country Name Already Exist In Database!");
                }
            }
            var errorList = (from item in ModelState.Values
                             from error in item.Errors
                             select error.ErrorMessage).ToList();

            return Json(JsonResponseFactory.ErrorResponse(errorList), JsonRequestBehavior.DenyGet);
        }



        public string GetMaxID()
        {
            var query = "SELECT isnull(MAX(convert(int, CountryID)),0) FROM [PFTM].[dbo].[LU_tbl_Country]";
            var data = unitOfWork.CountryRepository.GetRowCount(query);
            string s = (Convert.ToInt16(data) + 1) + "";
            return s.Trim().PadLeft(3, '0');
        }


        public ActionResult DeletePossible(string id)
        {
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 2);
            if (!b)
            {
                return Json(new { Success = false, ErrorMessage = "You are not authorized to delete information!" }, JsonRequestBehavior.AllowGet);
            }
            bool a = true;// unitOfWork.SupplierRepository.IsExist(filter: c => c.CountryID == id);
            if (!a)
            {
                return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Success = false, ErrorMessage = "This information is associated with other information AND cannot be deleted!" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteConfirm(string id)
        {

            unitOfWork.CountryRepository.Delete(id);
            try
            {
                unitOfWork.Save();
                return Json(new { Success = true, Message = "Successfully Deleted!" }, JsonRequestBehavior.DenyGet);
            }
            catch (Exception x)
            {
                return Json(new { Success = false, ErrorMessage = "Problem While deleting country., \nDetails:" + x.Message }, JsonRequestBehavior.DenyGet);
            }
        }

    }
}
