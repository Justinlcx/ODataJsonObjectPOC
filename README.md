# ODataJsonObjectPOC

We want to expose a property of type JsonObject (we don't know the internal structure of the JSON, which is why we wish to use a JsonObject) in our model and have it correctly serialized/deserialized as a JSON object in OData requests/responses.

## Model Builder

To achieve this, we create a [CustomODataConventionModelBuilder](https://github.com/Justinlcx/ODataJsonObjectPOC/blob/main/ODataConfigPOCnet9/ODataConfigPOCnet9/CustomODataConventionModelBuilder.cs) that lets us override the GetEdmModel() method. This allows us to remove all JsonObject properties in Complex Types and Entity Types, and add them back after building the model as untyped properties.

To make this work, we must declare the ComplexType in the ModelBuilder so the model has access to those complex types.

## Serialization

We implemented a [CustomResourceSerializer](https://github.com/Justinlcx/ODataJsonObjectPOC/blob/main/ODataConfigPOCnet9/ODataConfigPOCnet9/CustomResourceSerializer.cs) to serialize the JsonObject as an Untyped value.

Expected API Request Result :
On Get /odata/datasets
```
    "@odata.context": "https://localhost:7201/odata/$metadata#DataSets",
    "value": [
        {
            "Id": 1,
            "Name": "Customer1",
            "Data": {
                "@odata.type": "#NS.JsonConfig",
                "Value": {
                    "test": "vleu",
                    "nested": {
                        "inside": false
                    }
                }
            }
        }
    ]
  }
```

## Deserialization

We implemented a [CustomResourceDeserializer](https://github.com/Justinlcx/ODataJsonObjectPOC/blob/main/ODataConfigPOCnet9/ODataConfigPOCnet9/CustomResourceDeserializer.cs) that should be able to transform the untyped resource into its JsonObject CLR property.

This is the expected POST Payload : 

```
{
    "Id" : 3,
    "Name" : "3",
    "Data" : {
        "@odata.type" : "NS.JsonConfig",
        "Value" :
        {
	          "property1" : "value",
            "property2" : [123,345],
            "property3" : [{ "insideProperty" : false }]
		    }
    }
}
```
