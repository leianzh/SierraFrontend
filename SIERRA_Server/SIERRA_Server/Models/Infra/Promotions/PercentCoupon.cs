﻿using SIERRA_Server.Models.EFModels;

namespace SIERRA_Server.Models.Infra.Promotions
{
	public class PercentCoupon:ICoupon
	{
        public int Percent { get; set; }
        public PercentCoupon(int percent)
        {
            Percent = percent;
        }

        public string Calculate(IEnumerable<DessertCartItem> items)
		{
            var totalPrice = items.Select(i => i.Dessert.Discounts.Any(d => d.StartAt < DateTime.Now && d.EndAt > DateTime.Now) 
            ? Math.Round((decimal)i.Specification.UnitPrice*((decimal)i.Dessert.Discounts.First().DiscountPrice/100),0, MidpointRounding.AwayFromZero) : i.Specification.UnitPrice).Sum();
            var DiscountPrice = Math.Round(totalPrice * (decimal)this.Percent/100, 0, MidpointRounding.AwayFromZero);
            return DiscountPrice.ToString();
		}
	}
}
