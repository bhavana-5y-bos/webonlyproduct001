using BOS.Base.Client;
using BOS.IA.Client.ClientModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BOS.StarterCode.Models.BOSModels.Permissions
{

    public class PermissionsList : IPermissionsList
    {
        [JsonProperty("Permissions", ItemConverterType = typeof(ConcreteConverter<IPermissions, Permissions>))]
        public List<IPermissions> Permissions { get; set; }
    }

    public class Permissions : IPermissions
    {
        public Guid OwnerId { get; set; }
        public SetType Type { get; set; }
        [JsonProperty("Modules", ItemConverterType = typeof(ConcreteConverter<IModule, Module>))]
        public List<IModule> Modules { get; set; }
    }


}
