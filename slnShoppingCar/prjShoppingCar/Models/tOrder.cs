//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace prjShoppingCar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;


    public partial class tOrder
    {
        public int fId { get; set; }

        [DisplayName("訂單編號")]
        public string fOrderGuid { get; set; }

        [DisplayName("會員帳號")]
        public string fUserId { get; set; }

        [DisplayName("訂購人姓名")]
        [Required]
        public string fReceiver { get; set; }

        [DisplayName("聯絡電話")]
        [Required]
        public string fPhone { get; set; }

        [DisplayName("取餐時間")]
        [Required]
        public string fGetTime { get; set; }

        [DisplayName("訂單備註")]
        public string fRemark { get; set; }

        [DisplayName("訂購時間")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime? fDate { get; set; }

        [DisplayName("可接受延誤時間")]
        public string delayRange { get; set; }

        [DisplayName("總金額")]
        public int ftotal { get; set; }


        [DisplayName("訂單狀態")]
        public int fstate { get; set; }
        public Nullable<int> take_meal_number { get; set; }
    }
}
