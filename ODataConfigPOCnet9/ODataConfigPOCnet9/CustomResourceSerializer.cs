namespace ODataConfigPOCnet9;

using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.OData;
using Microsoft.OData.Edm;
using System.Text.Json.Nodes;

public class CustomResourceSerializer : ODataResourceSerializer
{
    public CustomResourceSerializer(IODataSerializerProvider serializerProvider) : base(serializerProvider)
    {
    }

    public override object CreateUntypedPropertyValue(IEdmStructuralProperty structuralProperty,
        ResourceContext resourceContext, out IEdmTypeReference actualType)
    {
        var o = base.CreateUntypedPropertyValue(structuralProperty, resourceContext, out actualType);

        if (o is JsonNode jsonObject)
        {
            return new ODataProperty() { 
                Name = structuralProperty.Name,
                Value = new ODataUntypedValue { RawValue = jsonObject.ToJsonString() } 
            };
        }

        return o;
    }
}