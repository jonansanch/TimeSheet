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

public class DateTimeOffsetTypeHandler : SqlMapper.TypeHandler<DateTimeOffset>
{
    public override void SetValue(IDbDataParameter parameter, DateTimeOffset value)
        => parameter.Value = value.ToString("o");

    public override DateTimeOffset Parse(object value)
        => value switch
        {
            DateTimeOffset dto => dto,
            string s           => DateTimeOffset.Parse(s, System.Globalization.CultureInfo.InvariantCulture,
                                      System.Globalization.DateTimeStyles.RoundtripKind),
            _                  => throw new InvalidCastException($"Cannot convert {value.GetType()} to DateTimeOffset")
        };
}

public class NullableDateTimeOffsetTypeHandler : SqlMapper.TypeHandler<DateTimeOffset?>
{
    public override void SetValue(IDbDataParameter parameter, DateTimeOffset? value)
    {
        if (value.HasValue)
            parameter.Value = value.Value.ToString("o");
        else
            parameter.Value = DBNull.Value;
    }

    public override DateTimeOffset? Parse(object value)
        => value is DBNull or null ? null : value switch
        {
            DateTimeOffset dto => dto,
            string s           => DateTimeOffset.Parse(s, System.Globalization.CultureInfo.InvariantCulture,
                                      System.Globalization.DateTimeStyles.RoundtripKind),
            _                  => throw new InvalidCastException($"Cannot convert {value.GetType()} to DateTimeOffset?")
        };
}
