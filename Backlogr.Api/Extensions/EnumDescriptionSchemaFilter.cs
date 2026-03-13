using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Backlogr.Api.Extensions;

public sealed class EnumDescriptionSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        var enumType = Nullable.GetUnderlyingType(context.Type) ?? context.Type;

        if (!enumType.IsEnum)
        {
            return;
        }

        var names = Enum.GetNames(enumType);
        var values = Enum.GetValues(enumType)
            .Cast<object>()
            .Select(value => Convert.ToInt32(value));

        var pairs = names.Zip(values, (name, value) => $"{name} = {value}");
        var allowedValuesText = string.Join(", ", pairs);

        schema.Description = string.IsNullOrWhiteSpace(schema.Description)
            ? $"Allowed values: {allowedValuesText}."
            : $"{schema.Description}{Environment.NewLine}Allowed values: {allowedValuesText}.";
    }
}