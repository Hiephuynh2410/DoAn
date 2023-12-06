//using Microsoft.AspNetCore.Mvc;
//using Stripe.Checkout;
//using Stripe;
//using System;
//using System.Collections.Generic;
//using DoAn.Models;

//namespace DoAn.ApiController
//{
//    [ApiController]
//    [Route("api/v1/[controller]")]
//    public class CheckoutApiController : Controller
//    {
//        private readonly DlctContext _dbContext;

//        public CheckoutApiController(DlctContext dbContext)
//        {
//            _dbContext = dbContext;
//        }
//        [HttpPost]
//        public ActionResult Create()
//        {
//            var domain = "http://localhost:7109";
//            var options = new SessionCreateOptions
//            {
//                LineItems = new List<SessionLineItemOptions>
//                {
//                    new SessionLineItemOptions
//                    {
//                        Price =  "price_1OKEAoIIWuifBMWHyTUgxBYT", 
//                        Quantity = 1,
//                    },
//                },
//                Mode = "payment", 
//                SuccessUrl = domain + "?success=true",
//                CancelUrl = domain + "?canceled=true",
//            };

//            var service = new SessionService();
//            Session session = service.Create(options);

//            Response.Headers.Add("Location", session.Url);
//            return new StatusCodeResult(303);
//        }

//    }
//}
