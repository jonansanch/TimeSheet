using System.Data;
using Dapper;

namespace KPG.Timesheet.Infrastructure.Data;

public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
        => parameter.Value = value.ToDateTime(TimeOnly.MinValue);

    public override DateOnly Parse(object value)
        => DateOnly.FromDateTime(Convert.ToDateTime(value));
}

public class NullableDateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly?>
{
    public override void SetValue(IDbDataParameter parameter, DateOnly? value)
    {
        if (value.HasValue)
            parameter.Value = value.Value.ToDateTime(TimeOnly.MinValue);
        else
            parameter.Value = DBNull.Value;
    }

    public override DateOnly? Parse(object value)
        => value is DBNull or null ? null : DateOnly.FromDateTime(Convert.ToDateTime(value));
}
