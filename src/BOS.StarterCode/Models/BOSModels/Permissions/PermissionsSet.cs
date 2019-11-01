using BOS.IA.Client.ClientModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BOS.StarterCode.Models.BOSModels.Permissions
{
    public class PermissionsSet : IPermissionsSet
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public Guid ModuleId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool IsDefault { get; set; }
        public SetType Type { get; set; }
        public Guid? ParentModuleId { get; set; }
    }
}
