
- **`SetDecimalPrecisionConvention`**: Extension method on `ModelBuilder` to set decimal precision globally
- **`AddAutoTruncateInterceptors`**: Extension method on `DbContextOptionsBuilder` that automatically truncates strings to their MaxLength attribute value


**Pagination:**
- `PageSize`: number of items per page (PageSize 0 will return all items)
- `Page`: **1-based** (Page 0 and 1 will return first page)


**Filtering** 

- Use `Q` property for general text search across multiple fields (typically IHasTitle/IHasDescription)
- **QKeywordHelper** provides wildcard support:
  - `Parse(string)` - Splits input by spaces and returns `ParsedKeywordCollection` of `QKeyword` objects
  - `ParseKeyword(string)` - Parses a single keyword and returns a `QKeyword` object
  - Wildcard character defaults: `*` in input, `%` in output (for SQL LIKE)
  - Each `QKeyword` provides:
    - `QW` - Normalized keyword with wildcards on both sides (use for "contains" searches)
    - `Q` - Normalized keyword with wildcards only if provided in input
    - `StartsWith` - Normalized keyword with wildcard at end only
    - `EndsWith` - Normalized keyword with wildcard at start only
    - `Normalized` - Normalized keyword without wildcards
  - Inject `IQKeywordHelper` into your filter class to use these methods
  - Use `EF.Functions.Like()` for SQL LIKE queries with wildcard keywords
- FilterHasNormalizedContentQueryBuilder