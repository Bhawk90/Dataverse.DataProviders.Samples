using Bogus;
using CrmEarlyBound;
using Gaborg.Dataverse.DataProviders.Transformations.Filters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dataverse.DataProviders.Tests
{
    [TestClass]
    public class EntityFilterTests
    {

        /// <summary>
        /// Test for filtering by using text and boolean fields with more complex, grouped conditions.
        /// </summary>
        [TestMethod]
        public void EntityFilter_FilterByComplex_ReturnsFilteredList()
        {
            /*
             * The test creates a list of accounts and filters them where:
             *  Name ends with LLC and
             *      Can be faxed and has fax number, or
             *      Can be phoned and has phone number, or
             *      Can be e-mailed and has e-mail.
             */

            // Create source entity list
            var accounts = GetAccountList();

            // Define queries to be appled on source
            var queryExpression = new QueryExpression();
            queryExpression.Criteria.AddCondition("name", ConditionOperator.EndsWith, "LLC");
            queryExpression.Criteria.FilterOperator = LogicalOperator.And;

            // Marketing filter to find accounts where any kind of contact type is allowed.
            var marketingAllowed = new FilterExpression(LogicalOperator.Or);

            // Can be faxed and has fax number
            var canBeFaxed = new FilterExpression(LogicalOperator.And);
            canBeFaxed.AddCondition("donotfax", ConditionOperator.Equal, false);
            canBeFaxed.AddCondition("fax", ConditionOperator.NotNull);

            // Can be called and has phone number
            var canBeCalled = new FilterExpression(LogicalOperator.And);
            canBeCalled.AddCondition("donotphone", ConditionOperator.Equal, false);
            canBeCalled.AddCondition("telephone1", ConditionOperator.NotNull);

            // Can be e-mailed and has e-mail
            var canBeEmailed = new FilterExpression(LogicalOperator.And);
            canBeEmailed.AddCondition("donotemail", ConditionOperator.Equal, false);
            canBeEmailed.AddCondition("emailaddress1", ConditionOperator.NotNull);

            marketingAllowed.AddFilter(canBeFaxed);
            marketingAllowed.AddFilter(canBeCalled);
            marketingAllowed.AddFilter(canBeEmailed);

            queryExpression.Criteria.AddFilter(marketingAllowed);

            // Apply filters on entity list
            IEntityFilter<Account> entityFilter = new EntityFilter<Account>();
            var filteredResults = entityFilter.FilterBy(accounts, queryExpression);

            Assert.IsTrue(
                filteredResults.All(a => 
                    a.name.Contains("LLC") &&
                    (
                        (a.donotfax == false && !String.IsNullOrWhiteSpace(a.fax)) ||
                        (a.donotemail == false && !String.IsNullOrWhiteSpace(a.telephone1)) ||
                        (a.donotphone == false && !String.IsNullOrWhiteSpace(a.emailaddress1))
                    )
                )
            );
        }

        /// <summary>
        /// Test for filtering by using text fields and simple conditions.
        /// </summary>
        [TestMethod]
        public void EntityFilter_FilterByName_ReturnsFilteredList()
        {
            // Create source entity list
            var accounts = GetAccountList();

            // Define queries to be appled on source
            var queryExpression = new QueryExpression();
            queryExpression.Criteria.AddCondition("name", ConditionOperator.EndsWith, "LLC");

            // Apply filters on entity list
            IEntityFilter<Account> entityFilter = new EntityFilter<Account>();
            var filteredResults = entityFilter.FilterBy(accounts, queryExpression);

            Assert.IsTrue(
                filteredResults.All(a =>
                    a.name.EndsWith("LLC")
                )
            );
        }

        /// <summary>
        /// Test for filtering by using OptionSet fields and simple conditions.
        /// </summary>
        [TestMethod]
        public void EntityFilter_FilterByOptionSet_ReturnsFilteredList()
        {
            // Create source entity list
            var accounts = GetAccountList();

            // Define queries to be appled on source
            var queryExpression = new QueryExpression();
            queryExpression.Criteria.AddCondition("preferredcontactmethodcode", ConditionOperator.In, Account_PreferredContactMethodCode.Email, Account_PreferredContactMethodCode.Phone);

            // Apply filters on entity list
            IEntityFilter<Account> entityFilter = new EntityFilter<Account>();
            var filteredResults = entityFilter.FilterBy(accounts, queryExpression);

            Assert.IsTrue(
                filteredResults.All(a =>
                    a.preferredcontactmethodcode == Account_PreferredContactMethodCode.Email ||
                    a.preferredcontactmethodcode == Account_PreferredContactMethodCode.Phone
                )
            );
        }

        /// <summary>
        /// Test for filtering by empty filter.
        /// </summary>
        [TestMethod]
        public void EntityFilter_EmptyFilter_ReturnsOriginalList()
        {
            // Create source entity list
            var accounts = GetAccountList();

            // Define queries to be appled on source
            var queryExpression = new QueryExpression();

            // Apply filters on entity list
            IEntityFilter<Account> entityFilter = new EntityFilter<Account>();
            var filteredResults = entityFilter.FilterBy(accounts, queryExpression);

            Assert.AreEqual(accounts.Count, filteredResults.Count);
        }

        /// <summary>
        /// Returns a list of fake accounts.
        /// </summary>
        /// <returns></returns>
        private List<Account> GetAccountList()
        {
            var accountFaker = new Faker<Account>()
                .RuleFor(a => a.accountid, f => f.Random.Guid())
                .RuleFor(a => a.name, f => $"{f.Company.CompanyName()} {f.Company.CompanySuffix()}")
                .RuleFor(a => a.accountnumber, f => String.Join("", f.Random.Digits(5)))
                .RuleFor(a => a.createdon, f => f.Date.Past(1))
                .RuleFor(a => a.telephone1, f => f.Phone.PhoneNumber())
                .RuleFor(a => a.emailaddress1, f => f.Internet.Email())
                .RuleFor(a => a.fax, f => f.Phone.PhoneNumber())
                .RuleFor(a => a.donotemail, f => f.Random.Bool())
                .RuleFor(a => a.donotfax, f => f.Random.Bool())
                .RuleFor(a => a.donotphone, f => f.Random.Bool())
                .RuleFor(a => a.preferredcontactmethodcode, f => f.PickRandom<Account_PreferredContactMethodCode>());

            return accountFaker.Generate(100);
        }
    }
}
