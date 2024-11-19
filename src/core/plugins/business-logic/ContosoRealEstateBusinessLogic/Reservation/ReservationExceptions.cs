// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using ContosoRealEstate.BusinessLogic.Resources;
using System;
using System.Runtime.Serialization;

namespace ContosoRealEstate.BusinessLogic.Plugins;


[Serializable]
public class ListingCannotBeChangedException : Exception
{
    public ListingCannotBeChangedException() : base(ExceptionMessages.LISTING_CANNOT_BE_CHANGED) { }
    public ListingCannotBeChangedException(string message) : base(message) { }
    public ListingCannotBeChangedException(string message, Exception inner) : base(message, inner) { }
    protected ListingCannotBeChangedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class ListingNotAvailableException : Exception
{
    public ListingNotAvailableException() : base(ExceptionMessages.LISTING_NOT_AVAILABLE) { }
    public ListingNotAvailableException(string message) : base(message) { }
    public ListingNotAvailableException(string message, Exception inner) : base(message, inner) { }
    protected ListingNotAvailableException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
