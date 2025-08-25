using Dapper;
using System.Data;

namespace Agendor.Infra.Data.Dapper
{
    public sealed class SqliteGuidTextHandler : SqlMapper.TypeHandler<Guid>
    {
        public override void SetValue(IDbDataParameter parameter, Guid value)
        {
            parameter.Value = value.ToString("D");
            parameter.DbType = DbType.String;
        }

        public override Guid Parse(object value)
        {
            return value switch
            {
                Guid g => g,
                string s => Guid.Parse(s),
                _ => Guid.Parse(value.ToString()!)
            };
        }
    }
}
