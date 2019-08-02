using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Specialized;

namespace Elmah
{
	public class NameValueCollectionSerializer : SerializerBase<object>
    {
		private static readonly NameValueCollectionSerializer instance = new NameValueCollectionSerializer();

		public static NameValueCollectionSerializer Instance
		{
			get { return instance; }
		}

        public override object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonType = context.Reader.GetCurrentBsonType();
            if (bsonType == BsonType.Null)
            {
                context.Reader.ReadNull();
                return null;
            }

            var nvc = new NameValueCollection();
            var stringSerializer = new StringSerializer();

            context.Reader.ReadStartArray();
            while (context.Reader.ReadBsonType() != BsonType.EndOfDocument)
            {
                context.Reader.ReadStartArray();
                var key = (string)stringSerializer.Deserialize(context, args);
                var val = (string)stringSerializer.Deserialize(context, args);
                context.Reader.ReadEndArray();
                nvc.Add(key, val);
            }
            context.Reader.ReadEndArray();

            return nvc;
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            if (value == null)
            {
                context.Writer.WriteNull();
                return;
            }

            var nvc = (NameValueCollection)value;
            var stringSerializer = new StringSerializer();

            context.Writer.WriteStartArray();
            foreach (var key in nvc.AllKeys)
            {
                foreach (var val in nvc.GetValues(key))
                {
                    context.Writer.WriteStartArray();
                    stringSerializer.Serialize(context, args, key);
                    stringSerializer.Serialize(context, args, val);
                    context.Writer.WriteEndArray();
                }
            }
            context.Writer.WriteEndArray();
        }
    }
}