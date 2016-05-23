﻿using Newtonsoft.Json;
using NJsonApi;
using NJsonApi.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace NJsonApiCore.Web.MVC5
{
    public class JsonApiActionFilter : IActionFilter
    {
        public bool AllowMultiple { get { return false; } }
        private readonly IJsonApiTransformer jsonApiTransformer;
        private readonly IConfiguration configuration;
        private readonly JsonSerializer serializer;

        public JsonApiActionFilter(
            IJsonApiTransformer jsonApiTransformer,
            IConfiguration configuration,
            JsonSerializer serializer)
        {
            this.jsonApiTransformer = jsonApiTransformer;
            this.configuration = configuration;
            this.serializer = serializer;
        }

        public async Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext context, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            var contentType = context.Request.Content.Headers.ContentType;

            if (contentType == null || contentType.MediaType != configuration.DefaultJsonApiMediaType)
            {
                return new HttpResponseMessage(HttpStatusCode.UnsupportedMediaType);
            }

            if (!ValidateAcceptHeader(context.Request.Headers))
            {
                return new HttpResponseMessage(HttpStatusCode.NotAcceptable);
            }

            InternalActionExecuting(context, cancellationToken);

            HttpActionExecutedContext executedContext;

            if (context.Response != null)
            {
                return context.Response;
            }

            var response = await continuation();
            executedContext = new HttpActionExecutedContext(context, null)
            {
                Response = response
            };

            InternalActionExecuted(executedContext, cancellationToken);

            return context.Response;
        }

        public virtual void InternalActionExecuting(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            // TODO Deal with POST etc

            /*
            var body = context.Request.Content.ReadAsStreamAsync().Result;
            using (var reader = new StreamReader(body))
            {
                using (var jsonReader = new JsonTextReader(reader))
                {
                    var updateDocument = serializer.Deserialize(jsonReader, typeof(UpdateDocument)) as UpdateDocument;

                    // TODO - this is required for POST operations, have get working first
                    if (updateDocument != null)
                    {
                        //context.ActionDescriptor.GetParameters().Single(x => x.ParameterBinderAttribute)

                        //var actionDescriptorForBody = context.ActionDescriptor
                        //    .GetParameters()
                        //    .Single(x => x. == BindingSource.Body);

                        //var typeInsideDeltaGeneric = actionDescriptorForBody
                        //    .ParameterType
                        //    .GenericTypeArguments
                        //    .Single();
                        //var jsonApiContext = new Context(new Uri(context.Request.RequestUri.AbsoluteUri));
                        //var transformed = jsonApiTransformer.TransformBack(updateDocument, typeInsideDeltaGeneric, jsonApiContext);
                        //context.ActionArguments.Add(actionDescriptorForBody.Name, transformed);
                        //context.ModelState.Clear();
                    }
                }
            }

            */
        }

        public virtual void InternalActionExecuted(HttpActionExecutedContext context, CancellationToken cancellationToken)
        {
            var content = context.Response.Content as ObjectContent;

            if (content == null)
            {
                return;
            }

            if (!context.Response.IsSuccessStatusCode)
            {
                // TODO - Deal with errors do GET first
                //var transformed = BadActionResultTransformer.Transform(context.Result);

                //context.Result = new ObjectResult(transformed)
                //{
                //    StatusCode = transformed.Errors.First().Status
                //};
                return;
            }

            var relationshipPaths = FindRelationshipPathsToInclude(context.Request);

            // TODO validate that there are correct relationship paths
            //if (!configuration.ValidateIncludedRelationshipPaths(relationshipPaths, responseResult.Value))
            //{
            //    context.Result = new HttpStatusCodeResult(400);
            //    return;
            //}

            var jsonApiContext = new Context(
                context.Request.RequestUri,
                relationshipPaths);
            var transformedIntoJsonApi = jsonApiTransformer.Transform(content.Value, jsonApiContext);

            context.Response = context.Request.CreateResponse(HttpStatusCode.OK, transformedIntoJsonApi, configuration.DefaultJsonApiMediaType);
        }

        private string[] FindRelationshipPathsToInclude(HttpRequestMessage request)
        {
            var result = request.GetQueryNameValuePairs().Where(x => x.Key == "include").FirstOrDefault();

            return string.IsNullOrEmpty(result.Value) ? new string[0] : result.Value.Split(',');
        }

        // TODO - Merge into NJsonApiCore and remove from MVC support libraries
        private bool ValidateAcceptHeader(HttpRequestHeaders headers)
        {
            var acceptsHeaders = headers.Accept.FirstOrDefault().MediaType;

            if (string.IsNullOrEmpty(acceptsHeaders))
            {
                return true;
            }

            return acceptsHeaders
                .Split(',')
                .Select(x => x.Trim())
                .Any(x =>
                    x == "*/*" ||
                    x == configuration.DefaultJsonApiMediaType);
        }
    }
}