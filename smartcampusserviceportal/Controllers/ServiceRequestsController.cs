using System;
using System.Web.Mvc;
using smartcampusserviceportal.Data;
using smartcampusserviceportal.Models;

namespace smartcampusserviceportal.Controllers
{
    public class ServiceRequestsController : Controller
    {
        private readonly ServiceRequestRepository repository = new ServiceRequestRepository();

        public ActionResult Index()
        {
            return View(repository.GetAll());
        }

        public ActionResult Create()
        {
            return View(new ServiceRequest
            {
                Status = "Open",
                CreatedAt = DateTime.Now
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ServiceRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            request.CreatedAt = DateTime.Now;
            repository.Add(request);
            TempData["Message"] = "Service request created.";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var request = repository.GetById(id);
            return request == null ? HttpNotFound() : View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, ServiceRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            request.Id = id;
            if (!repository.Update(request))
            {
                return HttpNotFound();
            }

            TempData["Message"] = "Service request updated.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            repository.Delete(id);
            TempData["Message"] = "Service request deleted.";
            return RedirectToAction("Index");
        }
    }
}
