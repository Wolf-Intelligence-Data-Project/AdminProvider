//using GraphQL;
//using GraphQL.Types;
//using ProductProvider.Models;
//using ProductProvider.Interfaces.Services;
//using ProductProvider.Models.Data.Entities;

//namespace ProductProvider.GraphQL;

//public class ProductQuery : ObjectGraphType
//{
//    public ProductQuery(IProductService productService)
//    {
//        Field<IntGraphType>("productCount")
//            .Arguments(
//                new QueryArgument<ListGraphType<StringGraphType>> { Name = "businessTypes" },
//                new QueryArgument<ListGraphType<StringGraphType>> { Name = "cities" },
//                new QueryArgument<ListGraphType<StringGraphType>> { Name = "postalCodes" },
//                new QueryArgument<IntGraphType> { Name = "minRevenue" },
//                new QueryArgument<IntGraphType> { Name = "maxRevenue" },
//                new QueryArgument<IntGraphType> { Name = "minNumberOfEmployees" },
//                new QueryArgument<IntGraphType> { Name = "maxNumberOfEmployees" },
//                new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "quantityOfFiltered" }
//            )
//            .ResolveAsync(async context =>
//            {
//                var businessTypes = context.GetArgument<List<string>>("businessTypes");
//                var cities = context.GetArgument<List<string>>("cities");
//                var postalCodes = context.GetArgument<List<string>>("postalCodes");
//                var minRevenue = context.GetArgument<int?>("minRevenue");
//                var maxRevenue = context.GetArgument<int?>("maxRevenue");
//                var minNumberOfEmployees = context.GetArgument<int?>("minNumberOfEmployees");
//                var maxNumberOfEmployees = context.GetArgument<int?>("maxNumberOfEmployees");
//                var quantityOfFiltered = context.GetArgument<int>("quantityOfFiltered");

//                var filter = new ProductFilterRequest
//                {
//                    BusinessTypes = businessTypes,
//                    Cities = cities,
//                    PostalCodes = postalCodes,
//                    MinRevenue = minRevenue,
//                    MaxRevenue = maxRevenue,
//                    MinNumberOfEmployees = minNumberOfEmployees,
//                    MaxNumberOfEmployees = maxNumberOfEmployees,
//                    QuantityOfFiltered = quantityOfFiltered
//                };

//                return await productService.GetProductCountAsync(filter);
//            });
//    }

//    public class ProductType : ObjectGraphType<ProductEntity>
//    {
//        public ProductType()
//        {
//            Field(x => x.ProductId);
//            Field(x => x.CompanyName);
//            Field(x => x.BusinessType);
//            Field(x => x.Revenue);
//            Field(x => x.NumberOfEmployees);
//            Field(x => x.PhoneNumber);
//            Field(x => x.Address);
//            Field(x => x.City);
//            Field(x => x.PostalCode);
//        }
//    }
//}
