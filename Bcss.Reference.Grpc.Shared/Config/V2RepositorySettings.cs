using System.ComponentModel.DataAnnotations;

namespace Bcss.Reference.Grpc.Shared.Config
{
    public class V2RepositorySettings
    {
        [StringLength(1000, MinimumLength = 1)]
        public string Suffix { get; set; }
    }
}