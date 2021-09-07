using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CustomJsonResponse;
using DLL;
using DLL.DataPrepare;
using DLL.Repository;
using DLL.ViewModel;
using PFMVC.common;
using Telerik.Web.Mvc;

namespace PFMVC.Areas.PFSettings.Controllers
{
    public class YearlyInterestController : Controller
    {
        int PageID = 5;
        private UnitOfWork unitOfWork = new UnitOfWork();
        DP_PFStatus dp_pfStatus = new DP_PFStatus();

        public ActionResult YearlyInterest()
        {
            return View("YearlyInterest");
        }


        [GridAction]
        public ActionResult _SelectPFYearlyInterest()
        {
            return View(new GridModel(GetPFYearlyInterest()));
        }


        private IEnumerable<VM_InterestRate> GetPFYearlyInterest()
        {
            var result = unitOfWork.CustomRepository.GetInterestRate().ToList();
            List<VM_InterestRate> lst_vm_interestRate = new List<VM_InterestRate>();

            foreach (var item in result)
            {
                lst_vm_interestRate.Add(item);
            }
            return lst_vm_interestRate;
        }

        public ActionResult YearlyInterestForm(string conYear, string conMonth)
        {
            VM_InterestRate vm_interest = new VM_InterestRate();
            if (!string.IsNullOrEmpty(conYear) && !string.IsNullOrEmpty(conMonth))
            {
                bool isRecordExist = unitOfWork.InterestRateRepository.IsExist(i => i.ConYear == conYear && i.ConMonth == conMonth);
                if (!isRecordExist)
                {
                    return Json(new { Success = false, ErrorMessage = "This record does not exist" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    vm_interest = unitOfWork.CustomRepository.GetInterestRate().Where(w => w.ConYear == conYear && w.ConMonth == conMonth).FirstOrDefault();
                    vm_interest.EffectiveFrom = Convert.ToDateTime("01/" + conMonth + "/" + conYear);

                }
            }
            return PartialView("_YearlyInterestForm", vm_interest);
        }


        [HttpPost]
        public ActionResult YearlyInterestForm(VM_InterestRate v)
        {
            bool IsAllowedToEdit = PagePermission.HasPermission(User.Identity.Name, PageID, 1);
            if (!IsAllowedToEdit) return Json(new { Success = false, ErrorMessage = "You are not allowed to edit information! contact system admin!" }, JsonRequestBehavior.AllowGet);

            string year;
            string month;

            if (ModelState.IsValid)
            {
                DateTime datetime;
                if (DateTime.TryParse(v.EffectiveFrom + "", out datetime))
                {
                    year = datetime.Year + "";
                    month = (datetime.Month + "").PadLeft(2, '0');


                }
                else
                {
                    return Json(new { Success = false, ErrorMessage = "Month year not valid..." }, JsonRequestBehavior.AllowGet);
                }
                bool isRecordExist = unitOfWork.InterestRateRepository.IsExist(i => i.ConYear == year && i.ConMonth == month && i.RowId != v.RowId);

                if (isRecordExist)
                {
                    return Json(new { Success = false, ErrorMessage = "Another record for this month and year exist..." }, JsonRequestBehavior.AllowGet);
                }
                if (v.RowId > 0)
                {
                    tbl_InterestRate tbl_interestRate = unitOfWork.InterestRateRepository.Get().Where(w => w.ConMonth == month && w.ConYear == year).Single();
                    tbl_interestRate.ConYear = year;
                    tbl_interestRate.ConMonth = month;
                    tbl_interestRate.InterestRate = v.InterestRate;
                    tbl_interestRate.EditDate = System.DateTime.Now;
                    tbl_interestRate.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                    unitOfWork.InterestRateRepository.Update(tbl_interestRate);
                }
                else
                {

                    tbl_InterestRate tbl_interestRate = new tbl_InterestRate();
                    tbl_interestRate.RowId = GetMaxID()+1;
                    tbl_interestRate.ConYear = year;
                    tbl_interestRate.ConMonth = month;
                    tbl_interestRate.InterestRate = v.InterestRate;
                    tbl_interestRate.EditDate = System.DateTime.Now;
                    tbl_interestRate.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                    unitOfWork.InterestRateRepository.Insert(tbl_interestRate);
                }
                try
                {
                    unitOfWork.Save();
                    return Json(new { Success = true, Message = "Record successfully updated..." }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception x)
                {
                    return Json(new { Success = false, ErrorMessage = "Check error : " + x.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            var errorList = (from item in ModelState.Values
                             from error in item.Errors
                             select error.ErrorMessage).ToList();

            return Json(JsonResponseFactory.ErrorResponse(errorList), JsonRequestBehavior.DenyGet);
        }

        public bool YearlyInterestFormValidation(string conYear, string conMonth)
        {
            bool isRecoredAlreadyUsed = unitOfWork.ContributionRepository.IsExist(w => w.ConMonth == conMonth && w.ConYear == conYear);
            return isRecoredAlreadyUsed;
        }

        public int GetMaxID()
        {
            //var query = "SELECT isnull(MAX(convert(int, BranchID)),0) FROM [ICMS_LIVE].[dbo].[LU_tbl_Branch]";
            int data;
            try
            {
                data = unitOfWork.InterestRateRepository.Get().Select(m => Convert.ToInt32(m.RowId)).Max();
            }
            catch
            {
                data = 0000;
            }
            //var max_id = data.
            //string s = (Convert.ToInt16(data) + 1) + "";
            //string s = (Convert.ToInt16(data) + 1) + "";
            //return s.Trim().PadLeft(4, '0');'
            return data;
        }



    }
}
