// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using ContosoRealEstate.BusinessLogic.Models;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System;
using Microsoft.Xrm.Sdk.Extensions;

namespace ContosoRealEstate.BusinessLogic.Plugins;

public abstract class ReservationLogic : BusinessLogicPluginBase
{
    internal ReservationLogic(Type pluginClassName, string unsecureConfiguration, string secureConfiguration) : base(pluginClassName, unsecureConfiguration, secureConfiguration)
    {
    }

    internal static void CheckForReadOnlyFields(ILocalPluginContext localPluginContext)
    {
        // Prevent changing the listing associated
        if (localPluginContext.ChangedAttributes.Contains(contoso_Reservation.Fields.contoso_Listing))
        {
            throw new ListingCannotBeChangedException();
        }
    }

    internal static void SetNameField(ILocalPluginContext localPluginContext)
    {
        var reservation = localPluginContext.MergedTarget.ToEntity<contoso_Reservation>();
        var reservationTarget = localPluginContext.Target.ToEntity<contoso_Reservation>();

        if (localPluginContext.ChangedAttributes.Contains(contoso_Reservation.Fields.contoso_From) || localPluginContext.ChangedAttributes.Contains(contoso_Reservation.Fields.contoso_To))
        {
            var service = localPluginContext.ServiceProvider.Get<IOrganizationService>();
            // Set the name if not already set
            contoso_listing listing = service.Retrieve(
                contoso_listing.EntityLogicalName,
                reservation.contoso_Listing.Id,
                new ColumnSet(contoso_listing.Fields.contoso_name))
                .ToEntity<contoso_listing>();

            var name = $"{listing.contoso_name} - {reservation.contoso_From:yyyy-MM-dd} - {reservation.contoso_To:yyyy-MM-dd}";
            localPluginContext.Trace($"Setting Name: {name}");
            reservationTarget.contoso_Name = name;
        }
    }

    internal static void ValidateFields(ILocalPluginContext localPluginContext)
    {
        var reservation = localPluginContext.MergedTarget.ToEntity<contoso_Reservation>();
        // Validate that the To is after the from date
        if (reservation.contoso_To < reservation.contoso_From)
        {
            throw new ToDateMustBeAfterFromDateException();
        }
    }

    internal static void IsSelectedListingAvailable(ILocalPluginContext localPluginContext)
    {
        var service = localPluginContext.ServiceProvider.Get<IOrganizationService>();
        var reservation = localPluginContext.MergedTarget.ToEntity<contoso_Reservation>();
        // Only exclude the reservation from teh check if we are updateing an existing one
        var reservationId = localPluginContext.PluginExecutionContext.MessageName == MessageNameEnum.Create.ToString()
                ? "" : reservation.contoso_ReservationId.ToString();
 
        // Check if the listing is available
        var isListingAvailableResponse = (contoso_IsListingAvailableAPIResponse)service.Execute(new contoso_IsListingAvailableAPIRequest
        {
            FromDate = reservation.contoso_From.Value,
            ListingID = reservation.contoso_Listing.Id.ToString(),
            ToDate = reservation.contoso_To.Value,
            ExcludeReservation = reservationId
        });

        var isListingAvailable = isListingAvailableResponse.Available;
        if (!isListingAvailable)
        {
            throw new ListingNotAvailableException();
        }
    }

    internal static void SetReservationDateOnCreate(ILocalPluginContext localPluginContext)
    {
        var reservationTarget = localPluginContext.Target.ToEntity<contoso_Reservation>();
        // Set the reservation date if not already
        if (reservationTarget.contoso_ReservationDate == null)
        {
            localPluginContext.Trace("Setting Reservation Date");
            reservationTarget.contoso_ReservationDate = DateTime.UtcNow;
        }
    }
}
