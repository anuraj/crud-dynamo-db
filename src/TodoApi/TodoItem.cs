using Amazon.DynamoDBv2.DataModel;

namespace TodoApi
{
    [DynamoDBTable("TodoItems")]
    public class TodoItem
    {
        [DynamoDBHashKey]
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        [DynamoDBProperty]
        public string Name { get; set; } = null!;
        [DynamoDBProperty]
        public bool IsComplete { get; set; }
    }
}