using BOS.Base.Client;
using BOS.IA.Client.ClientModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BOS.StarterCode.Models.BOSModels
{
    public class Operation : IOperation
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool IsDefault { get; set; }
        public bool Deleted { get; set; }
        public Guid? ParentOperationId { get; set; }
        [JsonProperty("ChildOperations", ItemConverterType = typeof(ConcreteConverter<IOperation, Operation>))]
        public List<IOperation> ChildOperations { get; set; }
        public IOperation ParentOperation { get; set; }

        public bool IsPermitted { get; set; } = false;
    }
}
