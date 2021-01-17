# Refactored API
The attached project is a refactor of the RefactorThis API. 

## Author
Frank Fajardo

## What has been refactored
- The single controller from the RefactorThis API was split into 2 controllers: (1) ProductsController, and (2) ProductOptionsController. This was to allow for the classes to be more focused.
- An Error Controller was added, based on ASP.NET 5's way of tackling unhandled exceptions. The /error-local-development endpoint, which is served on during Development mode, adds the stack trace to the info it returns.
- Endpoints were added which were missing from the original.
- The endpoints were changed to return IActionResult so we can also return HTTP error status codes.
- The POST endpoints were changed to return a response header with location of the inserted resource. It also returns the current representation of the inserted resource.
- Type checks were addded to Controller routes to enable the routes for ProductOptions to be split from Products.
- Code in the controller which relate to manipulating data were moved to Repository classes so that the Controllers remain as just controllers
- Model classes were made plain classes with almost no logic. Logic relating to saving and deleting were moved to Repository classes.
- Logic for reading data and checking for entity relationships were modified to be more consistent.
- Database connection were properly closed and disposed. Execution result readers were property disposed.
- SQL string interpolation which were prone to SQL Injection attacks were removed.
- SQL connection string was moved from class code to appsettings.json

## Additional Changes
- During deletion of a product, all product options for that product are also removed with the product. This is done in one transaction.
- Moved the API to .NET 5 since .NET 2.2 is no longer supported
- Swagger was enabled on Development mode. This also helps with manual testing during development.

## What has not been done
- No tests were added
- The API was left as a single project since it was too small to warrant a multi-project approach
- The database was left as is.

## API Endpoints

1. `GET /products` - gets all products.
2. `GET /products?name={name}` - finds all products matching the specified name.
3. `GET /products/{id}` - gets the project that matches the specified ID - ID is a GUID.
4. `POST /products` - creates a new product.
5. `PUT /products/{id}` - updates a product.
6. `DELETE /products/{id}` - deletes a product and its options.
7. `GET /products/{id}/options` - finds all options for a specified product.
8. `GET /products/{id}/options/{optionId}` - finds the specified product option for the specified product.
9. `POST /products/{id}/options` - adds a new product option to the specified product.
10. `PUT /products/{id}/options/{optionId}` - updates the specified product option.
11. `DELETE /products/{id}/options/{optionId}` - deletes the specified product option.

All models are specified in the `/Models` folder, but should conform to:

**Product:**
```
{
  "Id": "01234567-89ab-cdef-0123-456789abcdef",
  "Name": "Product name",
  "Description": "Product description",
  "Price": 123.45,
  "DeliveryPrice": 12.34
}
```

**Products:**
```
{
  "Items": [
    {
      // product
    },
    {
      // product
    }
  ]
}
```

**Product Option:**
```
{
  "Id": "01234567-89ab-cdef-0123-456789abcdef",
  "Name": "Product name",
  "Description": "Product description"
}
```

**Product Options:**
```
{
  "Items": [
    {
      // product option
    },
    {
      // product option
    }
  ]
}
```
