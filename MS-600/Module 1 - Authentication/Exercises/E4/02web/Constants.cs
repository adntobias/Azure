using System.Collections.Generic;

namespace Constants
{
  public static class ProductCatalogAPI
  {
    public const string CategoryUrl = "https://localhost:5050/api/Categories";
    public const string ProductUrl = "https://localhost:5050/api/Products";
    public const string ProductReadScope = "api://99bde69a-6a5b-41bc-84a9-483c4ed5122d/Product.Read";
    public const string ProductWriteScope = "api://99bde69a-6a5b-41bc-84a9-483c4ed5122d/Product.Write";
    public const string CategoryReadScope = "api://99bde69a-6a5b-41bc-84a9-483c4ed5122d/Category.Read";
    public const string CategoryWriteScope = "api://99bde69a-6a5b-41bc-84a9-483c4ed5122d/Category.Write";

    public static List<string> SCOPES = new List<string>()
    {
      ProductReadScope, ProductWriteScope, CategoryReadScope, CategoryWriteScope
    };
  }

  public static class ClaimIds
  {
    public const string UserObjectId = "http://schemas.microsoft.com/identity/claims/objectidentifier";
    public const string TenantId = "http://schemas.microsoft.com/identity/claims/tenantid";
  }
}