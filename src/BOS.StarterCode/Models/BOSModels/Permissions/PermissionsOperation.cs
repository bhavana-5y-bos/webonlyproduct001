using BOS.IA.Client.ClientModels;
using System;

namespace BOS.StarterCode.Models.BOSModels.Permissions
{
    public class PermissionsOperation : IPermissionsOperation
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public Guid OperationId { get; set; }
        public Guid ModuleId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool IsDefault { get; set; }
        public SetType Type { get; set; }
        public Guid? ParenOperationId { get; set; }
    }
}
