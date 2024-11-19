// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using ContosoRealEstate.BusinessLogic.Resources;
using System;
using System.Runtime.Serialization;

namespace ContosoRealEstate.BusinessLogic.Plugins;

[Serializable]
public class ReservationNullException : Exception
{
    public ReservationNullException() : base(ExceptionMessages.RESERVATION_NULL) { }
    public ReservationNullException(string message) : base(message) { }
    public ReservationNullException(string message, Exception inner) : base(message, inner) { }
    protected ReservationNullException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class ToDateMustBeAfterFromDateException : Exception
{
    public ToDateMustBeAfterFromDateException() : base(ExceptionMessages.TO_DATE_MUST_BE_AFTER_FROM_DATE) { }
    public ToDateMustBeAfterFromDateException(string message) : base(message) { }
    public ToDateMustBeAfterFromDateException(string message, Exception inner) : base(message, inner) { }
    protected ToDateMustBeAfterFromDateException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class MissingInputParametersException : Exception
{
    public MissingInputParametersException() : base(ExceptionMessages.MISSING_INPUT_PARAMETERS) { }
    public MissingInputParametersException(string message) : base(message) { }
    public MissingInputParametersException(string message, Exception inner) : base(message, inner) { }
    protected MissingInputParametersException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
