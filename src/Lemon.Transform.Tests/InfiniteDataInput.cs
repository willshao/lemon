﻿using System.Collections.Generic;

namespace Lemon.Transform.Tests
{
    public class InfiniteDataInput : AbstractDataInput
    {
        public override void Start(IDictionary<string, object> parameters = null)
        {
            long index = 0;

            var text = "";

            for(int i = 0; i < 1024; i++)
            {
                text += i;
            }

            int len = text.Length;

            while(true)
            {
                char[] characters = new char[len];

                text.CopyTo(0, characters, 0, len);

                var str = new string(characters);

                Send(new BsonDataRow(new MongoDB.Bson.BsonDocument {
                    {"id",  index},
                    {"text", str}
                }));
            }
        }
    }
}