using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

public class CustomODataConventionModelBuilder : ODataConventionModelBuilder
{
    public CustomODataConventionModelBuilder()
        : base()
    {
    }

    public override IEdmModel GetEdmModel()
    {
        var entityTypeProperties = new List<(string EntitySetName, string PropertyName)>();
        var complexTypeProperties = new List<(string ComplexName, string PropertyName)>();

        var processedComplexTypes = new HashSet<Type>();

        foreach (var entitySet in this.EntitySets)
        {
            foreach (var property in entitySet.EntityType.ClrType.GetProperties().Where(p => p.PropertyType.IsAssignableTo(typeof(JsonNode))))
            {
                entitySet.EntityType.RemoveProperty(property);
                entityTypeProperties.Add((entitySet.Name, property.Name));
            }
        }

        foreach (var complexType in this.StructuralTypes.Where(st => st.Kind == EdmTypeKind.Complex))
        {
            foreach (var property in complexType.ClrType.GetProperties().Where(p => p.PropertyType.IsAssignableTo(typeof(JsonNode))))
            {
                complexType.RemoveProperty(property);
                complexTypeProperties.Add((complexType.FullName, property.Name));
            }
        }

        var model = base.GetEdmModel();

        foreach (var entityTypeProperty in entityTypeProperties)
        {
            var entitySet = model.EntityContainer.FindEntitySet(entityTypeProperty.EntitySetName);
            if (entitySet != null)
            {
                var entityType = (EdmEntityType)entitySet.EntityType;
                entityType.AddStructuralProperty(entityTypeProperty.PropertyName, EdmCoreModel.Instance.GetUntyped(true));
            }
        }

        foreach (var complexTypeProperty in complexTypeProperties)
        {
            var complexType = (EdmComplexType)model.FindType(complexTypeProperty.ComplexName);

            if(complexType != null)
            {
                complexType.AddStructuralProperty(complexTypeProperty.PropertyName, EdmCoreModel.Instance.GetUntyped(true));
            }
        }

        return model;
    }
}