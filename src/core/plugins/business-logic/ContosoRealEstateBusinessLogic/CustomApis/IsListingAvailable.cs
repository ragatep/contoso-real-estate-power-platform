// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using ContosoRealEstate.BusinessLogic.Models;
using ContosoRealEstate.BusinessLogic.Resources;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Globalization;
using System.Linq;

namespace ContosoRealEstate.BusinessLogic.Plugins;

/// <summary>
/// Plugin development guide: https://docs.microsoft.com/powerapps/developer/common-data-service/plug-ins
/// Best practices and guidance: https://docs.microsoft.com/powerapps/developer/common-data-service/best-practices/business-logic/
/// </summary>
[CrmPluginRegistration("contoso_IsListingAvailableAPI")]
public class IsListingAvailable : BusinessLogicPluginBase, IPlugin
{
    public IsListingAvailable(string unsecureConfiguration, string secureConfiguration)
        : base(typeof(IsListingAvailable), unsecureConfiguration, secureConfiguration)
    {
    }

    // Entry point for custom business logic execution
    public override void ExecuteDataversePlugin(ILocalPluginContext localPluginContext)
    {
        if (localPluginContext == null)
        {
            throw new ArgumentNullException(nameof(localPluginContext));
        }

        if (UsePowerFxPlugins(localPluginContext)) return;

        ValidateCustomApiExectionContext(localPluginContext, "contoso_IsListingAvailableAPI");

        var request = MapInputParametersToRequest(localPluginContext.PluginExecutionContext.InputParameters);

        Entity reservationFound = QueryIsListingAvailable(localPluginContext, request);

        // set the output parameter
        localPluginContext.Trace(
            "Output Parameters\n" +
            "------------------\n" +
            $"Available: {reservationFound == null}");

        localPluginContext.PluginExecutionContext.OutputParameters["Available"] = reservationFound == null;

    }

    private static Entity QueryIsListingAvailable(ILocalPluginContext localPluginContext, contoso_IsListingAvailableAPIRequest request)
    {
        var service = localPluginContext.ServiceProvider.Get<IOrganizationService>();
        // check if the listing is available
        var query = new QueryExpression()
        {
            EntityName = contoso_Reservation.EntityLogicalName,
            ColumnSet = new ColumnSet(contoso_Reservation.Fields.contoso_ReservationId)
        };
        query.Criteria.AddCondition(contoso_Reservation.Fields.contoso_Listing, ConditionOperator.Equal, new Guid(request.ListingID));
        query.Criteria.AddCondition(contoso_Reservation.Fields.contoso_ReservationStatus, ConditionOperator.NotEqual, (int)contoso_reservationstatus.Cancelled);
        query.Criteria.AddCondition(contoso_Reservation.Fields.contoso_From, ConditionOperator.LessThan, request.ToDate);
        query.Criteria.AddCondition(contoso_Reservation.Fields.contoso_To, ConditionOperator.GreaterThan, request.FromDate);
        if (!string.IsNullOrEmpty(request.ExcludeReservation)) query.Criteria.AddCondition(contoso_Reservation.Fields.contoso_ReservationId, ConditionOperator.NotEqual, new Guid(request.ExcludeReservation));

        var reservations = service.RetrieveMultiple(query);
        var reservationFound = reservations.Entities.FirstOrDefault();
        return reservationFound;
    }

    private static contoso_IsListingAvailableAPIRequest MapInputParametersToRequest(ParameterCollection inputs)
    {
        // Map the keys from the inputs to create a new contoso_IsListingAvailableAPIRequest
        var request = new contoso_IsListingAvailableAPIRequest();

        if (inputs.TryGetValue<DateTime>("FromDate", out var fromValue)) request.FromDate = fromValue;
        if (inputs.TryGetValue<DateTime>("ToDate", out var toValue)) request.ToDate = toValue;
        if (inputs.TryGetValue<string>("ExcludeReservation", out var excludeReservationValue)) request.ExcludeReservation = excludeReservationValue;
        if (inputs.TryGetValue<string>("ListingID", out var listingIDValue)) request.ListingID = listingIDValue;


        // Check that ListingID, From, To are set
        if (string.IsNullOrEmpty(request.ListingID) || request.FromDate == default|| request.ToDate == default)
        {
            throw new MissingInputParametersException();
        }

        // String rather than GUID to be compatible with the Power Fx Plugin
        // Test for None because empty string cannot be passed
        request.ExcludeReservation = (string.IsNullOrEmpty(request.ExcludeReservation) || request.ExcludeReservation == "None") ? null : request.ExcludeReservation;

        // Validate ExcludeReservation Guid if set
        if (request.ExcludeReservation is not null)
        {
            if (!Guid.TryParse(request.ExcludeReservation, out _))
            {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, ExceptionMessages.INVALID_PARAMETER, "ExcludeReservation", request.ExcludeReservation));
            }
        }

        // Validate Listing ID Guid
        ValidateGuid("ListingID", request.ListingID);
  
        return request;
    }

}
