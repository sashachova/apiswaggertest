using NUnit.Framework;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
 
namespace WordPressApiTests
{
    public class WordPressPost
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
 
        [JsonPropertyName("date")]
        public string Date { get; set; }
 
        [JsonPropertyName("date_gmt")]
        public string DateGmt { get; set; }
 
        [JsonPropertyName("guid")]
        public Guid Guid { get; set; }
 
        [JsonPropertyName("modified")]
        public string Modified { get; set; }
 
        [JsonPropertyName("modified_gmt")]
        public string ModifiedGmt { get; set; }
 
        [JsonPropertyName("slug")]
        public string Slug { get; set; }
 
        [JsonPropertyName("status")]
        public string Status { get; set; }
 
        [JsonPropertyName("type")]
        public string Type { get; set; }
 
        [JsonPropertyName("link")]
        public string Link { get; set; }
 
        [JsonPropertyName("title")]
        public Title Title { get; set; }
 
        [JsonPropertyName("content")]
        public Content Content { get; set; }
 
        [JsonPropertyName("author")]
        public int Author { get; set; }
 
        [JsonPropertyName("comment_status")]
        public string CommentStatus { get; set; }
 
        [JsonPropertyName("ping_status")]
        public string PingStatus { get; set; }
 
        [JsonPropertyName("template")]
        public string Template { get; set; }
 
        [JsonPropertyName("meta")]
        public object Meta { get; set; }
    }
 
    public class Guid
    {
        [JsonPropertyName("rendered")]
        public string Rendered { get; set; }
    }
 
    public class Title
    {
        [JsonPropertyName("rendered")]
        public string Rendered { get; set; }
    }
 
    public class Content
    {
        [JsonPropertyName("rendered")]
        public string Rendered { get; set; }
 
        [JsonPropertyName("protected")]
        public bool Protected { get; set; }
    }
 
    [TestFixture]
    public class WordPressPostLifecycleTests
    {
        private const string BaseUrl = "https://dev.emeli.in.ua/wp-json/wp/v2";
        private readonly string _credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes("admin:Engineer_123"));
        private const int PerformanceTimeout = 3000; // 3 seconds
        private HttpClient _client;
 
        [SetUp]
        public void Setup()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);
        }
 
        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
        }
 
        [OneTimeSetUp]
        public async Task BeforeAllTests()
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);
            
            var response = await client.GetAsync($"{BaseUrl}/posts");
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK), "API should be accessible");
        }
 
        [Test]
        public async Task ShouldHandleFullPostLifecycle_Create_Edit_Delete()
        {
            
            var createStartTime = DateTime.Now;
            
            var createData = new
            {
                title = "Test Lifecycle Post",
                content = "Initial content for lifecycle testing",
                status = "publish"
            };
 
            var createContent = new StringContent(
                JsonSerializer.Serialize(createData),
                Encoding.UTF8,
                "application/json"
            );
 
            var createResponse = await _client.PostAsync($"{BaseUrl}/posts", createContent);
            var createTime = (DateTime.Now - createStartTime).TotalMilliseconds;
            
            Assert.That(createTime, Is.LessThan(PerformanceTimeout), "Create operation should be fast");
            Assert.That(createResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created), "Post should be created");
 
            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var createdPost = JsonSerializer.Deserialize<WordPressPost>(createResponseContent);
            
            Assert.That(createdPost.Id, Is.GreaterThan(0), "Created post should have an ID");
            Assert.That(createdPost.Title.Rendered, Is.EqualTo(createData.title));
            Assert.That(createdPost.Content.Rendered, Does.Contain(createData.content));
            Assert.That()
 
            //add asserts
            await Task.Delay(1000);
 
            
            var editStartTime = DateTime.Now;
            
            var updateData = new
            {
                title = "Updated Lifecycle Post",
                content = "Updated content for lifecycle testing"
            };
 
            var editContent = new StringContent(
                JsonSerializer.Serialize(updateData),
                Encoding.UTF8,
                "application/json"
            );
 
            var editResponse = await _client.PutAsync($"{BaseUrl}/posts/{createdPost.Id}", editContent);
            var editTime = (DateTime.Now - editStartTime).TotalMilliseconds;
            
            Assert.That(editTime, Is.LessThan(PerformanceTimeout), "Edit operation should be fast");
            Assert.That(editResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK), "Post should be updated");
 
            var editResponseContent = await editResponse.Content.ReadAsStringAsync();
            var updatedPost = JsonSerializer.Deserialize<WordPressPost>(editResponseContent);
            
            Assert.That(updatedPost.Id, Is.EqualTo(createdPost.Id), "Post ID should remain the same");
            Assert.That(updatedPost.Title.Rendered, Is.EqualTo(updateData.title));
            Assert.That(updatedPost.Content.Rendered, Does.Contain(updateData.content));
            Assert.That(updatedPost.Modified, Is.Not.EqualTo(createdPost.Modified), "Modified date should be updated");
 
            
            var getResponse = await _client.GetAsync($"{BaseUrl}/posts/{createdPost.Id}");
            
            Assert.That(getResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK), "Should be able to get updated post");
            
            var getResponseContent = await getResponse.Content.ReadAsStringAsync();
            var retrievedPost = JsonSerializer.Deserialize<WordPressPost>(getResponseContent);
            
            Assert.That(retrievedPost.Title.Rendered, Is.EqualTo(updateData.title));
            Assert.That(retrievedPost.Content.Rendered, Does.Contain(updateData.content));
 
            
            await Task.Delay(1000);
 
            
            var deleteStartTime = DateTime.Now;
            
            var deleteResponse = await _client.DeleteAsync($"{BaseUrl}/posts/{createdPost.Id}");
            var deleteTime = (DateTime.Now - deleteStartTime).TotalMilliseconds;
            
            Assert.That(deleteTime, Is.LessThan(PerformanceTimeout), "Delete operation should be fast");
            Assert.That(deleteResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK), "Post should be deleted");
 
            
            var checkDeletedResponse = await _client.GetAsync($"{BaseUrl}/posts/{createdPost.Id}");
            
            Assert.That(checkDeletedResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound), "Deleted post should not be accessible");
 
            
            var totalTime = createTime + editTime + deleteTime;
            Console.WriteLine($"Create time: {createTime}ms");
            Console.WriteLine($"Edit time: {editTime}ms");
            Console.WriteLine($"Delete time: {deleteTime}ms");
            Console.WriteLine($"Total time: {totalTime}ms");
        }
 
        [Test]
        public async Task ShouldHandleErrorsAppropriately()
        {
            
            var nonExistentId = 999999;
            var errorData = new
            {
                title = "This should fail",
                content = "This update should fail"
            };
 
            var errorContent = new StringContent(
                JsonSerializer.Serialize(errorData),
                Encoding.UTF8,
                "application/json"
            );
 
            var errorResponse = await _client.PutAsync($"{BaseUrl}/posts/{nonExistentId}", errorContent);
            Assert.That(errorResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound));
            
            
            var invalidData = new
            {
         
                content = "This should fail due to missing title"
            };
 
            var invalidContent = new StringContent(
                JsonSerializer.Serialize(invalidData),
                Encoding.UTF8,
                "application/json"
            );
 
            var invalidResponse = await _client.PostAsync($"{BaseUrl}/posts", invalidContent);
            Assert.That(invalidResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
        }
    }
} 