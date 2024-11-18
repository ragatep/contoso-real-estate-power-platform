// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using ContosoRealEstate.BusinessLogic.Resources;
using ContosoRealEstate.BusinessLogic.Models;
using ContosoRealEstate.BusinessLogic.Plugins;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Moq;
using System;
using ContosoRealEstate.BusinessLogic;

namespace ContosoRealEstateBusinessLogic.Tests.UnitTests
{
    [TestClass]
    public class ReserveListingApiTests : PluginUnitTestBase
    {

        [TestMethod]
        public void ExecuteDataversePlugin_ShouldThrowInvalidPluginExecutionException_WhenInputParametersAreMissing()
        {
            // Arrange
            var (mockLocalPluginContext, mockOrganizationService) = SetupMockLocalPluginContext("contoso_ReserveListingApi", StageEnum.PlatformOperation);
            var plugin = new ReserveListing(null, null);

            // Act
            void executePlugin() => plugin.ExecuteDataversePlugin(mockLocalPluginContext);

            // Assert
            var expectedException = Assert.ThrowsException<MissingInputParametersException>(executePlugin);
        }

        [TestMethod]
        public void ExecuteDataversePlugin_ShouldCreateReservation_WhenListingIsAvailable()
        {
            // Arrange
            var (mockLocalPluginContext, mockOrganizationService) = SetupMockLocalPluginContext("contoso_ReserveListingApi", StageEnum.PlatformOperation);
            mockLocalPluginContext.PluginExecutionContext.InputParameters["FromDate"] = new DateTime(2023, 10, 1);
            mockLocalPluginContext.PluginExecutionContext.InputParameters["ToDate"] = new DateTime(2023, 10, 10);
            mockLocalPluginContext.PluginExecutionContext.InputParameters["ExcludeReservation"] = Guid.Empty.ToString();
            mockLocalPluginContext.PluginExecutionContext.InputParameters["ListingID"] = Guid.NewGuid().ToString();
            mockLocalPluginContext.PluginExecutionContext.InputParameters["DataverseUserId"] = Guid.NewGuid().ToString();
            mockLocalPluginContext.PluginExecutionContext.InputParameters["Guests"] = 2;

            var listing = new contoso_listing
            {
                Id = Guid.NewGuid(),
                contoso_pricepermonth = new Money(1000)
            };

            // Mock retrieve Listing record
            mockOrganizationService.Setup(x => x.Retrieve(contoso_listing.EntityLogicalName, It.IsAny<Guid>(), It.IsAny<ColumnSet>()))
                .Returns(listing);

            // Mock IsListingAvailableAPI call
            var isListingAvailableResponse = new contoso_IsListingAvailableAPIResponse();
            isListingAvailableResponse.Results["Available"] = true;


            mockOrganizationService.Setup(x => x.Execute(It.IsAny<OrganizationRequest>()))
                .Returns(isListingAvailableResponse);


            #region Mock ListingFees Retrieve Multiple
            var totalFeesPerDayEntities = new EntityCollection
            {
                EntityName = contoso_ListingFee.EntityLogicalName,
                Entities =
                {
                    new contoso_ListingFee { Id = Guid.NewGuid(),contoso_FeeAmount = new Money(50) },
                }
            };

            var totalFeesPerDayPerGuestEntities = new EntityCollection
            {
                EntityName = contoso_ListingFee.EntityLogicalName,
                Entities =
                {
                    new contoso_ListingFee { Id = Guid.NewGuid(),contoso_FeeAmount = new Money(20) },
                    new contoso_ListingFee { Id = Guid.NewGuid(),contoso_FeeAmount = new Money(10) },
                }
            };

            mockOrganizationService.Setup(service => service.RetrieveMultiple(It.Is<QueryExpression>(
                q => q.Criteria.Conditions.Count == 2 &&
                q.Criteria.Conditions[1].AttributeName == contoso_ListingFee.Fields.contoso_PerGuest &&
                q.Criteria.Conditions[1].Operator == ConditionOperator.Equal &&
                q.Criteria.Conditions[1].Values[0].Equals(false))))
                    .Returns(totalFeesPerDayEntities);

            mockOrganizationService.Setup(service => service.RetrieveMultiple(It.Is<QueryExpression>(
                q => q.Criteria.Conditions.Count == 2 &&
                q.Criteria.Conditions[1].AttributeName == contoso_ListingFee.Fields.contoso_PerGuest &&
                q.Criteria.Conditions[1].Operator == ConditionOperator.Equal &&
                q.Criteria.Conditions[1].Values[0].Equals(true))))
                    .Returns(totalFeesPerDayPerGuestEntities);
            #endregion

            // Mock create Reservation record
            var reservation = new contoso_Reservation
            {
                Id = Guid.NewGuid(),
                contoso_ReservationNumber = "001002"
            };

            mockOrganizationService.Setup(x => x.Create(It.IsAny<Entity>()))
                .Returns(Guid.NewGuid());

            // Mock retrieve Reservation record
            mockOrganizationService.Setup(x => x.Retrieve(contoso_Reservation.EntityLogicalName, It.IsAny<Guid>(), It.IsAny<ColumnSet>()))
                .Returns(new Entity(contoso_Reservation.EntityLogicalName) { [contoso_Reservation.Fields.contoso_ReservationNumber] = reservation.contoso_ReservationNumber });

            // Mock the ReservationNumber output parameter
            mockLocalPluginContext.PluginExecutionContext.OutputParameters["ReservationNumber"] = reservation.contoso_ReservationNumber;

            var plugin = new ReserveListing(null, null);

            // Act
            plugin.ExecuteDataversePlugin(mockLocalPluginContext);

            // Assert
            Assert.IsTrue(mockLocalPluginContext.PluginExecutionContext.OutputParameters.Contains("ReservationNumber"));
        }

