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

namespace ContosoRealEstateBusinessLogic.Tests.UnitTests
{
    [TestClass]
    public class ReservationUpdateTests : PluginUnitTestBase
    {

        [TestMethod]
        public void ExecuteDataversePlugin_ShouldThrowException_WhenPreImageIsMissing()
        {
            // Arrange
            var target = new Entity(contoso_Reservation.EntityLogicalName)
            {
                Id = Guid.NewGuid()
            };

            var (mockLocalPluginContext, mockOrganizationService) = SetupMockLocalPluginContext("Update", StageEnum.PreValidation, target);

            var plugin = new ReservationOnUpdatePreValidation("", "");


            // Act
            void executePlugin() => plugin.ExecuteDataversePlugin(mockLocalPluginContext);

            // Assert
            var expectedException = Assert.ThrowsException<InvalidPluginExecutionException>(executePlugin);
            Assert.AreEqual(ExceptionMessages.PREIMAGE_MISSING, expectedException.Message);

        }

        [TestMethod]
        public void ExecuteDataversePlugin_ShouldThrowException_WhenListingIsChanged()
        {
            // Arrange
            var preImage = new contoso_Reservation()
            {
                Id = Guid.NewGuid(),
                contoso_Listing = new EntityReference(contoso_listing.EntityLogicalName, Guid.NewGuid())
            };

            // Set different GUIDs for preImage and target
            var target = new contoso_Reservation()
            {
                Id = preImage.Id,
                contoso_Listing = new EntityReference(contoso_listing.EntityLogicalName, Guid.NewGuid())
            };

            var (mockLocalPluginContext, mockOrganizationService) = SetupMockLocalPluginContext("Update", StageEnum.PreValidation, target, preImage);

            var plugin = new ReservationOnUpdatePreValidation("", "");

            // Act
            void executePlugin() => plugin.ExecuteDataversePlugin(mockLocalPluginContext);

            // Assert
            var expectedException = Assert.ThrowsException<ListingCannotBeChangedException>(executePlugin);
        }


        [TestMethod]
        public void ExecuteDataversePlugin_ShouldThrowException_WhenListingIsNotAvailable()
        {
            // Arrange
            var preImage = new contoso_Reservation()
            {
                Id = Guid.NewGuid(),
                contoso_From = new DateTime(2023, 10, 1),
                contoso_To = new DateTime(2023, 10, 10),
                contoso_Listing = new EntityReference(contoso_listing.EntityLogicalName, Guid.NewGuid())
            };

            var target = new Entity(contoso_Reservation.EntityLogicalName)
            {
                Id = preImage.Id
            };

            var (mockLocalPluginContext, mockOrganizationService) = SetupMockLocalPluginContext("Update", StageEnum.PreValidation, target, preImage);

            var plugin = new ReservationOnUpdatePreValidation("", "");


            mockOrganizationService.Setup(x => x.Execute(It.IsAny<OrganizationRequest>()))
                .Returns((OrganizationRequest request) =>
                {
                    var response = new contoso_IsListingAvailableAPIResponse();
                    response.Results["Available"] = false;
                    return response;
                });

            // Act
            void executePlugin() => plugin.ExecuteDataversePlugin(mockLocalPluginContext);

            // Assert
            var expectedException = Assert.ThrowsException<ListingNotAvailableException>(executePlugin);
        }


        [TestMethod]
        public void ExecuteDataversePlugin_ShouldThrowException_WhenToDateIsBeforeFromDate()
        {
            // Arrange
            var preImage = new Entity(contoso_Reservation.EntityLogicalName)
            {
                Id = Guid.NewGuid()
            };
            preImage[contoso_Reservation.Fields.contoso_From] = DateTime.Now.AddDays(1);
            preImage[contoso_Reservation.Fields.contoso_To] = DateTime.Now.AddDays(2);
            preImage[contoso_Reservation.Fields.contoso_Listing] = new EntityReference(contoso_listing.EntityLogicalName, Guid.NewGuid());

            var target = new Entity(contoso_Reservation.EntityLogicalName)
            {
                Id = preImage.Id
            };
            target[contoso_Reservation.Fields.contoso_From] = DateTime.Now.AddDays(3);
            target[contoso_Reservation.Fields.contoso_To] = DateTime.Now.AddDays(1);

            var (mockLocalPluginContext, mockOrganizationService) = SetupMockLocalPluginContext("Update", StageEnum.PreValidation, target, preImage);

            mockOrganizationService.Setup(x => x.Execute(It.IsAny<OrganizationRequest>()))
               .Returns((OrganizationRequest request) =>
               {
                   var response = new contoso_IsListingAvailableAPIResponse();
                   response.Results["Available"] = true;
                   return response;
               });

            var plugin = new ReservationOnUpdatePreValidation("", "");

            // Act
            void executePlugin() => plugin.ExecuteDataversePlugin(mockLocalPluginContext);

            // Assert
            var expectedException = Assert.ThrowsException<ToDateMustBeAfterFromDateException>(executePlugin);
        }


        [TestMethod]
        public void ExecuteDataversePlugin_ShouldUpdateReservationName_WhenValidData()
        {
            var preImage = new Entity(contoso_Reservation.EntityLogicalName)
            {
                Id = Guid.NewGuid()
            };
            preImage[contoso_Reservation.Fields.contoso_From] = DateTime.Now.AddDays(1);
            preImage[contoso_Reservation.Fields.contoso_To] = DateTime.Now.AddDays(2);
            preImage[contoso_Reservation.Fields.contoso_Listing] = new EntityReference(contoso_listing.EntityLogicalName, Guid.NewGuid());

            var target = new Entity(contoso_Reservation.EntityLogicalName)
            {
                Id = preImage.Id
            };
            target[contoso_Reservation.Fields.contoso_From] = DateTime.Now.AddDays(3);
            target[contoso_Reservation.Fields.contoso_To] = DateTime.Now.AddDays(4);

            var (mockLocalPluginContext, mockOrganizationService) = SetupMockLocalPluginContext("Update", StageEnum.PreValidation, target, preImage);

            mockOrganizationService.Setup(x => x.Execute(It.IsAny<OrganizationRequest>()))
               .Returns((OrganizationRequest request) =>
               {
                   var response = new contoso_IsListingAvailableAPIResponse();
                   response.Results["Available"] = true;
                   return response;
               });

            mockOrganizationService.Setup(x => x.Retrieve(contoso_listing.EntityLogicalName, It.IsAny<Guid>(), It.IsAny<ColumnSet>()))
                .Returns(new Entity(contoso_listing.EntityLogicalName) { [contoso_listing.Fields.contoso_name] = "Test Listing" });

            var plugin = new ReservationOnUpdatePreValidation("", "");

            // Act
            plugin.ExecuteDataversePlugin(mockLocalPluginContext);

            // Assert
            Assert.AreEqual($"Test Listing - {target[contoso_Reservation.Fields.contoso_From]:yyyy-MM-dd} - {target[contoso_Reservation.Fields.contoso_To]:yyyy-MM-dd}", target[contoso_Reservation.Fields.contoso_Name]);
        }

    }
}