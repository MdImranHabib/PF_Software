using System;
using System.Linq;
using System.Web.Mvc;
using DLL;
using DLL.Repository;
using DLL.ViewModel;
using PFMVC.common;

namespace PFMVC.Areas.CompanyInformation.Controllers
{
    public class CompanyController : Controller
    {
        UnitOfWork unitOfWork = new UnitOfWork();

        int PageID = 18;

        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <returns></returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>Mar-6-2016</ModificationDate>
        [Authorize]
        public ActionResult Index()
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 0);
            if (!b)
            {
                ViewBag.PageName = "Employee Setup";
                return View("Unauthorized");
            }
            //End
            return View();
        }


        /// <summary>
        /// Companies the information.
        /// </summary>
        /// <param name="companyID">The company identifier.</param>
        /// <returns></returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>Mar-6-2016</ModificationDate>
        [Authorize]
        public ActionResult CompanyInformation(int companyID = 0)
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            if (companyID == 0)
            {
                ViewBag.Message = "Add new company...";
                return PartialView("_CompanyForm", new VM_CompanyInformation());
            }
            var v = unitOfWork.CompanyInformationRepository.GetByID(companyID);
            VM_CompanyInformation company = new VM_CompanyInformation();
            if (v != null)
            {
                company.CompanyID = v.CompanyID;
                company.CompanyName = v.CompanyName;
                company.CompanyAddress = v.CompanyAddress;
                company.SystemImplementationDate = v.SystemImplementationDate;
                company.AccountingYearBeginningFrom = v.AccountingYearBeginningFrom ?? DateTime.MinValue;
                company.EditDate = v.EditDate;
                company.EditUserName = v.EditUserName;
                company.EditUser = v.EditUser ?? Guid.NewGuid();

                ViewBag.Message = "Company name already set...";

                return PartialView("_CompanyForm", company);
            }
            ViewBag.Message = "Please set company information...";
            return PartialView("_CompanyForm");
        }


        /// <summary>
        /// Companies the information.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns></returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>Mar-6-2016</ModificationDate>
        [HttpPost]
        public ActionResult CompanyInformation(VM_CompanyInformation v)
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 1);
            if (!b)
            {
                return Json(new { Success = false, ErrorMessage = "You donot have permission in this section." }, JsonRequestBehavior.DenyGet);
            }
            //End
            if (string.IsNullOrEmpty(v.CompanyName))
            {
                return Json(new { Success = false, ErrorMessage = "You must give a company name!" }, JsonRequestBehavior.DenyGet);
            }
            bool isCompanyNameExist = unitOfWork.CompanyInformationRepository.IsExist(i => i.CompanyName == v.CompanyName && i.CompanyID != v.CompanyID);
            if (isCompanyNameExist)
            {
                return Json(new { Success = false, ErrorMessage = "Company name previously exist" }, JsonRequestBehavior.DenyGet);
            }
            LU_tbl_CompanyInformation company;
            if (v.CompanyID == 0)
            {
                int getMaxId = unitOfWork.CompanyInformationRepository.Get().Max(m => (int?)m.CompanyID) ?? 0;
                v.CompanyID = getMaxId + 1;
                company = new LU_tbl_CompanyInformation();
                company.CompanyID = v.CompanyID;
                company.AccountingYearBeginningFrom = v.AccountingYearBeginningFrom;
                company.CompanyAddress = v.CompanyAddress;
                company.CompanyName = v.CompanyName;
                company.SystemImplementationDate = v.SystemImplementationDate;
                company.OCode = v.CompanyID;
                unitOfWork.CompanyInformationRepository.Insert(company);
            }
            else if (v.CompanyID > 0)
            {
                company = unitOfWork.CompanyInformationRepository.GetByID(v.CompanyID);
                company.AccountingYearBeginningFrom = v.AccountingYearBeginningFrom;
                company.CompanyAddress = v.CompanyAddress;
                company.CompanyName = v.CompanyName;
                company.SystemImplementationDate = v.SystemImplementationDate;
                company.OCode = v.CompanyID;
                unitOfWork.CompanyInformationRepository.Update(company);
            }
            else
            {
                return Json(new { Success = false, ErrorMessage = "Undefined State! CompanyID Don't fall under any criteria!" }, JsonRequestBehavior.DenyGet);
            }
            //save data
            try
            {
                company.EditDate = DateTime.Now;
                company.EditUserName = User.Identity.Name;
                company.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                unitOfWork.Save();
                return Json(new { Success = true, Message = "Company information saved successfully...", CompanyID = v.CompanyID }, JsonRequestBehavior.DenyGet);
            }
            catch (Exception x)
            {
                return Json(new { Success = false, ErrorMessage = "Error : " + x.Message }, JsonRequestBehavior.DenyGet);
            }
        }

        //step 4
        /// <summary>
        /// Creates the forfeiture account.
        /// </summary>
        /// <returns></returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>Mar-6-2016</ModificationDate>
        [Authorize]
        public ActionResult CreateForfeitureAccount()
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                ViewBag.Message = "You must associate with an company to continue...";
                return View("404");
            }
            //Account with name "Forfeiture Account" should be in database
            var v = unitOfWork.ACC_LedgerRepository.Get(w => w.LedgerName == "Forfeiture" && w.OCode == oCode).SingleOrDefault();
            if (v == null)
            {
                return Content("Forfeiture not found in ledger list. Please create forfeiture ledger and try again!");
            }
            ViewBag.EditUserName = unitOfWork.CustomRepository.GetUserName(v.EditUser ?? Guid.NewGuid());
            ViewBag.EditDate = v.EditDate;
            ViewBag.ForfeitedAmount = v.InitialBalance ?? 0;
            var companyInformation = unitOfWork.CompanyInformationRepository.Get(o => o.OCode == oCode).FirstOrDefault();
            if (companyInformation != null)
            {
                ViewBag.ImplementationDate = companyInformation.SystemImplementationDate;
            }
            else
            {
                ViewBag.Message = "Please updated system implementation date!";
                return View("404");
            }
            return View("ForfeitureAccount");
        }

        /// <summary>
        /// Creates the forfeiture account.
        /// </summary>
        /// <param name="ForfeitedAmount">The forfeited amount.</param>
        /// <returns></returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>Mar-6-2016</ModificationDate>
        [HttpPost]
        public ActionResult CreateForfeitureAccount(string ForfeitedAmount)
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 1);
            if (!b)
            {
                return Json(new { Success = false, ErrorMessage = "You donot have permission in this section." }, JsonRequestBehavior.DenyGet);
            }
            //End
            var v = unitOfWork.ACC_LedgerRepository.Get(w => w.LedgerName == "Forfeiture").SingleOrDefault();
            if (v == null)
            {
                return Json(new { Success = false, ErrorMessage = "Forfeiture not found in ledger list. Please create forfeiture ledger and try again!" }, JsonRequestBehavior.DenyGet);
            }
            try
            {
                decimal d = Convert.ToDecimal(ForfeitedAmount);
                v.InitialBalance = d;
                v.BalanceType = 1;// credit
                v.EditDate = DateTime.Now;
                v.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                unitOfWork.Save();
                return Json(new { Success = true, Message = "Forfeited ledger has been updated!" }, JsonRequestBehavior.DenyGet);
            }
            catch
            {
                return Json(new { Success = false, ErrorMessage = "Forfeited amount not in correct format!" }, JsonRequestBehavior.DenyGet);
            }
        }

        public ActionResult CompanyList()
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            var v = unitOfWork.CompanyInformationRepository.Get().Where(x=>x.OCode == oCode).ToList();
            return PartialView("_CompanyList", v);
        }

        public ActionResult CompanyDeletePossible(int id)
        {
            if (_CompanyDeletePossible(id))
            {
                return Json(new { Success = true, Message = "This company can be deleted safely..." }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Success = false, ErrorMessage = "Pending..." }, JsonRequestBehavior.AllowGet);
        }

        private bool _CompanyDeletePossible(int id)
        {
            bool isCompanyUsedInUser = unitOfWork.UserProfileRepository.IsExist(o => o.OCode == id);
            if (!isCompanyUsedInUser)
            {
                bool isCompanyUsedInVoucherInfo = unitOfWork.ACC_VoucherEntryRepository.IsExist(o => o.OCode == id);
                if (!isCompanyUsedInVoucherInfo)
                {
                    return true;
                }
            }
            return false;
        }

        [HttpPost]
        public ActionResult CompanyDeleteConfirm(int id)
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 2);
            if (!b)
            {
                return Json(new { Success = false, ErrorMessage = "You donot have permission in this section." }, JsonRequestBehavior.DenyGet);
            }
            //End
            try
            {
                unitOfWork.CompanyInformationRepository.Delete(id);
                unitOfWork.Save();
                return Json(new { Success = true, Message = "Company Deleted Successfully!!!" }, JsonRequestBehavior.DenyGet);
            }
            catch (Exception x)
            {
                return Json(new { Success = false, ErrorMessage = "Following error occured while deleting: " + x.Message }, JsonRequestBehavior.DenyGet);
            }
        }
    }
}
