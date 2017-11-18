﻿using System.Threading.Tasks;
using Example.Domain.Helpers;
using LightBDD.Framework;
using LightBDD.XUnit2;
using Xunit.Abstractions;

namespace Example.LightBDD.XUnit2.Features
{
    public partial class Customer_journey
    {
        #region Emails
        private class MailBox
        {
            public async Task Then_customer_should_receive_invoice()
            {
                StepExecution.Current.IgnoreScenario("Not implemented yet");
            }

            public async Task Then_customer_should_receive_order_confirmation()
            {
            }
        }
        #endregion

        #region Basket

        private async Task Then_the_product_addition_should_be_successful()
        {
        }

        private async Task When_customer_adds_product_to_the_basket(string product)
        {
            await LongRunningOperationSimulator.SimulateAsync();
            StepExecution.Current.Bypass("Until proper api is implemented, product is added directly to the DB.");
        }

        private async Task Given_product_is_in_stock(string product)
        {
        }

        #endregion

        #region  Payment
        private void Then_payment_should_be_successful()
        {
        }

        private void When_customer_requests_to_pay()
        {
            LongRunningOperationSimulator.Simulate();
        }
        #endregion

        #region Login steps
        private void Then_the_login_operation_should_be_successful()
        {
        }

        private void When_the_user_clicks_login_button()
        {
            LongRunningOperationSimulator.Simulate();
        }

        private void Given_the_user_entered_valid_password()
        {

        }

        private void Given_the_user_entered_valid_login()
        {
        }

        private void Given_the_user_is_about_to_login()
        {
        }
        #endregion

        public Customer_journey(ITestOutputHelper output) : base(output)
        {
        }
    }
}