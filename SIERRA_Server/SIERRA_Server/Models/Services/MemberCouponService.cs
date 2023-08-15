﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SIERRA_Server.Models.DTOs.Promotions;
using SIERRA_Server.Models.EFModels;
using SIERRA_Server.Models.Exts;
using SIERRA_Server.Models.Infra.Promotions;
using SIERRA_Server.Models.Interfaces;
using System.Linq;
using static Microsoft.AspNetCore.Razor.Language.TagHelperMetadata;

namespace SIERRA_Server.Models.Services
{
    public class MemberCouponService
	{
		private IMemberCouponRepository _repo;
		public MemberCouponService(IMemberCouponRepository repo)
        {
            _repo = repo;
		}

        public async Task<IEnumerable<MemberCouponDto>> GetUsableCoupon(int? memberId)
        {
            var coupons = await _repo.GetUsableCoupon(memberId);
            return coupons.Select(mc => mc.ToMemberCouponDto());
        }

        public async Task<IEnumerable<MemberCouponCanNotUseDto>> GetCouponCanNotUseNow(int? memberId)
        {
            var coupons = await _repo.GetCouponCanNotUseNow(memberId);
            return coupons.Select(mc => mc.ToMemberCouponCanNotUseDto());
        }

        public async Task<string> GetCouponByCode(int memberId, string code)
        {
            var isExist = await _repo.CheckCouponExist(code);
            if (!isExist)
            {
                return "查無此優惠碼";
            }
            else
            {
                var result =await _repo.CheckHaveSame(memberId, code);
                if (result.HaveSame)
                {
                    return "已領取過此優惠券";
                }
                else
                {
                    MemberCouponCreateDto dto = new MemberCouponCreateDto();
                    dto.CouponId = result.CouponId;
                    dto.MemberId = memberId;
                    return await _repo.GetCouponByCode(dto);
                }
            }
            
        }

        public async Task<IEnumerable<MemberCouponHasUsedDto>> GetCouponHasUsed(int? memberId)
        {
            var coupons = await _repo.GetCouponHasUsed(memberId);
            return coupons;
        }

        public async Task<IEnumerable<MemberCouponCanUseDto>> GetCouponMeetCriteria(int memberId)
        {
            var coupons = await DoThisToGetCouponMeetCriteria(memberId);
            var result = coupons.Select(c => c.ToMemberCouponCanUseDto());
            return result;
        }

        public async Task<IEnumerable<MemberCouponDto>> GetIneligibleCoupon(int memberId)
        {
            var coupons = await _repo.GetUsableCoupon(memberId);
            var usableCoupons = await DoThisToGetCouponMeetCriteria(memberId);
            var ineligibleCoupons = coupons.Except(usableCoupons).Select(mc => mc.ToMemberCouponDto());
            return ineligibleCoupons;
        }
        public async Task<IEnumerable<CouponCanGetDto>> GetCouponCanGet(int memberId)
        {
            var promotionCoupons = await _repo.GetPromotionCoupons();
            var promotionCouponsId = promotionCoupons.Select(c => c.CouponId);
            var memberCouponsId = await _repo.GetAllMemberPromotionCoupon(memberId);
            var couponCanGet = promotionCoupons.ToList();
            foreach (int couponId in memberCouponsId)
            {
                if (promotionCouponsId.Contains(couponId))
                {
                    var couponShouldBeRemove = couponCanGet.Find(c => c.CouponId == couponId);
                    couponCanGet.Remove(couponShouldBeRemove);
                }
            }

            return couponCanGet.Select(c=>c.ToCouponCanGetDto());
        }
		public  bool IsMemberExist(int memberId)
		{
            var result = _repo.IsMemberExist(memberId);
            return result;
		}
		

