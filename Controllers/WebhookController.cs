using juggle_joy.Models;
using Stripe;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
namespace juggle_joy.Controllers
{
    public class WebhookController : Controller
    {
        // This is your Stripe CLI webhook secret for testing your endpoint locally.
        const string endpointSecret = "whsec_74d9aa82f52c4e8e35ad62eb6e67d8f4dd77f57a94e11084ee73e9f6ea8fe9a0";

        [HttpPost]
        [Route("webhook")]
        public async Task<ActionResult> Index()
        {
            var json = await new StreamReader(Request.InputStream).ReadToEndAsync();
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"], endpointSecret);

                // Handle the event
                if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                {
                    // Handle successful payment intent
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;

                    var subscription = stripeEvent.Data.Object as Subscription;
                    var subscriptionId = subscription.Id;
                    var amountPaid = 0L;

                    switch (stripeEvent.Type)
                    {
                        case Events.InvoicePaymentSucceeded:
                            var invoice = stripeEvent.Data.Object as Invoice;
                             amountPaid = invoice.AmountPaid;
                            // Log subscription ID and amount paid
                            Console.WriteLine($"Subscription ID: {subscriptionId}");
                            Console.WriteLine($"Amount Paid: {amountPaid / 100.0} {invoice.Currency}");                            
                            break;

                            // Handle other events if needed
                    }

                    using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                    {
                        var user = ctx.tbl_user.Where(x => x.SubscriptionId == subscriptionId).FirstOrDefault();
                        user.Status = (int)UserStatus.IncompleteRegistration;
                        ctx.Entry(user).State = System.Data.Entity.EntityState.Modified;


                        tbl_subscription objsuscription = new tbl_subscription();
                        objsuscription.UserID = user.UserID;
                        objsuscription.SubscriptionFee = amountPaid;
                        objsuscription.TransactionNo = subscription.ApplicationId;
                        objsuscription.TransactionStatus = subscription.Status;
                        objsuscription.TransactionMsg = "Web Hook Successful";

                        objsuscription.SubscriptionDate = DateTime.Now;
                        objsuscription.StripeOrderID = subscription.ApplicationId;
                        objsuscription.ExpirationDate = DateTime.Now.AddMonths(1);
                        objsuscription.StripePaymentType = "Stripe";
                        objsuscription.StripeReturnData = "";
                        ctx.tbl_subscription.Add(objsuscription);

                        ctx.SaveChanges();
                    }

                    Console.WriteLine("PaymentIntent was successful!");
                }
                // ... handle other event types
                else
                {
                    Console.WriteLine("Unhandled event type: {0}", stripeEvent.Type);
                }

                return new HttpStatusCodeResult(200);
            }
            catch (StripeException e)
            {
                return new HttpStatusCodeResult(400);
            }
        }
    }
}