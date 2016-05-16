﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NJsonApi.Exceptions;
using NJsonApi.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace NJsonApi
{
    public class Configuration : IConfiguration
    {
        private readonly Dictionary<string, IResourceMapping> resourcesMappingsByResourceType = new Dictionary<string, IResourceMapping>();
        private readonly Dictionary<Type, IResourceMapping> resourcesMappingsByType = new Dictionary<Type, IResourceMapping>();

        public string DefaultJsonApiMediaType => "application/vnd.api+json";

        public void AddMapping(IResourceMapping resourceMapping)
        {
            resourcesMappingsByResourceType[resourceMapping.ResourceType] = resourceMapping;
            resourcesMappingsByType[resourceMapping.ResourceRepresentationType] = resourceMapping;
        }

        public bool IsMappingRegistered(Type type)
        {
            if (typeof(IEnumerable).IsAssignableFrom(type) && type.GetTypeInfo().IsGenericType)
            {
                return resourcesMappingsByType.ContainsKey(type.GetGenericArguments()[0]);
            }

            return resourcesMappingsByType.ContainsKey(type);
        }

        public IResourceMapping GetMapping(Type type)
        {
            IResourceMapping mapping;
            resourcesMappingsByType.TryGetValue(type, out mapping);
            return mapping;
        }

        public IResourceMapping GetMapping(object objectGraph)
        {
            return GetMapping(Reflection.GetObjectType(objectGraph));
        }

        public bool ValidateIncludedRelationshipPaths(string[] includedPaths, object objectGraph)
        {
            var mapping = GetMapping(objectGraph);
            if (mapping == null)
            {
                throw new MissingMappingException(Reflection.GetObjectType(objectGraph));
            }
            return mapping.ValidateIncludedRelationshipPaths(includedPaths);
        }

        public JsonSerializer GetJsonSerializer()
        {
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.Converters.Add(new IsoDateTimeConverter());
            serializerSettings.Converters.Add(new StringEnumConverter() { CamelCaseText = true });
#if DEBUG
            serializerSettings.Formatting = Formatting.Indented;
#endif
            var jsonSerializer = JsonSerializer.Create(serializerSettings);
            return jsonSerializer;
        }

        public IEnumerable<IResourceMapping> All()
        {
            return resourcesMappingsByType.Values;
        }
    }
}