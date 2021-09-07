using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DLL.Repository;
using System;

namespace PFMVC.Controllers
{
    public class NotificationFactoryController : Controller
    {
        UnitOfWork unitOfWork = new UnitOfWork();
        public int totalNotification = 0;
        public int employeeRecordPending = 0;
        public int salaryTransactionPending = 0;
        public int instrumentTransactionPending = 0;
        public int coutributionMonthPending = 0;



        
        public ActionResult GetNotificationPF()
        {
            //employeeRecordPending = AccountTransactionEmployee();
            salaryTransactionPending = AccountTransactionSalary();
            //instrumentTransactionPending = AccountTransactionInstrument();
            //coutributionMonthPending = ContributionMonthPending();
            totalNotification = employeeRecordPending + salaryTransactionPending + instrumentTransactionPending + coutributionMonthPending;

            if (totalNotification > 0)
            {
                ViewBag.TotalCount = totalNotification;
            }
            return PartialView("Notification");
        }

        
        public ActionResult GetNotificationAccounting()
        { 
            ViewBag.TotalCount = totalNotification;
            return PartialView("Notification");
        }

        //public int AccountTransactionEmployee()
        //{
        //    int v = unitOfWork.EmployeesRepository.Get(w => w.PassVoucher == false).Count();
        //    return v;
        //}

        public int AccountTransactionSalary()
        {
            int v = unitOfWork.ContributionMonthRecordRepository.Get(w => w.PassVoucher == false).Count();
            return v;
        }

        //public int AccountTransactionInstrument()
        //{
        //    int v = unitOfWork.InstrumentRepository.Get(w => w.PassVoucher == false).Count();
        //    return v;
        //}

        //public int ContributionMonthPending()
        //{
        //    //int v = unitOfWork.ContributionMonthListRepository.Get(x => (DateTime.Now.Month - x.ProcessDate.Month) > 4).Count();
        //    int v = unitOfWork.CustomRepository.GetContributionPendingMonth();
        //    return v;
        //}
        

        [Authorize]
        public ActionResult NotificationDetailPF()
        {
            List<string> stringList = new List<string>();
            List<string> linkList = new List<string>();
            //Detail notification will be added here time to time...
            //employeeRecordPending = AccountTransactionEmployee();
            salaryTransactionPending = AccountTransactionSalary();
            //instrumentTransactionPending = AccountTransactionInstrument();
            //coutributionMonthPending = ContributionMonthPending();
            if (coutributionMonthPending > 0)
            {
                stringList.Add(coutributionMonthPending + " employee contribution month pending. ");
                linkList.Add("/Salary1/PendingCountribution/");
            }
            if (employeeRecordPending > 0)
            {
                stringList.Add(employeeRecordPending + " employee account voucher pass pending. ");
                linkList.Add("/Employee/PassVoucher/");
            }
            if (salaryTransactionPending > 0)
            {
                stringList.Add(salaryTransactionPending + " Contribution account voucher pass pending.");
                linkList.Add("/Salary1/Import/");
            }
            if (instrumentTransactionPending > 0)
            {
                stringList.Add(instrumentTransactionPending + " instrument account voucher pass pending.");
                linkList.Add("/Instrument/Instrument/Index/");
            }
            ViewBag.NotificationDetail = stringList;
            ViewBag.Link = linkList;
            return View("NotificationDetail");
        }

        [Authorize]
        public ActionResult NotificationDetailAccounting()
        {
            List<string> stringList = new List<string>();
            List<string> linkList = new List<string>();
            ////Detail notification will be added here time to time...
            //employeeRecordPending = AccountTransactionEmployee();
            //salaryTransactionPending = AccountTransactionSalary();
            //instrumentTransactionPending = AccountTransactionInstrument();
            //if (employeeRecordPending > 0)
            //{
            //    stringList.Add(employeeRecordPending + " employee account voucher pass pending. ");
            //    linkList.Add("/Employee/PassVoucher/");
            //}
            //if (salaryTransactionPending > 0)
            //{
            //    stringList.Add(salaryTransactionPending + " salary account voucher pass pending.");
            //    linkList.Add("/Salary1/Import/");
            //}
            //if (instrumentTransactionPending > 0)
            //{
            //    stringList.Add(instrumentTransactionPending + " instrument account voucher pass pending.");
            //    linkList.Add("/Instrument/Instrument/Index/");
            //}
            ViewBag.NotificationDetail = stringList;
            ViewBag.Link = linkList;
            return View("NotificationDetail");
        }


    }
}
