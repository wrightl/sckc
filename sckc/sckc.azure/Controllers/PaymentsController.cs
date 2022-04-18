﻿using Azure;
using Azure.Data.Tables;
using sckc.azure.Extensions;
using sckc.core.Models;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace sckc.azure.APIs
{
    public class PaymentsController : ApiController
    {

        [Route("api/livewebhook")]
        [HttpPost]
        public async Task<IHttpActionResult> LiveWebhook()
        {
            var secret = System.IO.File.ReadAllText(Helper.MapPath("data/stripe_webhook_live_secret.data")).Trim();
            return await this.processWebhook(true, secret);
        }

        [Route("api/testwebhook")]
        [HttpPost]
        public async Task<IHttpActionResult> TestWebhook()
        {
            var secret = System.IO.File.ReadAllText(Helper.MapPath("data/stripe_webhook_test_secret.data")).Trim();

            return await this.processWebhook(false, secret);
        }

        [Route("api/booking")]
        [HttpPost]
        public async Task<IHttpActionResult> Booking(BookingDto info)
        {
            var entity = new TableEntity(getPartitionKey(info.Event, info.Date), info.Email)
            {
                {"Name", info.Name },
                {"BookingType", "Request" },
                {"TelNo", info.TelNo },
                {"Type", (info.isLiveBooking ? "Live" : "Test") },
                {"People", info.Items.Sum(item => item.Quantity) },
                {"Date", info.Date }
            };

            if (await addToStorage(entity))
                return Ok();
            return BadRequest();
        }

        private async Task<IHttpActionResult> processWebhook(bool isLive, string secret)
        { 
            try
            {
                var json = await Request.Content.ReadAsStringAsync();

                var stripeEvent = EventUtility.ConstructEvent(
                  json,
                  Request.Headers.GetValues("Stripe-Signature").FirstOrDefault(),
                  secret
                );

                // Handle the checkout.session.completed event
                switch (stripeEvent.Type)
                {
                    case Events.CheckoutSessionCompleted:
                        var session = stripeEvent.Data.Object as Session;
                        await this.confirmBooking(isLive, session);
                        break;
                    case Events.ChargeRefunded:
                        var charge = stripeEvent.Data.Object as Charge;
                        await this.removeBooking(charge);
                        break;
                }

                return Ok();
            }
            catch (StripeException e)
            {
                return BadRequest();
            }
        }

        private async Task<bool> confirmBooking(bool isLive, Session session)
        {
            var metadata = session.Metadata;

            var date = DateTime.MinValue;
            DateTime.TryParse(metadata["Date"], out date);

            var entity = new TableEntity(getPartitionKeyFromMetadata(metadata), getRowKeyFromMetadata(metadata))
            {
                {"PaymentId", session.PaymentIntentId },
                {"Name", metadata["Name"] },
                {"BookingType", "Paid" },
                {"TelNo", metadata["TelNo"] },
                {"Type", (isLive ? "Live" : "Test") },
                {"Amount", Convert.ToDecimal(session.AmountTotal) / 100 },
                {"People", Convert.ToInt32(metadata["People"]) },
                {"Date", date.ToUniversalTime() }
            };

            return await addToStorage(entity);
        }

        private async Task<bool> addToStorage(TableEntity entity)
        { 
            var tableClient = await getTableClient("Bookings");

            try
            {
                await tableClient.UpsertEntityAsync(entity);
            }
            catch (Exception)
            {

            }

            purgeOldEntries(tableClient);

            return true;
        }

        private async Task<bool> removeBooking(Charge charge)
        {
            var tableClient = await getTableClient("Bookings");

            Pageable<TableEntity> queryResultsFilter = tableClient.Query<TableEntity>(filter: $"PaymentId eq '{charge.PaymentIntentId}'");

            if (queryResultsFilter != null && queryResultsFilter.Count() > 0)
            {
                TableEntity entity = queryResultsFilter.First();
                await tableClient.DeleteEntityAsync(entity.PartitionKey, entity.RowKey);
            }
            return true;
        }

        private async void purgeOldEntries(TableClient client)
        {
            // Delete entries more than 12 months old
            try
            {
                Pageable<TableEntity> queryResultsFilter = client.Query<TableEntity>(entity => this.filterQuery(entity));

                foreach (var entity in queryResultsFilter)
                {
                    client.DeleteEntity(entity.PartitionKey, entity.RowKey);
                }
            }
            catch
            {
            }
        }

        private bool filterQuery(TableEntity entity)
        {
            object dateString;
            entity.TryGetValue("Date", out dateString);
            if (dateString != null)
            {
                DateTime dt;
                if (DateTime.TryParse(dateString.ToString(), out dt))
                    return dt.AddYears(1) > DateTime.Now;
            }
            return false;
        }

        private async Task<TableClient> getTableClient(string tableName)
        {
            var connectionString = System.IO.File.ReadAllText(Helper.MapPath("data/azure_storage_connstring.data")).Trim();
            var client = new TableClient(connectionString, tableName);

            await client.CreateIfNotExistsAsync();
            return client;
        }

        private string getPartitionKeyFromMetadata(Dictionary<string, string> metadata)
        {
            return getPartitionKey(metadata["Event"], metadata["Date"]);
        }

        private string getPartitionKey(string Event, string Date)
        {
            return $"{Event}_{Date}".Replace("/", String.Empty);
        }

        private string getRowKeyFromMetadata(Dictionary<string, string> metadata)
        {
            return metadata["Email"];
        }
    }
}