using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading;
using System.Web.Security;
using prjShoppingCar.Models;
using System.Collections;
using System.Dynamic;
using System.Web.Configuration;

namespace prjShoppingCar.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        //建立可存取dbShoppingCar.mdf 資料庫的dbShoppingCarEntities 類別物件db
        dbShoppingCarEntities db = new dbShoppingCarEntities();
        DateTime myDate = DateTime.Today;
        // GET: Manage
        public ActionResult Index()
        {
            return View("../Manage/Index", "_LayoutManage");
        }

        //Get:Member/Logout
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();   // 登出
            return RedirectToAction("Index", "Home");
        }

        //製作時間圖表
        public ActionResult makePlanChar()
        {
            //抓出已經接受訂單
            var adjust = db.tOrder.Where(m => m.fstate == 1 && m.fDate == myDate)
                .OrderByDescending(m => m.fDate).ToList();
            return View(adjust);
        }

        public ActionResult setBusinessStatus()
        {
            //取得最新一筆狀態
            var BusinessStatus = db.tbusinessStatus
               .ToList();
            return View(BusinessStatus.Last());
        }

        [HttpPost]
        public ActionResult setBusinessStatus(int Status )
        {
            string fUserId = User.Identity.Name;

            tbusinessStatus businessStatus = new tbusinessStatus();
            businessStatus.time = DateTime.Now;
            businessStatus.username = fUserId;
            businessStatus.Status = Status;
            db.tbusinessStatus.Add(businessStatus);
            db.SaveChanges();

            //取得最新一筆狀態
            var BusinessStatus = db.tbusinessStatus
               .ToList();
            return View(BusinessStatus.Last());
        }

        public ActionResult newProduct()
        {
            return View();
        }

        [HttpPost]
        public ActionResult newProduct
            (tProduct vproduct , HttpPostedFileBase fproductImg)
        {
            //確認id是否重複
            var oldproduct = db.tProduct
                .Where(m => m.fPId == vproduct.fPId)
                .FirstOrDefault();

            if (oldproduct == null)
            {
                try
                {
                    //上傳圖
                    string fileName = "";
                    //上傳
                    if (fproductImg != null)
                    {
                        if (fproductImg.ContentLength > 0)
                        {
                            fileName = Guid.NewGuid().ToString() + ".jpg";
                            var path = System.IO.Path.Combine
                                (Server.MapPath("~/images"), fileName);
                            fproductImg.SaveAs(path);
                        }
                    }
                    tProduct product = new tProduct();
                    product.fPId = vproduct.fPId;
                    product.fName = vproduct.fName;
                    product.fPrice = vproduct.fPrice;
                    product.makeTImes = vproduct.makeTImes;
                    product.pState = 0;
                    product.fImg = fileName;
                    db.tProduct.Add(product);
                    db.SaveChanges();
                    return RedirectToAction("AdjustProduct");
                }
                catch (Exception ex)
                {
                    ViewBag.Error = ex.Message;
                }
            }
            ViewBag.Error = "此產品編號已使用，請重新命名。";
            return View();
        }

        public ActionResult DeleteProduct()
        {
            //取得所有產品放入products
            var products = db.tProduct
                .OrderByDescending(m => m.fId).ToList();
            return View(products);
        }
        public ActionResult Delete(string fPId)
        {
            var product = db.tProduct.Where(m => m.fPId == fPId)
                .FirstOrDefault();
            string fileName = product.fImg;
            if (fileName != "")
            {
                //刪除圖片
                System.IO.File.Delete(Server.MapPath("~/images")
                    + "/" + fileName);
            }
            db.tProduct.Remove(product);
            db.SaveChanges();
            return RedirectToAction("DeleteProduct");
        }

        public ActionResult AdjustProduct()
        {
            //取得所有產品放入products
            var products = db.tProduct
                .OrderByDescending(m => m.fId).ToList();
            return View(products);
        }
        [HttpPost]
        public ActionResult AdjustProduct(string fPId,string fName,int fPrice,int makeTImes)
        {
            //查詢產品編號
            var adjust = db.tProduct
                .Where(m => m.fPId == fPId)
                .ToList();
            //將購物車狀態產品的fIsApproved設為"是"，表示確認訂購產品
            foreach (var item in adjust)
            {
                item.fName = fName;
                item.fPrice = fPrice;
                item.makeTImes = makeTImes;
            }
            //更新資料庫，異動tOrder和tOrderDetail
            //完成訂單主檔和訂單明細的更新
            db.SaveChanges();

            //取得所有產品放入products
            var products = db.tProduct
                .OrderByDescending(m => m.fId).ToList();


            return View(products);
        }

        public ActionResult released()
        {
            //取得所有產品放入products
            var products = db.tProduct
                .OrderByDescending(m => m.fId).ToList();
            return View(products);
        }
        [HttpPost]
        public ActionResult released(string fPId)
        {
            //查詢產品編號
            var adjust = db.tProduct
                .Where(m => m.fPId == fPId)
                .ToList();
            //將購物車狀態產品的fIsApproved設為"是"，表示確認訂購產品
            foreach (var item in adjust)
            {
                if (item.pState == 0)
                {
                    item.pState = 1;
                }
                else
                {
                    item.pState = 0;
                }
            }
            //更新資料庫，異動tOrder和tOrderDetail
            //完成訂單主檔和訂單明細的更新
            db.SaveChanges();


            //取得所有產品放入products
            var products = db.tProduct
                .OrderByDescending(m => m.fId).ToList();
            return View(products);
        }
        public ActionResult getOrder(int state_all)
        {

            if (state_all == 0)
            {

                //今日訂單
                dynamic mix = new ExpandoObject();
                mix.order = db.tOrder.Where(m => m.fDate == myDate)
                        .OrderByDescending(m => m.fDate).ToList();
                mix.tOrderDetail = db.tOrderDetail
                    .ToList();
                return View(mix);
            }
            else
            {
                //所有訂單
                dynamic mix = new ExpandoObject();
                mix.order = db.tOrder
                        .OrderByDescending(m => m.fDate).ToList();
                mix.tOrderDetail = db.tOrderDetail
                    .ToList();
                return View(mix);
            }
        }

        public ActionResult newOrder(int state_all)
        {

            if (state_all == 0)
            {
                ViewBag.OrderlistColor = 0;
                //今日訂單 用網站選狀態
                dynamic mix = new ExpandoObject();
                mix.order = db.tOrder.Where(m => m.fDate == myDate)
                        .OrderByDescending(m => m.fDate).ToList();
                mix.tOrderDetail = db.tOrderDetail
                    .ToList();
                return View(mix);
            }
            else
            {
                ViewBag.OrderlistColor = 1;
                //所有訂單
                dynamic mix = new ExpandoObject();
                mix.order = db.tOrder.Where(m => m.fstate == 0)
                        .OrderByDescending(m => m.fDate).ToList();
                mix.tOrderDetail = db.tOrderDetail
                    .ToList();
                return View(mix);
            }

        }

        public ActionResult paymentOrder(int state_all)
        {
            if (state_all == 0)
            {
                ViewBag.OrderlistColor = 0;
                //今日訂單
                dynamic mix = new ExpandoObject();
                mix.order = db.tOrder.Where(m => m.fstate == 1 && m.fDate == myDate)
                        .OrderByDescending(m => m.fDate).ToList();
                mix.tOrderDetail = db.tOrderDetail
                    .ToList();
                return View(mix);
            }
            else
            {
                ViewBag.OrderlistColor = 1;
                //所有訂單
                dynamic mix = new ExpandoObject();
                mix.order = db.tOrder.Where(m => m.fstate == 1)
                        .OrderByDescending(m => m.fDate).ToList();
                mix.tOrderDetail = db.tOrderDetail
                    .ToList();
                return View(mix);
            }
        }
        public ActionResult cancelOrder(int state_all)
        {
            if (state_all == 0)
            {
                //今日訂單
                dynamic mix = new ExpandoObject();
                mix.order = db.tOrder.Where(m => m.fstate == 2 && m.fDate == myDate)
                        .OrderByDescending(m => m.fDate).ToList();
                mix.tOrderDetail = db.tOrderDetail
                    .ToList();
                return View(mix);
            }
            else
            {
                //所有訂單
                dynamic mix = new ExpandoObject();
                mix.order = db.tOrder.Where(m => m.fstate == 2)
                        .OrderByDescending(m => m.fDate).ToList();
                mix.tOrderDetail = db.tOrderDetail
                    .ToList();
                return View(mix);
            }
        }


        public ActionResult completeOrder(int state_all)
        {
            if (state_all == 0)
            {
                //今日訂單
                dynamic mix = new ExpandoObject();
                mix.order = db.tOrder.Where(m => m.fstate == 3 && m.fDate == myDate)
                        .OrderByDescending(m => m.fDate).ToList();
                mix.tOrderDetail = db.tOrderDetail
                    .ToList();
                return View(mix);
            }
            else
            {
                //所有訂單
                dynamic mix = new ExpandoObject();
                mix.order = db.tOrder.Where(m => m.fstate == 3)
                        .OrderByDescending(m => m.fDate).ToList();
                mix.tOrderDetail = db.tOrderDetail
                    .ToList();
                return View(mix);
            }
        }

        public ActionResult acceptOrder_new(string fOrderGuid)
        {
            //修改狀態為接受
            var accept = db.tOrder
                .Where(m => m.fOrderGuid==fOrderGuid)
                .ToList();
            //將購物車狀態產品的fIsApproved設為"是"，表示確認訂購產品
            foreach (var item in accept)
            {
                item.fstate = 1;
            }
            //更新資料庫，異動tOrder和tOrderDetail
            //完成訂單主檔和訂單明細的更新
            db.SaveChanges();

            //新進訂單確認
            dynamic mix = new ExpandoObject();
            mix.order = db.tOrder.Where(m => m.fstate == 0 && m.fDate == myDate)
                    .OrderByDescending(m => m.fDate).ToList();
            mix.tOrderDetail = db.tOrderDetail
                .ToList();
            return View("../Manage/newOrder", "_LayoutManage",mix);
        }

        public ActionResult removeOrders_new(string fOrderGuid)
        {
            //修改狀態為接受
            var accept = db.tOrder
                .Where(m => m.fOrderGuid == fOrderGuid)
                .ToList();
            //將購物車狀態產品的fIsApproved設為"是"，表示確認訂購產品
            foreach (var item in accept)
            {
                item.fstate = 2;
            }
            //更新資料庫，異動tOrder和tOrderDetail
            //完成訂單主檔和訂單明細的更新
            db.SaveChanges();

            //新進訂單確認
            dynamic mix = new ExpandoObject();
            mix.order = db.tOrder.Where(m => m.fstate == 0 && m.fDate == myDate)
                    .OrderByDescending(m => m.fDate).ToList();
            mix.tOrderDetail = db.tOrderDetail
                .ToList();
            return View("../Manage/newOrder", "_LayoutManage", mix);
        }

        public ActionResult removeOrders_payment(string fOrderGuid)
        {
            //修改狀態為接受
            var accept = db.tOrder
                .Where(m => m.fOrderGuid == fOrderGuid)
                .ToList();
            //將購物車狀態產品的fIsApproved設為"是"，表示確認訂購產品
            foreach (var item in accept)
            {
                item.fstate = 2;
            }
            //更新資料庫，異動tOrder和tOrderDetail
            //完成訂單主檔和訂單明細的更新
            db.SaveChanges();

            dynamic mix = new ExpandoObject();
            mix.order = db.tOrder.Where(m => m.fstate == 1 && m.fDate == myDate)
                    .OrderByDescending(m => m.fDate).ToList();
            mix.tOrderDetail = db.tOrderDetail
                .ToList();
            return View("../Manage/paymentOrder", "_LayoutManage", mix);
        }

        public ActionResult completeOrders_payment(string fOrderGuid)
        {
            //修改狀態為接受
            var accept = db.tOrder
                .Where(m => m.fOrderGuid == fOrderGuid)
                .ToList();
            //將購物車狀態產品的fIsApproved設為"是"，表示確認訂購產品
            foreach (var item in accept)
            {
                item.fstate = 3;
            }
            //更新資料庫，異動tOrder和tOrderDetail
            //完成訂單主檔和訂單明細的更新
            db.SaveChanges();

            dynamic mix = new ExpandoObject();
            mix.order = db.tOrder.Where(m => m.fstate == 1 && m.fDate == myDate)
                    .OrderByDescending(m => m.fDate).ToList();
            mix.tOrderDetail = db.tOrderDetail
                .ToList();
            return View("../Manage/paymentOrder", "_LayoutManage", mix);
        }

        public ActionResult toPayment_complete(string fOrderGuid)
        {
            //修改狀態為接受
            var accept = db.tOrder
                .Where(m => m.fOrderGuid == fOrderGuid)
                .ToList();
            //將購物車狀態產品的fIsApproved設為"是"，表示確認訂購產品
            foreach (var item in accept)
            {
                item.fstate = 1;
            }
            //更新資料庫，異動tOrder和tOrderDetail
            //完成訂單主檔和訂單明細的更新
            db.SaveChanges();

            dynamic mix = new ExpandoObject();
            mix.order = db.tOrder.Where(m => m.fstate == 3 && m.fDate == myDate)
                    .OrderByDescending(m => m.fDate).ToList();
            mix.tOrderDetail = db.tOrderDetail
                .ToList();
            return View("../Manage/completeOrder", "_LayoutManage", mix);
        }
        public ActionResult toPayment_cancel(string fOrderGuid)
        {
            //修改狀態為接受
            var accept = db.tOrder
                .Where(m => m.fOrderGuid == fOrderGuid)
                .ToList();
            //將購物車狀態產品的fIsApproved設為"是"，表示確認訂購產品
            foreach (var item in accept)
            {
                item.fstate = 1;
            }
            //更新資料庫，異動tOrder和tOrderDetail
            //完成訂單主檔和訂單明細的更新
            db.SaveChanges();

            dynamic mix = new ExpandoObject();
            mix.order = db.tOrder.Where(m => m.fstate == 2 && m.fDate == myDate)
                    .OrderByDescending(m => m.fDate).ToList();
            mix.tOrderDetail = db.tOrderDetail
                .ToList();
            return View("../Manage/cancelOrder", "_LayoutManage", mix);
        }


    }
}