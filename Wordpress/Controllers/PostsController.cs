using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;
using Wordpress.Models;

namespace Wordpress.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        static readonly HttpClient client = new HttpClient();
        private readonly IMapper _mapper;
        string baseUrl = "http://localhost/abhisar/wp-json/wp/v2/posts";
        string mediaUrl = "http://localhost/abhisar/wp-json/wp/v2/media";
        string username = "abhisargarg";
        string password = "KamalAbhi@11";
        int categoryId = 2;

        public PostsController(IMapper mapper)
        {
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<Post>>> GetPosts()
        {
            // Fetch posts from WordPress API
            var wordpressPosts = await FetchPosts();

            // Return only required Properties
            var posts = _mapper.Map<List<Post>>(wordpressPosts);
            return posts;
        }

        [HttpPost]
        public async Task<ActionResult<List<string>>> GeneratePost()
        {
            List<string> successpaths = new List<string>();
            string rootFolderPath = @"C:\Users\abhis\Desktop\Python\Untitled Folder";

            // Check if the root folder exists
            if (Directory.Exists(rootFolderPath))
            {
                try
                {
                    // Get all directories within the root folder
                    string[] subDirectories = Directory.GetDirectories(rootFolderPath);

                    // Iterate through each subdirectory
                    foreach (string subDirectory in subDirectories)
                    {
                        dynamic title = string.Empty;
                        dynamic content = string.Empty;
                        dynamic imagePath = string.Empty;
                        int mediaId = 0;

                        content = "<!--more-->";

                        // Example: List all files within the subdirectory
                        string[] files = Directory.GetFiles(subDirectory);
                        foreach (string file in files)
                        {
                            if (file.Contains(".txt"))
                            {
                                //Get Blog Title
                                using (StreamReader reader = new StreamReader(file))
                                {
                                    // Read first line of the file
                                    string firstLine = reader.ReadLine();

                                    if (!string.IsNullOrEmpty(firstLine) && (firstLine.Contains("Book-") || firstLine.Contains("Book -")))
                                    {
                                        // Find the index of the first "-"
                                        int indexOfFirstDash = firstLine.IndexOf('-');

                                        if (indexOfFirstDash != -1)
                                        {
                                            // Extract the title
                                            title = firstLine.Substring(indexOfFirstDash + 1).Trim();
                                        }
                                    }
                                }

                                // Get Blog Content only if Title is not null or empty
                                // Use StreamReader to open the file and read its content
                                if (!string.IsNullOrEmpty(title))
                                {
                                    using (StreamReader reader = new StreamReader(file))
                                    {
                                        // Read the entire content of the file into a string
                                        string fileContent = reader.ReadToEnd();

                                        content = content + fileContent;
                                    }
                                }
                            }
                            if (file.Contains(".jpg"))
                            {
                                imagePath = file;
                            }
                        }

                        // Upload Media only in case the title & imagePath is not null.
                        if (!string.IsNullOrEmpty(imagePath) && !string.IsNullOrEmpty(title))
                        {
                            mediaId = await UploadMedia(imagePath);
                        }
                        if (mediaId <= 0)
                        {
                            continue;
                        }

                        // Create post only When media upload is true
                        var result = await CreatePost(title, content, categoryId, mediaId);
                        if (result == true)
                        {
                            successpaths.Add(subDirectory);
                        }
                    }
                }
                catch (Exception ex)
                {
                    successpaths.Add(ex.Message);
                    return successpaths;
                }
            }

            return successpaths;
        }

        private async Task<int> UploadMedia(string imagePath)
        {
            var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            // Read image file
            byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
            ByteArrayContent imageContent = new ByteArrayContent(imageBytes);
            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");

            MultipartFormDataContent multiContent = new MultipartFormDataContent();
            multiContent.Add(imageContent, "file", Path.GetFileName(imagePath));

            HttpResponseMessage response = await client.PostAsync(mediaUrl, multiContent);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();

                JObject obj = JObject.Parse(jsonResponse);
                int mediaId = (int)obj["id"];
                return mediaId;
            }
            else
            {
                return -1; // Return -1 if upload fails
            }
        }

        private async Task<bool> CreatePost(string title, string content, int categoryId, int imageId)
        {
            var byteArray = System.Text.Encoding.ASCII.GetBytes($"{username}:{password}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            MultipartFormDataContent multiContent = new MultipartFormDataContent();

            multiContent.Add(new StringContent(title), "title");
            multiContent.Add(new StringContent(content), "content");
            multiContent.Add(new StringContent(categoryId.ToString()), "categories");
            multiContent.Add(new StringContent(imageId.ToString()), "featured_media");
            multiContent.Add(new StringContent("publish"), "status");

            HttpResponseMessage response = await client.PostAsync(baseUrl, multiContent);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Post created successfully!");
                return true;
            }
            else
            {
                Console.WriteLine($"Failed to create post. Status code: {response.StatusCode}");
                return false;
            }
        }

        private async Task<List<WordpressPost>> FetchPosts()
        {
            var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            var response = await client.GetAsync(baseUrl);
            response.EnsureSuccessStatusCode();

            var posts = await response.Content.ReadAsAsync<List<WordpressPost>>();
            return posts;
        }
    }
}
