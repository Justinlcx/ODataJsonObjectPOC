using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter.Wrapper;
using Microsoft.OData;
using Microsoft.OData.Edm;
using ODataConfigPOCnet9.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Microsoft.AspNetCore.OData.Formatter.Deserialization;

/// <summary>
/// A custom resource deserializer that handles JsonObject for untyped properties.
/// </summary>
public class CustomResourceDeserializer : ODataResourceDeserializer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomResourceDeserializer"/> class.
    /// </summary>
    /// <param name="deserializerProvider">The deserializer provider to use to read inner objects.</param>
    public CustomResourceDeserializer(IODataDeserializerProvider deserializerProvider)
        : base(deserializerProvider)
    {
    }

    public override void ApplyNestedProperty(object resource, ODataNestedResourceInfoWrapper resourceInfoWrapper,
         IEdmStructuredTypeReference structuredType, ODataDeserializerContext readContext)
    {
        IEdmProperty edmProperty = structuredType.StructuredDefinition().FindProperty(resourceInfoWrapper.NestedResourceInfo.Name);
        if (edmProperty == null)
        {
            base.ApplyNestedProperty(resource, resourceInfoWrapper, structuredType, readContext);
            return;
        }

        PropertyInfo? clrProperty;

        if (resource is IDelta delta)
        {
            var deltaType = delta.GetType();
            var structuredTypeProperty = deltaType.GetProperty("StructuredType");
            var underlyingType = structuredTypeProperty?.GetValue(delta) as Type;
            clrProperty = underlyingType?.GetProperty(readContext.Model.GetClrPropertyName(edmProperty));
        }
        else
        {
            clrProperty = resource.GetType().GetProperty(readContext.Model.GetClrPropertyName(edmProperty));
        }

        if (clrProperty != null && clrProperty.PropertyType == typeof(JsonObject) && edmProperty.Type.IsUntyped())
        {
            IList<ODataItemWrapper> nestedItems = resourceInfoWrapper.NestedItems;
            foreach (ODataItemWrapper childItem in nestedItems)
            {
                if (childItem is ODataResourceWrapper resourceWrapper && resourceWrapper != null)
                {
                    JsonObject jsonObject = ConvertResourceWrapperToJsonObject(resourceWrapper, readContext);

                    if(resource is IDelta deltaInstance)
                    {
                        deltaInstance.TrySetPropertyValue(clrProperty.Name, jsonObject);
                    }
                    else 
                    {
                        clrProperty.SetValue(resource, jsonObject);
                    }
                }
            }
        }
        else
        {
            base.ApplyNestedProperty(resource, resourceInfoWrapper, structuredType, readContext);
        }
    }

    private JsonObject ConvertResourceWrapperToJsonObject(ODataResourceWrapper resourceWrapper, ODataDeserializerContext readContext)
    {
        var jsonObject = new JsonObject();

        if (resourceWrapper?.Resource?.Properties != null)
        {
            foreach (ODataProperty property in resourceWrapper.Resource.Properties)
            {
                jsonObject[property.Name] = JsonSerializer.SerializeToNode(property.Value);
            }
        }

        if (resourceWrapper?.NestedResourceInfos != null)
        {
            foreach (var nestedInfo in resourceWrapper.NestedResourceInfos)
            {
                string propertyName = nestedInfo.NestedResourceInfo.Name;

                JsonNode? nestedValue = null;

                foreach (var nestedItem in nestedInfo.NestedItems)
                {
                    if (nestedItem is ODataResourceWrapper nestedResourceWrapper)
                    {
                        nestedValue = ConvertResourceWrapperToJsonObject(nestedResourceWrapper, readContext);
                    }
                    else if (nestedItem is ODataResourceSetWrapper nestedResourceSetWrapper)
                    {
                        nestedValue = ConvertResourceSetWrapperToJsonArray(nestedResourceSetWrapper, readContext);
                    }
                }

                if (nestedValue != null)
                {
                    jsonObject[propertyName] = nestedValue;
                }
            }
        }

        return jsonObject;
    }

    private JsonArray ConvertResourceSetWrapperToJsonArray(ODataResourceSetWrapper resourceSetWrapper, ODataDeserializerContext readContext)
    {
        var jsonArray = new JsonArray();

        foreach (var resourceWrapper in resourceSetWrapper.Resources)
        {
            jsonArray.Add(ConvertResourceWrapperToJsonObject(resourceWrapper, readContext));
        }

        foreach (var item in resourceSetWrapper.Items)
        {
            if (item is ODataPrimitiveWrapper primitive)
            {
                jsonArray.Add(primitive.Value.Value);
                continue;
            }
        }

        return jsonArray;
    }
}