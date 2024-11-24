using System.Text.Json.Serialization;

namespace Crany.Shared.Models
{
    public class PackageVersionItem
    {
        [JsonPropertyName("@id")] public string Id { get; set; }

        public CatalogEntry CatalogEntry { get; set; }
    }

    public class CatalogEntry
    {
        [JsonPropertyName("@id")] public string Id { get; set; }
        [JsonPropertyName("id")] public string PackageId { get; set; }

        public string Version { get; set; }

        public string Description { get; set; }

        public string Authors { get; set; }

        public List<string> Tags { get; set; } = new();

        public List<Dependency> Dependencies { get; set; } = new();

        public DateTime Published { get; set; }

        public string LicenseUrl { get; set; } = "https://opensource.org/licenses/MIT";

        public string PackageContent { get; set; } // .nupkg dosya URI'si
    }

    public class Dependency
    {
        public string Id { get; set; }

        public string VersionRange { get; set; }
    }
}

namespace Crany.Shared.Models.RegistrationIndex
{
    public class PackageVersionRoot
    {
        [JsonPropertyName("@id")] public string Id { get; set; }

        [JsonPropertyName("@type")] public string Type { get; set; } = "catalog:CatalogRoot";

        public string CommitId { get; set; }

        public DateTime CommitTimeStamp { get; set; }

        public int Count { get; set; }

        public List<PackageVersionGroup> Items { get; set; } = new();
    }

    public class PackageVersionGroup
    {
        [JsonPropertyName("@id")] public string Id { get; set; }

        public int Count { get; set; }

        public string Lower { get; set; } // En düşük sürüm
        public string Upper { get; set; } // En yüksek sürüm
        public string Parent { get; set; } // Parent URI

        public List<PackageVersionItem> Items { get; set; } = new();
    }
}

namespace Crany.Shared.Models.PackageInfos
{
    public class PackageVersionRoot
    {
        [JsonPropertyName("@id")] public string Id { get; set; }

        public string Type { get; set; } = "catalog:CatalogRoot";

        public List<PackageVersionItem> Items { get; set; } = new();
    }
}

namespace Crany.Shared.Models.RegistrationLeaf
{
    public class PackageVersionRoot
    {
        [JsonPropertyName("@id")] public string Id { get; set; }

        public string PackageContent { get; set; }

        public CatalogEntry CatalogEntry { get; set; }
        public string Registration { get; set; }
    }
}