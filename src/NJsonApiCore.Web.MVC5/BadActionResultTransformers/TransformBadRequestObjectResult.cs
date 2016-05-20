﻿using NJsonApi.Serialization.Representations;

namespace NJsonApi.Web.MVC5.BadActionResultTransformers
{
    internal class TransformBadRequestObjectResult : BaseTransformBadAction<BadRequestObjectResult>
    {
        public override Error GetError(BadRequestObjectResult result)
        {
            return new Error()
            {
                Title = $"There was a bad request for {result.Value}",
                Status = result.StatusCode.Value
            };
        }
    }
}