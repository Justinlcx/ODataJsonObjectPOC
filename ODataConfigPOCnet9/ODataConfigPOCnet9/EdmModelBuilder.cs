using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using ODataConfigPOCnet9.Models;
using System.Linq;
using System.Reflection;

namespace ODataConfigPOCnet9
{
    public class EdmModelBuilder
    {
        // Learn more about OData Model Builder: https://learn.microsoft.com/odata/webapi/model-builder-abstract
        public static IEdmModel GetEdmModel()
        {
            var builder = new CustomODataConventionModelBuilder();
            builder.Namespace = "NS";
            builder.EntitySet<DataSet>("DataSets");
            builder.ComplexType<JsonConfig>();

            return builder.GetEdmModel();

        }
    }
}