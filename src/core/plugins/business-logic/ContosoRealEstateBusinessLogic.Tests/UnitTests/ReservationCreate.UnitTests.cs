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
using Microsoft.Xrm.Sdk.Extensions;

namespace ContosoRealEstateBusinessLogic.Tests.UnitTests
{
    [TestClass]
    public class ReservationCreateTests : PluginUnitTestBase
    {

        [TestMethod]
        public void ExecuteDataversePlugin_ShouldThrowException_WhenListingIsNotAvailable()
        {
            // Arrange
            var target = new Entity(contoso_Reservation.EntityLogicalName)
            {
                Id = Guid.NewGuid()
            };
            target[contoso_Reservation.Fields.contoso_Listing] = new EntityReference(contoso_listing.EntityLogicalName, Guid.NewGuid());
            target[contoso_Reservation.Fields.contoso_From] = DateTime.Now.AddDays(1);
            target[contoso_Reservation.Fields.contoso_To] = DateTime.Now.AddDays(2);

            var (mockLocalPluginContext, mockOrganizationService) = SetupMockLocalPluginContext("Create", StageEnum.PreValidation, target);
            var plugin = new ReservationOnCreatePreValidation("", "");


            var isListingAvailableresponse = new contoso_IsListingAvailableAPIResponse();
            isListingAvailableresponse.Results["Available"] = false;

            mockOrganizationService.Setup(x => x.Execute(It.IsAny<OrganizationRequest>()))
                .Returns(isListingAvailableresponse);

            // Act
            void executePlugin() => plugin.ExecuteDataversePlugin(mockLocalPluginContext);

            // Assert
            var expectedException = Assert.ThrowsException<ListingNotAvailableException>(executePlugin);
        }


        [TestMethod]
        public void ExecuteDataversePlugin_ShouldThrowException_WhenToDateIsBeforeFromDate()
        {
            // Arrange           
            var target = new Entity(contoso_Reservation.EntityLogicalName)
            {
                Id = Guid.NewGuid()
            };
            target[contoso_Reservation.Fields.contoso_From] = DateTime.Now.AddDays(3);
            target[contoso_Reservation.Fields.contoso_To] = DateTime.Now.AddDays(1);
            target[contoso_Reservation.Fields.contoso_Listing] = new EntityReference(contoso_listing.EntityLogicalName, Guid.NewGuid());

            var (mockLocalPluginContext, mockOrganizationService) = SetupMockLocalPluginContext("Create", StageEnum.PreValidation, target);

            mockOrganizationService.Setup(x => x.Execute(It.IsAny<OrganizationRequest>()))
               .Returns((OrganizationRequest request) =>
               {
                   var response = new contoso_IsListingAvailableAPIResponse();
                   response.Results["Available"] = true;
                   return response;
               });

            var plugin = new ReservationOnCreatePreValidation("", "");

            // Act
            void executePlugin() => plugin.ExecuteDataversePlugin(mockLocalPluginContext);

            // Assert
            var expectedException = Assert.ThrowsException<ToDateMustBeAfterFromDateException>(executePlugin);
        }


        [TestMethod]
        public void ExecuteDataversePlugin_ShouldSetReservationName_WhenValidData()
        {
            var target = new Entity(contoso_Reservation.EntityLogicalName)
            {
                Id = Guid.NewGuid()
            };
            target[contoso_Reservation.Fields.contoso_From] = DateTime.Now.AddDays(1);
            target[contoso_Reservation.Fields.contoso_To] = DateTime.Now.AddDays(2);
            target[contoso_Reservation.Fields.contoso_Listing] = new EntityReference(contoso_listing.EntityLogicalName, Guid.NewGuid());

            var (mockLocalPluginContext, mockOrganizationService) = SetupMockLocalPluginContext("Create", StageEnum.PreValidation, target);

            mockOrganizationService.Setup(x => x.Execute(It.IsAny<OrganizationRequest>()))
               .Returns((OrganizationRequest request) =>
               {
                   var response = new contoso_IsListingAvailableAPIResponse();
                   response.Results["Available"] = true;
                   return response;
               });

            mockOrganizationService.Setup(x => x.Retrieve(contoso_listing.EntityLogicalName, It.IsAny<Guid>(), It.IsAny<ColumnSet>()))
                .Returns(new Entity(contoso_listing.EntityLogicalName) { [contoso_listing.Fields.contoso_name] = "Test Listing" });

            var plugin = new ReservationOnCreatePreValidation("", "");

            // Act
            plugin.ExecuteDataversePlugin(mockLocalPluginContext);

            // Assert
            Assert.AreEqual($"Test Listing - {target[contoso_Reservation.Fields.contoso_From]:yyyy/MM/dd} - {target[contoso_Reservation.Fields.contoso_To]:yyyy/MM/dd}", target[contoso_Reservation.Fields.contoso_Name]);
        }

    }
}