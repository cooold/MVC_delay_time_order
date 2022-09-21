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

namespace prjShoppingCar.Controllers
{
    [Authorize]  //指定Member控制器所有的動作方法必須通過授權才能執行。
    public class MemberController : Controller
    {
        //建立可存取dbShoppingCar.mdf 資料庫的dbShoppingCarEntities 類別物件db
        dbShoppingCarEntities db = new dbShoppingCarEntities();

        //日期
        DateTime myDate = DateTime.Today;


        // GET: Member/Index
        public ActionResult Index()
        {
            return View("../Home/Index", "_LayoutMember");
        }

        public ActionResult Index_introduce()
        {
            var products = db.tProduct.OrderByDescending(m => m.fId).ToList();
            return View("../Home/Index_introduce", "_LayoutMember", products);
        }

        //Get:Member/Logout
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();   // 登出
            return RedirectToAction("Index", "Home");
        }

        //Get:Member/ShoppingCar
        public ActionResult ShoppingCar()
        {
            //取得登入會員的帳號並指定給fUserId
            string fUserId = User.Identity.Name;
            //找出未成為訂單明細的資料，即購物車內容
            var orderDetails = db.tOrderDetail.Where
                (m => m.fUserId == fUserId && m.fIsApproved == "否")
                .ToList();


            //查詢是否忙碌狀態
            var BusinessStatus = db.tbusinessStatus
               .ToList();
            if (BusinessStatus.Last().Status == 1)
            {
                ViewBag.Status = 1;
                ViewBag.StatusMessage = "店家目前為忙碌中，暫時停止接受訂單，敬請見諒";
            }
            //傳送會員名稱和電話
            var nowMember = db.tMember
                .Where(m => m.fUserId == fUserId).ToList();
            ViewBag.shoping_name = nowMember[0].fName;
            ViewBag.shoping_phone = nowMember[0].fphone;

            //View使用orderDetails模型
            return View(orderDetails);
        }

       
        //Get:Member/AddCar
        public ActionResult AddCar(string fPId, int num)
        {
            //取得會員帳號並指定給fUserId
            string fUserId = User.Identity.Name;
            //找出會員放入訂單明細的產品，該產品的fIsApproved為"否"
            //表示該產品是購物車狀態
            var currentCar = db.tOrderDetail
                .Where(m => m.fPId == fPId && m.fIsApproved == "否" && m.fUserId == fUserId)
                .FirstOrDefault();
            //若currentCar等於null，表示會員選購的產品不是購物車狀態
            if (currentCar == null)
            {
                //找出目前選購的產品並指定給product
                var product = db.tProduct.Where(m => m.fPId == fPId).FirstOrDefault();
                //將產品放入訂單明細，因為產品的fIsApproved為"否"，表示為購物車狀態
                tOrderDetail orderDetail = new tOrderDetail();
                orderDetail.fUserId = fUserId;
                orderDetail.fPId = product.fPId;
                orderDetail.fName = product.fName;
                orderDetail.fPrice = (int)product.fPrice;
                orderDetail.fQty = num;
                orderDetail.fIsApproved = "否";
                db.tOrderDetail.Add(orderDetail);
            }
            else
            {
                //若產品為購物車狀態，即將該產品數量加上
                currentCar.fQty += num;
            }
            db.SaveChanges();
            Thread.Sleep(1500);
            var products = db.tProduct.OrderByDescending(m => m.fId).ToList();
            return View("../Home/Index_introduce", "_LayoutMember", products);
        }


        //Get:Member/DeleteCar
        public ActionResult DeleteCar(int fId)
        {
            // 依fId找出要刪除購物車狀態的產品
            var orderDetail = db.tOrderDetail.Where
                (m => m.fId == fId).FirstOrDefault();
            //刪除購物車狀態的產品
            db.tOrderDetail.Remove(orderDetail);
            db.SaveChanges();
            Thread.Sleep(1500);
            return RedirectToAction("ShoppingCar");
        }

        //Post:Member/ShoppingCar
        [HttpPost]
        public ActionResult ShoppingCar(string fReceiver, string fPhone, string fGetTime, string fRemark,int ftotal,string delayRange)
        {
            //找出會員帳號並指定給fUserId
            string fUserId = User.Identity.Name;
            //建立唯一的識別值並指定給guid變數，用來當做訂單編號
            //tOrder的fOrderGuid欄位會關聯到tOrderDetail的fOrderGuid欄位
            //形成一對多的關係，即一筆訂單資料會對應到多筆訂單明細
            string guid = Guid.NewGuid().ToString();

            //建立訂單主檔資料
            tOrder order = new tOrder();

            //查詢今日訂單是幾號
            var getTodayGuid = db.tOrder
               .ToList();
            int todaynum = 0;
            //如果最新訂單是今日則+1

            if (getTodayGuid.Last().fDate == myDate)
            {
                todaynum = int.Parse(getTodayGuid.Last().take_meal_number.ToString()) + 1;
                order.take_meal_number = todaynum;
            }
            //不是改成1
            else
            {
                todaynum = 1;
                order.take_meal_number = 1;
            }

            order.fOrderGuid = guid;
            order.fUserId = fUserId;
            order.fReceiver = fReceiver;
            order.fPhone = fPhone;
            order.fGetTime = fGetTime;
            order.delayRange = delayRange;
            order.fRemark = fRemark;
            order.ftotal = ftotal;
            order.fstate = 0;
            order.fDate = DateTime.Today;
            db.tOrder.Add(order);
            //找出目前會員在訂單明細中是購物車狀態的產品
            var carList = db.tOrderDetail
                .Where(m => m.fIsApproved == "否" && m.fUserId == fUserId)
                .ToList();
            //將購物車狀態產品的fIsApproved設為"是"，表示確認訂購產品
            foreach (var item in carList)
            {
                item.fOrderGuid = guid;
                item.fIsApproved = "是";
            }
            //更新資料庫，異動tOrder和tOrderDetail
            //完成訂單主檔和訂單明細的更新
            db.SaveChanges();
            Thread.Sleep(1500);
            return RedirectToAction("OrderList", new { state_all = 0 });
        }


        //Get:Member/OrderList
        public ActionResult OrderList(int state_all)
        {
            //找出會員帳號並指定給fUserId
            string fUserId = User.Identity.Name;
            //找出目前會員的所有訂單主檔記錄並依照fDate進行遞增排序
            //將查詢結果指定給orders
            dynamic mix = new ExpandoObject();
            if (state_all == 0)
            {
                //今日訂單
                mix.order = db.tOrder.Where(m => m.fUserId == fUserId && m.fDate == myDate)
                .OrderByDescending(m => m.fDate).ToList();
                mix.tOrderDetail = db.tOrderDetail
                    .Where(m => m.fUserId == fUserId).ToList();
            }
            else
            {
                //今日訂單
                mix.order = db.tOrder.Where(m => m.fUserId == fUserId)
                .OrderByDescending(m => m.fDate).ToList();
                mix.tOrderDetail = db.tOrderDetail
                    .Where(m => m.fUserId == fUserId).ToList();
            }



            return View(mix);
        }
    }
}