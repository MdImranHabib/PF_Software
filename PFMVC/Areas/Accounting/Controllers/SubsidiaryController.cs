using CustomJsonResponse;
using DLL;
using DLL.Repository;
using DLL.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PFMVC.Areas.Accounting.Controllers
{
    public class SubsidiaryController : Controller
    {
        private PFTMEntities context;
        UnitOfWork unitOfWork = new UnitOfWork();
        //
        // GET: /Accounting/Subsidiary/

        public ActionResult Index()
        {
            //Added By Shohid Date:Oct-18-2016
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            return View();
        }

        public ActionResult SubsidiaryIndex()
        {
            //Added By Shohid Date:Oct-9-2016
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            return View();
        }


        public ActionResult SubsidiaryForm(int SubsidiaryID = 0)
        {
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            if (SubsidiaryID == 0)
            {

                return PartialView("SubsidiaryForm");
            }
            else if (SubsidiaryID > 0)
            {

                var v = unitOfWork.ACC_Subsidiary.Get().Where(x => x.Subsidiary_Id == SubsidiaryID).FirstOrDefault();
                if (v != null)
                {
                    VM_acc_Subsidiary a = new VM_acc_Subsidiary();
                    a.Subsidiary_Id = v.Subsidiary_Id;
                    a.Subsidiary_Name = v.Subsidiary_Name;


                    return PartialView("SubsidiaryForm", a);
                }
                else
                {
                    return Json(new { Success = false, ErrorMessage = "Data not found! Unexpected error" }, JsonRequestBehavior.DenyGet);
                }
            }
            else
            {
                return Json(new { Success = false, ErrorMessage = "No Subsidiary record selected..." }, JsonRequestBehavior.DenyGet);
            }
        }

        [HttpPost]
        public ActionResult SubsidiaryForm(VM_acc_Subsidiary v)
        {


            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            if (OCode == 0)
            {
                return Json(new { Success = false, ErrorMessage = "To create a Subsidiary you must be under a compnay!" }, JsonRequestBehavior.DenyGet);
            }
            bool isSubsidiaryExist = unitOfWork.ACC_Subsidiary.IsExist(w => w.Subsidiary_Name == v.Subsidiary_Name);
            if (!isSubsidiaryExist)
            {

                if (v.Subsidiary_Id == 0)
                {

                    acc_Subsidiary acc_sub = new acc_Subsidiary();
                    acc_sub.Subsidiary_Name = v.Subsidiary_Name;
                    acc_sub.EditDate = DateTime.Now;
                    acc_sub.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                    acc_sub.OCode = OCode.ToString();

                    unitOfWork.ACC_Subsidiary.Insert(acc_sub);
                }
                else if (v.Subsidiary_Id > 0)
                {
                    acc_Subsidiary acc_sub = unitOfWork.ACC_Subsidiary.Get().Where(x => x.Subsidiary_Id == v.Subsidiary_Id).FirstOrDefault();
                    if (acc_sub != null)
                    {

                        acc_sub.Subsidiary_Name = v.Subsidiary_Name;
                        acc_sub.Subsidiary_Id = v.Subsidiary_Id;
                        acc_sub.OCode = OCode.ToString();
                        acc_sub.EditDate = DateTime.Now;
                        acc_sub.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                        unitOfWork.ACC_Subsidiary.Update(acc_sub);
                    }
                    else
                    {
                        return Json(new { Success = false, ErrorMessage = "Null object found!" }, JsonRequestBehavior.DenyGet);
                    }
                }
                try
                {
                    unitOfWork.Save();
                    return Json(new { Success = true, Message = "Successfully saved!" }, JsonRequestBehavior.DenyGet);
                }
                catch (Exception x)
                {
                    return Json(new { Success = false, ErrorMessage = "Error while saving. " + x.Message }, JsonRequestBehavior.DenyGet);
                }
            }
            else
            {
                return Json(new { Success = false, ErrorMessage = "Subsidiary name or Code exist. Try another..." }, JsonRequestBehavior.DenyGet);
            }


        }


        public ActionResult GetSubsidiaryJQFile()
        {
            return PartialView("SubsidiaryJQ");
        }

        public ActionResult GetSubsidiaryName(int filter = 0, string name = "")
        {

            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            if (filter > 0)
            {
                ViewBag.LegendName = "Subsidiary List: " + name;
                var v = unitOfWork.ACC_Subsidiary.Get().Where(w => w.Subsidiary_Id == filter && (w.OCode == null || w.OCode == OCode.ToString())).ToList();
                ViewBag.Group = v;
                return PartialView("SubsidiaryList");
            }
            else
            {
                ViewBag.LegendName = "Subsidiary: All";
                var v = unitOfWork.ACC_Subsidiary.Get().Where(w => (w.OCode == null || w.OCode == OCode.ToString())).ToList();
                ViewBag.Group = v;
                return PartialView("SubsidiaryList");
            }
        }


    }
}