        [TestMethod]
        public void ExecuteDataversePlugin_ShouldThrowInvalidPluginExecutionException_WhenListingIsNotAvailable()
        {
            // Arrange
            var (mockLocalPluginContext, mockOrganizationService) = SetupMockLocalPluginContext("contoso_ReserveListingApi", StageEnum.PlatformOperation);
            mockLocalPluginContext.PluginExecutionContext.InputParameters["FromDate"] = new DateTime(2023,10,1);
            mockLocalPluginContext.PluginExecutionContext.InputParameters["ToDate"] = new DateTime(2023, 10, 10);
            mockLocalPluginContext.PluginExecutionContext.InputParameters["ExcludeReservation"] = Guid.Empty.ToString();
            mockLocalPluginContext.PluginExecutionContext.InputParameters["ListingID"] = Guid.NewGuid().ToString();
            mockLocalPluginContext.PluginExecutionContext.InputParameters["DataverseUserId"] = Guid.NewGuid().ToString();
            mockLocalPluginContext.PluginExecutionContext.InputParameters["Guests"] = 2;

            var listing = new contoso_listing
            {
                Id = Guid.NewGuid(),
            };

            // Mock retrieve Listing record
            mockOrganizationService.Setup(x => x.Retrieve(contoso_listing.EntityLogicalName, It.IsAny<Guid>(), It.IsAny<ColumnSet>()))
                .Returns(listing);

            // Mock IsListingAvailableAPI call
            var isListingAvailableResponse = new contoso_IsListingAvailableAPIResponse();
            isListingAvailableResponse.Results["Available"] = false;


            mockOrganizationService.Setup(x => x.Execute(It.IsAny<OrganizationRequest>()))
                .Returns(isListingAvailableResponse);

            var plugin = new ReserveListing(null, null);

            // Act
            void executePlugin() => plugin.ExecuteDataversePlugin(mockLocalPluginContext);

            // Assert
            var expectedException = Assert.ThrowsException<ListingNotAvailableException>(executePlugin);
        }
    }
}
