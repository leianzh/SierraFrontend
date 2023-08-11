﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace SIERRA_Server.Models.EFModels
{
    public partial class MemberCoupon
    {
        public MemberCoupon()
        {
            DessertCarts = new HashSet<DessertCart>();
            DessertOrders = new HashSet<DessertOrder>();
        }

        public int MemberCouponId { get; set; }
        public int MemberId { get; set; }
        public int CouponId { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UseAt { get; set; }
        public DateTime ExpireAt { get; set; }
        public string CouponName { get; set; }

        public virtual Coupon Coupon { get; set; }
        public virtual Member Member { get; set; }
        public virtual ICollection<DessertCart> DessertCarts { get; set; }
        public virtual ICollection<DessertOrder> DessertOrders { get; set; }
    }
}