using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using NJsonApi.Serialization.Representations;

namespace NJsonApi.Web
{
    public class JsonApiErrorResult : ObjectResult
    {
        public JsonApiErrorResult(Error error) : base(null)
        {
            StatusCode = (int)HttpStatusCode.BadRequest;
            Value = new { errors = new[] { error } };
        }

        public JsonApiErrorResult(List<Error> errors) : base(null)
        {
            StatusCode = (int)HttpStatusCode.BadRequest;
            Value = new { errors };
        }
    }
}