		public bool IsMemberCouponExist(int memberCouponId)
		{
			var result = _repo.IsMemberCouponExist(memberCouponId);
			return result;
		}
		public bool IsThisMemberHaveThisCoupon(int memberId, int memberCouponId)
		{
			var result = _repo.IsThisMemberHaveThisCoupon(memberId, memberCouponId);
			return result;
		}
		public async Task<int> UseCouponAndCalculateDiscountPrice(int memberId,int memberCouponId)
		{
            var memberCoupon =await _repo.GetMemberCouponById(memberCouponId);
			var cart = await _repo.GetDessertCart(memberId);
			var cartItems = cart.DessertCartItems.ToList();

			ICoupon coupon;
			//根據coupon中的欄位new不同的類別
			if (memberCoupon.DiscountGroupId == null)
			{
				if(memberCoupon.LimitType ==null)
				{
					if (memberCoupon.DiscountType == 1)
					{
						 coupon = new ReduceCoupon((int)memberCoupon.DiscountValue);
					}else if(memberCoupon.DiscountType == 2)
					{
						 coupon =new PercentCoupon((int)memberCoupon.DiscountValue);
					}
					else
					{
						 coupon = new FreightCoupon();
					}
				}
				else
				{
					if (memberCoupon.DiscountType == 1)
					{
						 coupon = new ReduceReachPriceCoupon((int)memberCoupon.LimitValue, (int)memberCoupon.DiscountValue);
					}
					else if(memberCoupon.DiscountType == 2)
					{
						 coupon = new PercentReachPriceCoupon((int)memberCoupon.LimitValue, (int)memberCoupon.DiscountValue);
					}
					else
					{
						 coupon = new FreightReachPriceCoupon((int)memberCoupon.LimitValue);
					}
				}
			}
			else
			{
				var dessertsInDiscountGroupId = memberCoupon.DiscountGroup.DiscountGroupItems.Select(dgi => dgi.DessertId);
				if (memberCoupon.LimitType == null)
				{
					if (memberCoupon.DiscountType == 1)
					{
						 coupon = new ReduceDiscountGroupCoupon(dessertsInDiscountGroupId,(int)memberCoupon.DiscountValue);
					}
					else if (memberCoupon.DiscountType == 2)
					{
						 coupon = new PercentDiscountGroupCoupon(dessertsInDiscountGroupId, (int)memberCoupon.DiscountValue);
					}
					else
					{
						 coupon = new FreightDiscountGroupCoupon(dessertsInDiscountGroupId);
					}
				}
				else
				{
					if (memberCoupon.DiscountType == 1)
					{
						 coupon = new ReduceReachCountCoupon(dessertsInDiscountGroupId,(int)memberCoupon.DiscountValue,(int)memberCoupon.LimitValue);
					}
					else if (memberCoupon.DiscountType == 2)
					{
						 coupon = new PercentReachCountCoupon(dessertsInDiscountGroupId,(int)memberCoupon.LimitValue, (int)memberCoupon.DiscountValue);
					}
					else
					{
						 coupon = new FreightReachCountCoupon(dessertsInDiscountGroupId, (int)memberCoupon.LimitValue);
					}
				}
			}
			//ICoupon coupon = new ReduceReachCountCoupon(dessertsInDiscountGroupId, (int)memberCoupon.DiscountValue, (int)memberCoupon.LimitValue);
			var price = coupon.Calculate(cartItems);
			var totalPrice = cartItems.Select(i => i.Dessert.Discounts.Any(d => d.StartAt < DateTime.Now && d.EndAt > DateTime.Now)
			? Math.Round(i.Specification.UnitPrice * ((decimal)i.Dessert.Discounts.First().DiscountPrice / 100), 0, MidpointRounding.AwayFromZero) : i.Specification.UnitPrice).Sum();
			var result = Math.Abs(price)> totalPrice? (int)-totalPrice:price;
			if (result != 0)
			{
				_repo.RecordCouponInCart(cart, memberCouponId);
			}
			return result;
		}
        public  bool HasCouponBeenUsed(int memberCouponId)
        {
			var result = _repo.HasCouponBeenUsed(memberCouponId);
			return result;
        }
		public bool IsPromotionCouponAndReady(int memberCouponId)
		{
			var result = _repo.IsPromotionCouponAndReady(memberCouponId);
			return result;
		}
		public async Task<AddCouponResult> PlayDailyGame(int memberId)
		{
			if(!_repo.IsMemberExist(memberId))
			{
				return AddCouponResult.Fail("查無此優惠券");
			}
			if (await _repo.HasPlayedGame(memberId))
			{
				return AddCouponResult.Fail("今天已經領取過囉");
			}
			var prizes =await _repo.GetPrizes();
			List<int> prizesList=new List<int>();
			foreach (var prize in prizes)
			{
				for(int i=0;i<prize.Qty;i++)
				{
					prizesList.Add((int)prize.CouponId);
				}
			}
			Random random = new Random();
			int randomIndex = random.Next(prizesList.Count());
			int resultCouponId = prizesList[randomIndex];
			var resultCouponName =await _repo.AddCouponAndRecordMemberPlay(memberId, resultCouponId);
			return AddCouponResult.Success(resultCouponName);

		}
        public async Task<IEnumerable<DailyGameRateDto>> GetDailyGameRate()
        {
            var prizes = await _repo.GetPrizes();
			var allPrizesCount = prizes.Select(p => p.Qty).Sum();
			List<DailyGameRateDto> result = new List<DailyGameRateDto>();
			foreach(var prize in prizes)
			{
				var coupon =await _repo.FindCoupon((int)prize.CouponId);
				decimal rate = (decimal)((decimal)prize.Qty / allPrizesCount)*100;
				DailyGameRateDto dto = new DailyGameRateDto()
				{
					PrizeName = coupon.CouponName,
					Rate = rate
				};
				result.Add(dto);
            }
			return result;
        }
        private async Task<IEnumerable<MemberCoupon>> DoThisToGetCouponMeetCriteria(int memberId)
		{
			var coupons = await _repo.GetUsableCoupon(memberId);
			//先把一定可以用的優惠券加進來
			var couponsMeetCriteria = coupons.Where(mc => mc.Coupon.DiscountGroupId == null && mc.Coupon.LimitType == null).ToList();
			//列出需要檢查的
			var waitToCheck = coupons.Except(couponsMeetCriteria);
			//找出此會員的購物車
			var cart = await _repo.GetDessertCart(memberId);
			//算出總金額
			var totalPrice = cart.DessertCartItems.Select(dci => dci.Dessert.Discounts.Any(d => d.StartAt < DateTime.Now && d.EndAt > DateTime.Now)
												  ? Math.Round((decimal)(dci.Specification.UnitPrice) * ((dci.Dessert.Discounts.First().DiscountPrice) / 100), 0, MidpointRounding.AwayFromZero)
												  : dci.Specification.UnitPrice)
												  .Sum();
			//列出購物車所優商品的id
			var cartItemsDessertIds = cart.DessertCartItems.Select(dci => dci.DessertId);
			//列出id跟數量
			var dessertIdAndQtys = cart.DessertCartItems.Select(dci => new {
				Id = dci.DessertId,
				Qty = dci.Quantity
			}).ToArray();
			//檢查剩下的優惠券
			foreach (MemberCoupon coupon in waitToCheck)
			{
				if (coupon.Coupon.DiscountGroupId == null)
				{
					if (totalPrice >= coupon.Coupon.LimitValue)
					{
						couponsMeetCriteria.Add(coupon);
					}
					else continue;
				}
				else
				{
					var discountGroupDessertIds = coupon.Coupon.DiscountGroup.DiscountGroupItems.Select(dgi => dgi.DessertId);

					if (coupon.Coupon.LimitType == null)
					{
						var commonItems = cartItemsDessertIds.Intersect(discountGroupDessertIds);
						if (commonItems.Any())
						{
							couponsMeetCriteria.Add(coupon);
						}
						else continue;
						//這種方法執行效率較佳但上面的比較簡潔
						//for (int i = 0; i < discountGroupDessertIds.Length; i++)
						//{
						//    if (cartItemsDessertIds.Contains(discountGroupDessertIds[i]))
						//    {
						//        couponsMeetCriteria.Add(coupon);
						//        break;
						//    }
						//    else continue;
						//}
					}
					else
					{
						var neededCount = coupon.Coupon.LimitValue;
						var buyCount = 0;
						for (int i = 0; i < dessertIdAndQtys.Length; i++)
						{
							if (buyCount >= neededCount)
							{
								couponsMeetCriteria.Add(coupon);
								break;
							}
							else
							{
								if (discountGroupDessertIds.Contains(dessertIdAndQtys[i].Id))
								{
									buyCount += dessertIdAndQtys[i].Qty;
								}
								else continue;
							}
						}
					}
				}
			}
			return couponsMeetCriteria;
		}

       
    }
}
