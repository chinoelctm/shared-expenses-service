using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using SharedExpenses.Domain.ValueObjects;

namespace SharedExpenses.Infrastructure.Persistence
{
    public class YearMonthSerializer : SerializerBase<YearMonth>
    {
        public override YearMonth Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var value = context.Reader.ReadString();
            return YearMonth.Parse(value);
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, YearMonth value)
        {
            context.Writer.WriteString(value.ToString());
        }
    }
}
