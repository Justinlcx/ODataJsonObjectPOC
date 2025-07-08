using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.Extensions.Logging;
using ODataConfigPOCnet9.Models;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;

namespace ODataConfigPOCnet9.Controllers
{
    [ApiController]
    public class DataSetsController : ODataController
    {
        public DataSetsController()
        {
        }

        // GET: odata/DataSets
        [HttpGet("odata/DataSets")]
        [ApiConventionMethod(typeof(DefaultApiConventions),
                     nameof(DefaultApiConventions.Get))]
        public IActionResult AllDataSets()
        { 
            
            return Ok(_dataSets);
        }

        // GET: odata/DataSets(1)
        [EnableQuery]
        [HttpGet("odata/DataSets({key})")]
        [ApiConventionMethod(typeof(DefaultApiConventions),
                     nameof(DefaultApiConventions.Get))]
        public IActionResult SingleDataSet([FromRoute] int key)
        {
            
            var dataSet = _dataSets.FirstOrDefault(c => c.Id == key);
            if (dataSet == null)
            {
                return NotFound("DataSets not found.");
            }

            return Ok(dataSet);
        }


        // POST: odata/DataSets
        [HttpPost("odata/DataSets")]
        [ApiConventionMethod(typeof(DefaultApiConventions),
                     nameof(DefaultApiConventions.Post))]
        public IActionResult AddDataSets([FromBody] Delta<DataSet> dataSet)
        {
            var ds = new DataSet();

            dataSet.Put(ds);

            if (!ModelState.IsValid && _dataSets.Any(c => c.Id == ds.Id))
            {
                return BadRequest(ModelState);
            }

            _dataSets.Add(ds);

            return Created(ds);
        }

        private static List<DataSet> _dataSets = new List<DataSet>
        {
            new DataSet
            {
                Id = 1,
                Name = "Customer1",
                
                    Data = new JsonConfig()
                    {
                        Value = new JsonObject()
                        {
                            ["test"] = "vleu",
                            ["nested"] = new JsonObject()
                            {
                                ["inside"] = false,
                            }
                        }
                    }
            }
        };
    }
}
