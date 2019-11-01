using BOS.Base.Client;
using BOS.IA.Client.ClientModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BOS.StarterCode.Models.BOSModels.Permissions
{
    public class PermissionsModule : IPermissionsModule
    {
        public Guid OwnerId { get; set; }
        public SetType Type { get; set; }
        [JsonProperty("Modules", ItemConverterType = typeof(ConcreteConverter<IPermissionsSet, PermissionsSet>))]
        public List<IPermissionsSet> Modules { get; set; }
        [JsonProperty("Operations", ItemConverterType = typeof(ConcreteConverter<IPermissionsOperation, PermissionsOperation>))]
        public List<IPermissionsOperation> Operations { get; set; }
    }
}
